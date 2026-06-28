using System.Collections.Generic;
using Ami.BroAudio;
using Chipmunk.ComponentContainers;
using Code.ETC;
using Cysharp.Threading.Tasks;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities;
using Scripts.SkillSystem.Manage;
using UnityEngine;

namespace Scripts.SkillSystem.Skills
{
    public class SwiftStrikeSkill : ActiveSkill, IMovementDataProvider
    {
        [SerializeField] private SoundID slashSound;
        [SerializeField] private MovementDataSO movementData;
        [SerializeField] private LayerMask targetLayer = ~0;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;
        [SerializeField, Min(0.1f)] private float hitRadius = 1f;
        [SerializeField, Min(1)] private int maxHitCount = 24;
        [SerializeField, Min(0f)] private float damage = 25f;
        [SerializeField] private float damageMultiplier = 1f;
        [SerializeField] private int defPierceLevel = 1;
        [SerializeField] private DamageType damageType = DamageType.MELEE;
        [SerializeField] private bool resetCooldownOnKill = true;
        [SerializeField] private GameObject dashEffectPrefab;
        [SerializeField] private Vector3 dashEffectOffset = new(0f, 0.5f, 0f);
        [SerializeField] private Vector3 dashEffectEulerOffset;
        [SerializeField, Min(0f)] private float dashEffectLifetime = 1f;
        [SerializeField] private bool parentDashEffectToOwner;

        private readonly HashSet<Entity> _hitEntities = new();

        private ISkillMovement _movement;
        private IAimProvider _aimProvider;
        private DamageCalcCompo _damageCalcCompo;
        private Collider[] _hitBuffer;
        private Vector3 _dashDirection;
        private bool _isPrepared;
        private bool _isDashing;
        private bool _subscribedKillReset;
        private bool _cooldownReset;
        private int _dashVersion;
        public MovementDataSO MovementData => movementData;

        private void Reset()
        {
            MoveType = SkillMoveType.Force;
        }

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _movement = container.GetSubclassComponent<ISkillMovement>();
            _aimProvider = container.GetSubclassComponent<IAimProvider>();
            container.TryGetComponent(out _damageCalcCompo, true);
            EnsureHitBuffer();
        }

        public override bool CanUseSkill()
        {
            return base.CanUseSkill() && _movement != null && movementData != null && TargetState != null;
        }

        public override void StartSkill()
        {
            base.StartSkill();

            FinishDash();

            _dashVersion++;
            _isPrepared = true;
            _cooldownReset = false;
            _hitEntities.Clear();
            EnsureHitBuffer();

            _dashDirection = GetDashDirection();
            _movement.SetRotation(_dashDirection);

            SubscribeKillReset();
            //BeginDash();
        }

        public override void OnSkillTrigger()
        {
            base.OnSkillTrigger();
            BeginDash();
        }

        private void BeginDash()
        {
            if (!_isPrepared || _isDashing)
                return;

            _isPrepared = false;
            _isDashing = true;

            _movement.CanMove = false;
            _movement.SetRotation(_dashDirection);
            _movement.ApplyMovementData(_dashDirection, movementData);
            PlayDashEffect();

            RunDashDamageLoop(_dashVersion).Forget();
            BroAudio.Play(slashSound, _owner.transform.position);
        }

        private void PlayDashEffect()
        {
            if (dashEffectPrefab == null)
                return;

            Quaternion effectRotation = Quaternion.LookRotation(_dashDirection, Vector3.up) *
                                        Quaternion.Euler(dashEffectEulerOffset);
            Vector3 effectPosition = _owner.transform.position + effectRotation * dashEffectOffset;
            Transform parent = parentDashEffectToOwner ? _owner.transform : null;
            GameObject effect = Instantiate(dashEffectPrefab, effectPosition, effectRotation, parent);

            ParticleSystem[] particles = effect.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particles[i].Play(true);
            }

            float lifetime = dashEffectLifetime > 0f
                ? dashEffectLifetime
                : Mathf.Max(movementData != null ? movementData.duration : 0f, 0.1f);
            Destroy(effect, lifetime);
        }

        public override void EndSkill()
        {
            base.EndSkill();
            FinishDash();
        }

        private async UniTaskVoid RunDashDamageLoop(int dashVersion)
        {
            float duration = Mathf.Max(0f, movementData.duration);
            float endTime = Time.time + duration;
            Vector3 previousPosition = _owner.transform.position;

            TryHitBetween(previousPosition, previousPosition);

            while (IsCurrentDash(dashVersion) && Time.time < endTime)
            {
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate);

                if (!IsCurrentDash(dashVersion))
                    break;

                Vector3 currentPosition = _owner.transform.position;
                TryHitBetween(previousPosition, currentPosition);
                previousPosition = currentPosition;
            }

            FinishDash(dashVersion);
        }

        private void TryHitBetween(Vector3 start, Vector3 end)
        {
            int hitCount = Physics.OverlapCapsuleNonAlloc(start, end, hitRadius, _hitBuffer, targetLayer, triggerInteraction);

            for (int i = 0; i < hitCount; i++)
            {
                Collider hitCollider = _hitBuffer[i];
                if (hitCollider == null)
                    continue;

                TryDamageTarget(hitCollider, start);
            }
        }

        private void TryDamageTarget(Collider hitCollider, Vector3 hitOrigin)
        {
            Entity targetEntity = hitCollider.GetComponentInParent<Entity>();

            if (targetEntity == null || targetEntity == _owner || targetEntity.IsDead)
                return;

            IDamageable damageable = targetEntity.GetSubclassCompo<IDamageable>();
            if (damageable == null && !hitCollider.TryGetComponent(out damageable))
                return;

            if (!_hitEntities.Add(targetEntity))
                return;

            Vector3 hitPoint = hitCollider.ClosestPoint(hitOrigin);
            Vector3 hitNormal = hitOrigin - targetEntity.HitTransform.position;
            if (hitNormal.sqrMagnitude <= 0.0001f)
                hitNormal = -_dashDirection;
            hitNormal.Normalize();

            DamageContext context = new DamageContext
            {
                DamageData = BuildDamageData(targetEntity.HitTransform),
                HitPoint = hitPoint,
                HitNormal = hitNormal,
                Source = gameObject,
                Attacker = _owner
            };

            damageable.ApplyDamage(context);
            _owner.OnAttack?.Invoke(_owner, damageable);
        }

        private DamageData BuildDamageData(Transform target)
        {
            float finalDamageMultiplier = damageMultiplier;

            if (_owner.OnDamageCalc != null)
            {
                foreach (Entity.OnDamageCalcDelegate damageCalc in _owner.OnDamageCalc.GetInvocationList())
                    finalDamageMultiplier += damageCalc(_owner, target);
            }

            if (_damageCalcCompo != null)
                return _damageCalcCompo.CalculateDamage(damage, finalDamageMultiplier, defPierceLevel, damageType);

            return new DamageData
            {
                damage = damage * finalDamageMultiplier,
                defPierceLevel = defPierceLevel,
                damageType = damageType
            };
        }

        private Vector3 GetDashDirection()
        {
            Vector3 direction = Vector3.zero;

            if (_aimProvider != null)
            {
                Vector3 aimPosition = _aimProvider.GetAimPosition(_owner.transform.position.y);
                direction = aimPosition - _owner.transform.position;
            }

            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.0001f)
                direction = _owner.transform.forward;

            direction.y = 0f;
            return direction.sqrMagnitude <= 0.0001f ? Vector3.forward : direction.normalized;
        }

        private void SubscribeKillReset()
        {
            if (!resetCooldownOnKill || _subscribedKillReset)
                return;

            _owner.OnKill += HandleOwnerKill;
            _subscribedKillReset = true;
        }

        private void UnsubscribeKillReset()
        {
            if (!_subscribedKillReset)
                return;

            _owner.OnKill -= HandleOwnerKill;
            _subscribedKillReset = false;
        }

        private void HandleOwnerKill(float exp)
        {
            if (_cooldownReset)
                return;

            _cooldownReset = true;
            ResetCooldownNextUpdate().Forget();
        }

        private async UniTaskVoid ResetCooldownNextUpdate()
        {
            await UniTask.Yield(PlayerLoopTiming.Update);

            if (this == null || _container == null)
                return;

            if (_container.TryGetComponent(out ActiveSkillComponent skillComponent) &&
                skillComponent.GetSocket(this) is ActiveSkillSocket skillSocket)
            {
                skillSocket.ReduceCooldown(cooldown);
            }
        }

        private bool IsCurrentDash(int dashVersion)
        {
            return _isDashing && _dashVersion == dashVersion;
        }

        private void FinishDash(int dashVersion)
        {
            if (!IsCurrentDash(dashVersion))
                return;

            FinishDash();
        }

        private void FinishDash()
        {
            if (!_isDashing)
            {
                _isPrepared = false;
                UnsubscribeKillReset();
                _hitEntities.Clear();
                return;
            }

            _isPrepared = false;
            _isDashing = false;
            _dashVersion++;
            _movement.CanMove = true;
            UnsubscribeKillReset();
            _hitEntities.Clear();
        }

        private void EnsureHitBuffer()
        {
            int bufferSize = Mathf.Max(1, maxHitCount);
            if (_hitBuffer == null || _hitBuffer.Length != bufferSize)
                _hitBuffer = new Collider[bufferSize];
        }
    }
}
