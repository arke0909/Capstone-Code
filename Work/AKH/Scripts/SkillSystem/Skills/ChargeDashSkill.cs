using System.Collections.Generic;
using Ami.BroAudio;
using Chipmunk.ComponentContainers;
using Code.ETC;
using Cysharp.Threading.Tasks;
using Entities;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Scripts.SkillSystem.Skills
{
    public class ChargeDashSkill : MovingSkill, IStateExitControlledSkill
    {
        [Header("Charge")]
        [SerializeField, Min(0f)] private float chargeDuration = 3f;
        [SerializeField, Min(0f)] private float directionLockDuration = 0.4f;
        [SerializeField] private bool trackTargetWhileCharging = true;
        [Tooltip("Maximum distance shared by the ground preview and the actual dash.")]
        [SerializeField, Min(0.1f)] private float previewDistance = 12f;

        [Header("Dash")]
        [SerializeField] private MovementDataSO movementData;
        [SerializeField] private bool stopOnHit = true;
        [SerializeField, Min(0f)] private float destinationStopDistance = 0.1f;

        [Header("Damage")]
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;
        [SerializeField, Min(0.1f)] private float hitRadius = 1.25f;
        [SerializeField, Min(1)] private int maxHitCount = 16;
        [SerializeField, Min(0f)] private float defaultDamage = 30f;
        [SerializeField, Min(0f)] private float damageMultiplier = 1f;
        [SerializeField, Min(0)] private int defPierceLevel = 1;
        [SerializeField] private DamageType damageType = DamageType.MELEE;
        [SerializeField] private float stunDuration;

        [Header("Presentation")]
        [SerializeField] private string chargeVfxName = "ChargeDashCharge";
        [SerializeField] private string dashVfxName = "ChargeDashDash";
        [SerializeField] private LineRenderer directionIndicator;
        [SerializeField] private Material pathIndicatorMaterial;
        [SerializeField] private Color pathBackgroundColor = new(1f, 0.03f, 0f, 0.16f);
        [ColorUsage(true, true)]
        [SerializeField] private Color pathChargeColor = new(2f, 0.08f, 0f, 0.7f);
        [SerializeField, Min(0.1f)] private float pathProjectionDepth = 1.5f;
        [SerializeField, Min(0f)] private float pathGroundOverlap = 0.05f;
        [SerializeField, Min(0f)] private float pathDrawDistance = 60f;
        [SerializeField] private SoundID chargeStartSound;
        [SerializeField] private SoundID dashSound;

        private readonly HashSet<Entity> _hitEntities = new();

        private ISkillMovement _movement;
        private IAimProvider _aimProvider;
        private DamageCalcCompo _damageCalcCompo;
        private VFXComponent _vfxComponent;
        private DecalProjector _pathBackgroundProjector;
        private DecalProjector _pathChargeProjector;
        private Material _pathBackgroundMaterial;
        private Material _pathChargeMaterial;
        private Collider[] _hitBuffer;
        private MovementDataSO _runtimeMovementData;
        private Vector3 _dashDirection;
        private Vector3 _dashStartPosition;
        private Vector3 _dashTargetPosition;
        private float _dashTargetDistance;
        private float _dashDuration;
        private float _chargeEndTime;
        private float _directionLockTime;
        private bool _isSkillActive;
        private bool _isCharging;
        private bool _isDashing;
        private int _skillVersion;

        private static readonly int ProgressId = Shader.PropertyToID("_Progress");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int DrawOrderId = Shader.PropertyToID("_DrawOrder");

        public bool IsSkillComplete { get; private set; } = true;

        private void Reset()
        {
            AnimType = SkillAnimType.ChargeDash;
            MoveType = SkillMoveType.Force;
        }

        private void OnValidate()
        {
            AnimType = SkillAnimType.ChargeDash;
            MoveType = SkillMoveType.Force;
            chargeDuration = Mathf.Max(0f, chargeDuration);
            directionLockDuration = Mathf.Clamp(directionLockDuration, 0f, chargeDuration);
            previewDistance = Mathf.Max(0.1f, previewDistance);
            destinationStopDistance = Mathf.Max(0f, destinationStopDistance);
            hitRadius = Mathf.Max(0.1f, hitRadius);
            maxHitCount = Mathf.Max(1, maxHitCount);
            defaultDamage = Mathf.Max(0f, defaultDamage);
            damageMultiplier = Mathf.Max(0f, damageMultiplier);
            defPierceLevel = Mathf.Max(0, defPierceLevel);
            pathProjectionDepth = Mathf.Max(0.1f, pathProjectionDepth);
            pathGroundOverlap = Mathf.Max(0f, pathGroundOverlap);
            pathDrawDistance = Mathf.Max(0f, pathDrawDistance);
        }

        public override void Init(ComponentContainer container)
        {
            base.Init(container);

            _movement = container.GetSubclassComponent<ISkillMovement>();
            _aimProvider = container.GetSubclassComponent<IAimProvider>();
            _vfxComponent = container.Get<VFXComponent>();
            container.TryGetComponent(out _damageCalcCompo, true);
            EnsureHitBuffer();
            EnsurePathIndicator();

            if (directionIndicator != null)
                directionIndicator.useWorldSpace = true;

            Debug.Assert(_movement != null, $"{nameof(ChargeDashSkill)} requires {nameof(ISkillMovement)}.", this);
            Debug.Assert(_aimProvider != null, $"{nameof(ChargeDashSkill)} requires {nameof(IAimProvider)}.", this);
            Debug.Assert(movementData != null, $"{nameof(ChargeDashSkill)} requires {nameof(movementData)}.", this);
            Debug.Assert(targetLayer.value != 0, $"{nameof(ChargeDashSkill)} requires {nameof(targetLayer)}.", this);
            Debug.Assert(_vfxComponent != null, $"{nameof(ChargeDashSkill)} requires {nameof(VFXComponent)}.", this);
            Debug.Assert(pathIndicatorMaterial != null,
                $"{nameof(ChargeDashSkill)} requires {nameof(pathIndicatorMaterial)} to draw its charge path.", this);
        }

        public override bool CanUseSkill()
        {
            return base.CanUseSkill() &&
                   _movement != null &&
                   _aimProvider != null &&
                   movementData != null &&
                   TargetState != null;
        }

        public override void StartSkill()
        {
            base.StartSkill();
            CancelRuntime(false);

            if (_movement == null || _aimProvider == null || movementData == null)
            {
                Debug.LogError(
                    $"{nameof(ChargeDashSkill)} cannot start. " +
                    $"Movement={_movement != null}, AimProvider={_aimProvider != null}, MovementData={movementData != null}",
                    this);
                IsSkillComplete = true;
                return;
            }

            _skillVersion++;
            _isSkillActive = true;
            _isCharging = true;
            IsSkillComplete = false;
            _hitEntities.Clear();
            EnsureHitBuffer();

            _movement.CanMove = false;
            RefreshDashPath();

            float currentTime = Time.time;
            _chargeEndTime = currentTime + chargeDuration;
            _directionLockTime = _chargeEndTime - directionLockDuration;

            PlayVFX(chargeVfxName);
            StopVFX(dashVfxName);
            SetDirectionIndicatorVisible(true);
            UpdateDirectionIndicator(0f);

            if (chargeStartSound.IsValid())
                BroAudio.Play(chargeStartSound, _owner.transform.position);

            RunChargeLoop(_skillVersion).Forget();
        }

        public override void OnSkillTrigger()
        {
            base.OnSkillTrigger();

            // 애니메이션 이벤트가 정확히 3초보다 일찍 호출되어도 준비 시간은 보장한다.
            if (_isCharging && Time.time >= _chargeEndTime)
                BeginDash(_skillVersion);
        }

        public override void EndSkill()
        {
            base.EndSkill();
            CancelRuntime(true);
        }

        private async UniTaskVoid RunChargeLoop(int skillVersion)
        {
            while (IsCurrentCharge(skillVersion) && Time.time < _chargeEndTime)
            {
                if (trackTargetWhileCharging && Time.time < _directionLockTime)
                    RefreshDashPath();

                float progress = chargeDuration <= 0f
                    ? 1f
                    : 1f - (_chargeEndTime - Time.time) / chargeDuration;
                UpdateDirectionIndicator(Mathf.Clamp01(progress));
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            if (IsCurrentCharge(skillVersion))
            {
                UpdateDirectionIndicator(1f);
                BeginDash(skillVersion);
            }
        }

        private void BeginDash(int skillVersion)
        {
            if (!IsCurrentCharge(skillVersion) || _isDashing)
                return;

            _isCharging = false;
            _isDashing = true;

            CaptureLockedDashDestination();
            SetDirectionIndicatorVisible(false);
            StopVFX(chargeVfxName);
            PlayVFX(dashVfxName);

            _movement.SetRotation(_dashDirection);

            if (_dashTargetDistance <= destinationStopDistance)
            {
                CompleteDash(skillVersion);
                return;
            }

            if (!PrepareRuntimeMovementData())
            {
                Debug.LogError($"{nameof(ChargeDashSkill)} requires a positive dash speed.", this);
                CompleteDash(skillVersion);
                return;
            }

            _movement.ApplyMovementData(_dashDirection, _runtimeMovementData);

            if (dashSound.IsValid())
                BroAudio.Play(dashSound, _owner.transform.position);

            RunDashLoop(skillVersion).Forget();
        }

        private async UniTaskVoid RunDashLoop(int skillVersion)
        {
            float endTime = Time.time + _dashDuration;
            Vector3 previousPosition = _owner.transform.position;

            if (TryHitBetween(previousPosition, previousPosition) && stopOnHit)
            {
                CompleteDash(skillVersion);
                return;
            }

            while (IsCurrentDash(skillVersion) && Time.time < endTime)
            {
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate);

                if (!IsCurrentDash(skillVersion))
                    return;

                Vector3 currentPosition = _owner.transform.position;
                bool didHit = TryHitBetween(previousPosition, currentPosition);
                bool reachedDestination = HasReachedDashDestination(currentPosition);
                previousPosition = currentPosition;

                if (reachedDestination || (didHit && stopOnHit))
                    break;
            }

            CompleteDash(skillVersion);
        }

        private bool TryHitBetween(Vector3 start, Vector3 end)
        {
            int hitCount = Physics.OverlapCapsuleNonAlloc(
                start,
                end,
                hitRadius,
                _hitBuffer,
                targetLayer,
                triggerInteraction);

            bool didHit = false;
            for (int i = 0; i < hitCount; i++)
            {
                Collider hitCollider = _hitBuffer[i];
                if (hitCollider == null)
                    continue;

                Entity target = hitCollider.GetComponentInParent<Entity>();
                if (target == null || target == _owner || target.IsDead || !_hitEntities.Add(target))
                    continue;

                IDamageable damageable = target.GetSubclassCompo<IDamageable>();
                if (damageable == null)
                    continue;

                Transform hitTransform = target.HitTransform != null ? target.HitTransform : target.transform;
                Vector3 hitPoint = hitCollider.ClosestPoint(_owner.transform.position);

                damageable.ApplyDamage(new DamageContext
                {
                    DamageData = BuildDamageData(hitTransform),
                    HitPoint = hitPoint,
                    HitNormal = -_dashDirection,
                    Source = gameObject,
                    Attacker = _owner
                });

                ApplyKnockback(target);
                _owner.OnAttack?.Invoke(_owner, damageable);
                didHit = true;
            }

            return didHit;
        }

        private DamageData BuildDamageData(Transform target)
        {
            float finalMultiplier = damageMultiplier;

            if (_owner.OnDamageCalc != null)
            {
                foreach (Entity.OnDamageCalcDelegate damageCalc in _owner.OnDamageCalc.GetInvocationList())
                    finalMultiplier += damageCalc(_owner, target);
            }

            if (_damageCalcCompo != null)
                return _damageCalcCompo.CalculateDamage(defaultDamage, finalMultiplier, defPierceLevel, damageType);

            return new DamageData
            {
                damage = defaultDamage * finalMultiplier,
                defPierceLevel = defPierceLevel,
                damageType = damageType
            };
        }

        private void CaptureLockedDashDestination()
        {
            _dashStartPosition = _owner.transform.position;
            _dashTargetPosition = _dashStartPosition + _dashDirection * _dashTargetDistance;
            _movement.SetRotation(_dashDirection);
        }

        private bool HasReachedDashDestination(Vector3 currentPosition)
        {
            Vector3 traveled = currentPosition - _dashStartPosition;
            traveled.y = 0f;

            float traveledDistance = Vector3.Dot(traveled, _dashDirection);
            return traveledDistance >= Mathf.Max(0f, _dashTargetDistance - destinationStopDistance);
        }

        private bool PrepareRuntimeMovementData()
        {
            float averageSpeed = movementData.maxSpeed * AverageCurve(movementData.moveCurve);
            if (averageSpeed <= 0.0001f)
                return false;

            // 이동 구현이 FixedUpdate 시작점의 커브 값을 사용하므로 한 틱을 보정한다.
            // 실제 종료는 목표 지점 도달 판정이 담당해 초과 이동하지 않는다.
            _dashDuration = _dashTargetDistance / averageSpeed + Time.fixedDeltaTime;
            _runtimeMovementData = _runtimeMovementData != null
                ? _runtimeMovementData
                : ScriptableObject.CreateInstance<MovementDataSO>();
            _runtimeMovementData.maxSpeed = movementData.maxSpeed;
            _runtimeMovementData.moveCurve = movementData.moveCurve;
            _runtimeMovementData.duration = _dashDuration;
            return true;
        }

        private static float AverageCurve(AnimationCurve curve)
        {
            if (curve == null || curve.length == 0)
                return 1f;

            const int samples = 16;
            float sum = 0f;
            for (int i = 0; i < samples; i++)
                sum += curve.Evaluate((i + 0.5f) / samples);

            return Mathf.Max(0.01f, sum / samples);
        }

        private void ApplyKnockback(Entity target)
        {
            target.Stun(stunDuration);
        }

        private void RefreshDashPath()
        {
            _dashStartPosition = _owner.transform.position;
            Vector3 direction = _aimProvider.GetAimPosition(_dashStartPosition.y) - _dashStartPosition;

            direction.y = 0f;
            float aimDistance = direction.magnitude;
            if (aimDistance <= 0.0001f)
            {
                direction = _owner.transform.forward;
                aimDistance = previewDistance;
            }

            direction.y = 0f;
            _dashDirection = direction.sqrMagnitude <= 0.0001f ? Vector3.forward : direction.normalized;
            _dashTargetDistance = Mathf.Min(aimDistance, previewDistance);
            _dashTargetPosition = _dashStartPosition + _dashDirection * _dashTargetDistance;
            _movement.SetRotation(_dashDirection);
        }

        private void CompleteDash(int skillVersion)
        {
            if (!IsCurrentDash(skillVersion))
                return;

            _isDashing = false;
            _movement.CancelMovementData();
            StopVFX(dashVfxName);
            IsSkillComplete = true;
        }

        private void CancelRuntime(bool restoreMovement)
        {
            _skillVersion++;
            _isSkillActive = false;
            _isCharging = false;
            _isDashing = false;
            IsSkillComplete = true;
            _hitEntities.Clear();

            SetDirectionIndicatorVisible(false);
            StopVFX(chargeVfxName);
            StopVFX(dashVfxName);

            if (_movement == null)
                return;

            _movement.CancelMovementData();
            if (restoreMovement)
                _movement.CanMove = true;
        }

        private bool IsCurrentCharge(int skillVersion)
        {
            return _isSkillActive && _isCharging && _skillVersion == skillVersion;
        }

        private bool IsCurrentDash(int skillVersion)
        {
            return _isSkillActive && _isDashing && _skillVersion == skillVersion;
        }

        private void EnsureHitBuffer()
        {
            int bufferSize = Mathf.Max(1, maxHitCount);
            if (_hitBuffer == null || _hitBuffer.Length != bufferSize)
                _hitBuffer = new Collider[bufferSize];
        }

        private void UpdateDirectionIndicator(float chargeProgress)
        {
            if (directionIndicator != null && directionIndicator.enabled)
            {
                directionIndicator.positionCount = 2;
                directionIndicator.SetPosition(0, _dashStartPosition);
                directionIndicator.SetPosition(1, _dashTargetPosition);
            }

            UpdatePathIndicator(chargeProgress);
        }

        private void SetDirectionIndicatorVisible(bool isVisible)
        {
            if (directionIndicator != null)
                directionIndicator.enabled = isVisible;

            if (_pathBackgroundProjector != null)
                _pathBackgroundProjector.enabled = isVisible;
            if (_pathChargeProjector != null)
                _pathChargeProjector.enabled = isVisible;
        }

        private void EnsurePathIndicator()
        {
            if (_pathBackgroundProjector != null || pathIndicatorMaterial == null)
                return;

            _pathBackgroundMaterial = new Material(pathIndicatorMaterial)
            {
                name = $"{pathIndicatorMaterial.name} (Charge Dash Background)"
            };
            _pathChargeMaterial = new Material(pathIndicatorMaterial)
            {
                name = $"{pathIndicatorMaterial.name} (Charge Dash Fill)"
            };

            _pathBackgroundMaterial.SetColor(ColorId, pathBackgroundColor);
            _pathBackgroundMaterial.SetFloat(ProgressId, 1f);
            _pathBackgroundMaterial.SetFloat(DrawOrderId, 0f);
            _pathChargeMaterial.SetColor(ColorId, pathChargeColor);
            _pathChargeMaterial.SetFloat(ProgressId, 0f);
            _pathChargeMaterial.SetFloat(DrawOrderId, 1f);

            _pathBackgroundProjector = CreatePathProjector("Charge Dash Path Background", _pathBackgroundMaterial);
            _pathChargeProjector = CreatePathProjector("Charge Dash Path Fill", _pathChargeMaterial);
            SetDirectionIndicatorVisible(false);
        }

        private DecalProjector CreatePathProjector(string objectName, Material material)
        {
            GameObject projectorObject = new(objectName)
            {
                layer = gameObject.layer,
                hideFlags = HideFlags.DontSave
            };
            projectorObject.transform.SetParent(transform, false);

            DecalProjector projector = projectorObject.AddComponent<DecalProjector>();
            projector.material = material;
            projector.scaleMode = DecalScaleMode.ScaleInvariant;
            projector.drawDistance = pathDrawDistance;
            projector.fadeFactor = 1f;
            projector.pivot = Vector3.zero;
            projector.enabled = false;
            return projector;
        }

        private void UpdatePathIndicator(float chargeProgress)
        {
            EnsurePathIndicator();
            if (_pathBackgroundProjector == null || !_pathBackgroundProjector.enabled)
                return;

            Vector3 path = _dashTargetPosition - _dashStartPosition;
            path.y = 0f;
            float pathLength = path.magnitude;
            if (pathLength <= 0.0001f)
            {
                SetDirectionIndicatorVisible(false);
                return;
            }

            Vector3 direction = path / pathLength;
            // BoxIndicator reveals UV.x from 1 to 0. Point local +X back at the caster
            // so progress grows from the caster toward the target.
            Vector3 localX = -direction;
            Vector3 localY = Vector3.Cross(Vector3.down, localX);
            Quaternion rotation = Quaternion.LookRotation(Vector3.down, localY);

            float projectionDepth = Mathf.Max(0.1f, pathProjectionDepth);
            float groundOverlap = Mathf.Min(pathGroundOverlap, projectionDepth * 0.5f);
            Vector3 center = (_dashStartPosition + _dashTargetPosition) * 0.5f;
            center.y = _dashStartPosition.y + projectionDepth * 0.5f - groundOverlap;

            Vector3 projectorSize = new(pathLength, hitRadius * 2f, projectionDepth);
            SetPathProjectorPose(_pathBackgroundProjector, center, rotation, projectorSize);
            SetPathProjectorPose(_pathChargeProjector, center, rotation, projectorSize);
            _pathChargeMaterial.SetFloat(ProgressId, Mathf.Clamp01(chargeProgress));
        }

        private static void SetPathProjectorPose(
            DecalProjector projector,
            Vector3 position,
            Quaternion rotation,
            Vector3 size)
        {
            projector.transform.SetPositionAndRotation(position, rotation);
            projector.size = size;
        }

        private void PlayVFX(string vfxName)
        {
            if (_vfxComponent == null || string.IsNullOrWhiteSpace(vfxName))
                return;

            Quaternion rotation = _dashDirection.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(_dashDirection, Vector3.up)
                : _owner.transform.rotation;
            _vfxComponent.PlayVFX(vfxName, _owner.transform.position, rotation);
        }

        private void StopVFX(string vfxName)
        {
            if (_vfxComponent == null || string.IsNullOrWhiteSpace(vfxName))
                return;

            _vfxComponent.StopVFX(vfxName);
        }

        private void OnDestroy()
        {
            if (_runtimeMovementData != null)
                Destroy(_runtimeMovementData);
            if (_pathBackgroundMaterial != null)
                Destroy(_pathBackgroundMaterial);
            if (_pathChargeMaterial != null)
                Destroy(_pathChargeMaterial);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.2f, 0f, 0.8f);
            Vector3 origin = transform.position;
            Vector3 direction = Application.isPlaying && _dashDirection.sqrMagnitude > 0.0001f
                ? _dashDirection
                : transform.forward;
            Gizmos.DrawWireSphere(origin, hitRadius);
            Vector3 target = origin + direction * previewDistance;
            Gizmos.DrawLine(origin, target);

            Vector3 side = Vector3.Cross(Vector3.up, direction).normalized * hitRadius;
            Gizmos.DrawLine(origin - side, target - side);
            Gizmos.DrawLine(origin + side, target + side);
            Gizmos.DrawLine(origin - side, origin + side);
            Gizmos.DrawLine(target - side, target + side);

            if (Application.isPlaying && _isDashing)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(_dashTargetPosition, Mathf.Max(0.1f, destinationStopDistance));
            }
        }
    }
}
