using Chipmunk.ComponentContainers;
using Cysharp.Threading.Tasks;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Combat.Projectiles;
using Scripts.Entities;
using SHS.Scripts;
using UnityEngine;

namespace Code.SkillSystem.Skills.RapidFire
{
    public class RapidFireBullet : MonoBehaviour, IProjectile
    {
        [SerializeField] private float hitOffset = 0f;

        [SerializeField] private bool useFirePointRotation;
        [SerializeField] private Vector3 rotationOffset = Vector3.zero;

        [SerializeField] private GameObject visualRoot;

        [SerializeField] private ParticleSystem hitEffect;
        [SerializeField] private ParticleSystem flashEffect;

        [SerializeField] private Rigidbody rigidbodyCompo;
        [SerializeField] private TrailRenderer trail;

        [field: SerializeField]
        public PoolItemSO PoolItem { get; private set; }

        public GameObject GameObject => gameObject;

        private Pool _myPool;

        private Entity _owner;
        private Collider _collider;
        private DamageCalcCompo _damageCalcCompo;

        private RapidFireSkill _sourceSkill;
        private IProjectileShooter _projectileShooter;

        private Vector3 _previousPosition;
        private Vector3 _spawnPosition;

        private float _maxTravelDistance;

        private bool _isReturningToPool;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        private void FixedUpdate()
        {
            CheckMaxTravelDistance();

            _previousPosition = transform.position;
        }

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        public void SetSourceSkill(RapidFireSkill sourceSkill)
        {
            _sourceSkill = sourceSkill;
        }

        public void ResetItem()
        {
            if (_collider != null)
            {
                _collider.excludeLayers = 0;
                _collider.enabled = true;
            }

            _owner = null;
            _damageCalcCompo = null;
            _sourceSkill = null;
            _projectileShooter = null;

            _previousPosition = transform.position;
            _spawnPosition = transform.position;

            _maxTravelDistance = 0f;
            _isReturningToPool = false;

            if (visualRoot != null)
                visualRoot.SetActive(true);

            if (rigidbodyCompo != null)
            {
                rigidbodyCompo.linearVelocity = Vector3.zero;
                rigidbodyCompo.angularVelocity = Vector3.zero;
            }

            if (trail != null)
            {
                trail.emitting = true;
                trail.Clear();
            }

            ResetEffect(hitEffect);
            ResetEffect(flashEffect);
        }

        public void InitProjectile(
            Entity owner,
            IProjectileShooter projectileShooter,
            Vector3 initPos,
            Vector3 direction,
            LayerMask excludeLayer)
        {
            _owner = owner;
            _projectileShooter = projectileShooter;

            _damageCalcCompo = owner.Get<DamageCalcCompo>();

            _previousPosition = initPos;
            _spawnPosition = initPos;

            _maxTravelDistance =
                projectileShooter.ProjectileMaxRange;

            _isReturningToPool = false;

            transform.position = initPos;

            if (direction.sqrMagnitude > 0.0001f)
                transform.forward = direction.normalized;

            if (_collider != null)
            {
                _collider.excludeLayers = excludeLayer;
                _collider.enabled = true;
            }

            if (rigidbodyCompo != null)
            {
                rigidbodyCompo.linearVelocity =
                    transform.forward *
                    projectileShooter.ProjectileSpeed;
            }

            if (trail != null)
            {
                trail.emitting = true;
                trail.Clear();
            }

            if (flashEffect != null)
                PlayMuzzleFlash().Forget();
        }

        private void CheckMaxTravelDistance()
        {
            if (_isReturningToPool)
                return;

            if (_maxTravelDistance <= 0f)
                return;

            float maxDistanceSqr =
                _maxTravelDistance * _maxTravelDistance;

            if ((transform.position - _spawnPosition)
                .sqrMagnitude < maxDistanceSqr)
            {
                return;
            }

            _isReturningToPool = true;

            HideProjectile();

            ReturnToPool();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_myPool == null)
                return;

            if (_isReturningToPool)
                return;

            if (other == null)
                return;

            if (!TryResolveDamageable(
                    other,
                    out Transform hitTransform,
                    out IDamageable damageable))
            {
                if (other.isTrigger)
                    return;
            }

            _isReturningToPool = true;

            ResolveHitInfo(
                other,
                out Vector3 point,
                out Vector3 normal);

            Vector3 hitPosition =
                point + normal * hitOffset;

            ProcessDamage(
                damageable,
                hitTransform,
                hitPosition,
                normal);

            HideProjectile();

            if (hitEffect != null)
                PlayHitEffect(hitPosition, normal).Forget();

            ReturnToPoolAfterEffect().Forget();
        }

        private void ProcessDamage(
            IDamageable damageable,
            Transform hitTransform,
            Vector3 hitPosition,
            Vector3 normal)
        {
            if (damageable == null)
                return;

            if (_projectileShooter == null)
                return;

            if (_damageCalcCompo == null)
                return;

            float finalDamageMultiplier =
                _projectileShooter.DamageMultiplier;

            if (_owner.OnDamageCalc != null)
            {
                foreach (var del in
                         _owner.OnDamageCalc.GetInvocationList())
                {
                    finalDamageMultiplier +=
                        (float)del.DynamicInvoke(
                            _owner,
                            hitTransform);
                }
            }

            if (_sourceSkill != null)
            {
                finalDamageMultiplier +=
                    _sourceSkill
                        .RegisterHitAndGetMultiplier(hitTransform);
            }

            DamageData damageData =
                _damageCalcCompo.CalculateDamage(
                    _projectileShooter.DefaultDamage,
                    finalDamageMultiplier,
                    _projectileShooter.DefPierceLevel,
                    DamageType.RANGE);

            DamageContext context = new DamageContext
            {
                DamageData = damageData,
                HitPoint = hitPosition,
                HitNormal = normal,
                Source = gameObject,
                Attacker = _owner
            };

            damageable.ApplyDamage(context);

            _owner.OnAttack?.Invoke(
                _owner,
                damageable);
        }

        private void HideProjectile()
        {
            if (visualRoot != null)
                visualRoot.SetActive(false);

            if (trail != null)
                trail.emitting = false;

            if (_collider != null)
                _collider.enabled = false;

            if (rigidbodyCompo != null)
            {
                rigidbodyCompo.linearVelocity = Vector3.zero;
                rigidbodyCompo.angularVelocity = Vector3.zero;
            }
        }

        private async UniTaskVoid ReturnToPoolAfterEffect()
        {
            float delay = 0f;

            if (hitEffect != null)
            {
                var main = hitEffect.main;

                delay =
                    main.duration +
                    main.startLifetime.constantMax;
            }

            if (delay > 0f)
                await UniTask.WaitForSeconds(delay);

            ReturnToPool();
        }

        private void ReturnToPool()
        {
            if (_myPool != null)
                _myPool.Push(this);
            else
                gameObject.SetActive(false);
        }

        private bool TryResolveDamageable(
            Collider other,
            out Transform hitTransform,
            out IDamageable damageable)
        {
            hitTransform = other.transform;

            damageable = null;

            Entity hitEntity =
                other.GetComponentInParent<Entity>();

            if (hitEntity != null
                && hitEntity == _owner)
            {
                return false;
            }

            if (other.TryGetComponent(out damageable))
                return true;

            if (hitEntity != null
                && hitEntity.TryGetComponent(out damageable))
            {
                hitTransform = hitEntity.transform;

                return true;
            }

            return false;
        }

        private void ResolveHitInfo(
            Collider other,
            out Vector3 point,
            out Vector3 normal)
        {
            Vector3 referencePoint =
                _previousPosition;

            point = other.ClosestPoint(referencePoint);

            if ((point - referencePoint).sqrMagnitude < 0.0001f)
            {
                point = other.ClosestPoint(transform.position);
            }

            normal = referencePoint - point;

            if (normal.sqrMagnitude < 0.0001f)
            {
                normal = -transform.forward;
            }
            else
            {
                normal.Normalize();
            }
        }

        private async UniTask PlayMuzzleFlash()
        {
            GameObject flashGo =
                flashEffect.gameObject;

            flashGo.SetActive(true);

            flashEffect.transform.SetParent(null);

            flashEffect.transform.position =
                transform.position;

            flashEffect.transform.forward =
                transform.forward;

            flashEffect.Play(true);

            float duration =
                flashEffect.main.duration;

            await UniTask.WaitForSeconds(duration);

            flashEffect.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);

            flashGo.SetActive(false);

            flashEffect.transform.SetParent(transform);

            flashEffect.transform.localPosition =
                Vector3.zero;

            flashEffect.transform.localRotation =
                Quaternion.identity;
        }

        private async UniTaskVoid PlayHitEffect(
            Vector3 position,
            Vector3 normal)
        {
            Transform effectTransform =
                hitEffect.transform;

            effectTransform.SetParent(null);

            effectTransform.position =
                position - normal * 0.1f;

            if (useFirePointRotation)
            {
                effectTransform.rotation =
                    transform.rotation *
                    Quaternion.Euler(0, 180f, 0);
            }
            else if (rotationOffset != Vector3.zero)
            {
                effectTransform.rotation =
                    Quaternion.Euler(rotationOffset);
            }
            else
            {
                effectTransform.rotation =
                    Quaternion.LookRotation(normal);
            }

            hitEffect.gameObject.SetActive(true);

            hitEffect.Play(true);

            float delay =
                hitEffect.main.duration +
                hitEffect.main.startLifetime.constantMax;

            await UniTask.WaitForSeconds(delay);

            hitEffect.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);

            hitEffect.gameObject.SetActive(false);

            effectTransform.SetParent(transform);

            effectTransform.localPosition =
                Vector3.zero;

            effectTransform.localRotation =
                Quaternion.identity;
        }

        private void ResetEffect(ParticleSystem effect)
        {
            if (effect == null)
                return;

            effect.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);

            effect.gameObject.SetActive(false);

            effect.transform.SetParent(transform);

            effect.transform.localPosition =
                Vector3.zero;

            effect.transform.localRotation =
                Quaternion.identity;
        }
    }
}