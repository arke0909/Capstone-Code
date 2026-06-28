using Chipmunk.ComponentContainers;
using Cysharp.Threading.Tasks;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat.Datas;
using Scripts.Entities;
using SHS.Scripts;
using SHS.Scripts.Combats.Events;
using SHS.Scripts.Effects;
using UnityEngine;
using Work.Code.Entities;
using Work.Code.GameEvents;
using Work.Code.Misc;

namespace Scripts.Combat.Projectiles
{
    public class Bullet : MonoBehaviour, IProjectile, IDamageDelaer
    {
        [SerializeField] private float despawnTime = 1f;
        [SerializeField] private float hitOffset = 0f;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private BulletImpactEffect _bulletImpactEffect;
        [SerializeField] private PoolItemSO bulletHole;
        [SerializeField] private PoolManagerSO poolManager;

        [field: SerializeField] public PoolItemSO PoolItem { get; private set; }
        public GameObject GameObject => gameObject;
        public GameObject Dealer => gameObject;
        public Entity Owner => _owner;
        public Vector3 Velocity => rb.linearVelocity;
        public IProjectileShooter ProjectileShooter => _projectileShooterSnapshot;

        private Pool _myPool;
        private Entity _owner;
        private Collider _collider;
        private Vector3 _spawnPosition;
        private Vector3 _onInitVelocity;
        private ProjectileShooterSnapshot _projectileShooterSnapshot;
        private float _maxTravelDistance;
        private bool _isReturningToPool;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            Debug.Assert(rb != null, "Bullet: Rigidbody is not assigned.");
            Debug.Assert(_collider != null, "Bullet: Collider is not assigned.");
            Debug.Assert(trail != null, "Bullet: TrailRenderer is not assigned.");
            Debug.Assert(_bulletImpactEffect != null, "Bullet: BulletImpactEffect is not assigned.");
            Debug.Assert(bulletHole != null, "Bullet: BulletHole pool item is not assigned.");
            Debug.Assert(poolManager != null, "Bullet: PoolManager is not assigned.");
        }

        private void FixedUpdate()
        {
            if (_isReturningToPool)
                return;

            CheckMaxTravelDistance();
        }

        public void InitProjectile(Entity owner, IProjectileShooter projectileShooter, Vector3 initPos,
            Vector3 direction,
            LayerMask excludeLayer)
        {
            Debug.Assert(projectileShooter != null, "Bullet: ProjectileShooter is null.");
            _owner = owner;
            _projectileShooterSnapshot = new ProjectileShooterSnapshot(projectileShooter);
            _isReturningToPool = false;
            _spawnPosition = initPos;

            _collider.excludeLayers = excludeLayer;
            transform.position = initPos;

            if (direction.sqrMagnitude > 0.0001f)
                transform.forward = direction.normalized;

            rb.linearVelocity = direction.normalized * _projectileShooterSnapshot.ProjectileSpeed;
            _onInitVelocity = rb.linearVelocity;
            _maxTravelDistance = _projectileShooterSnapshot.ProjectileMaxRange;

            trail.Clear();
        }

        public void ResetItem()
        {
            _isReturningToPool = false;
            _spawnPosition = transform.position;
            _onInitVelocity = Vector3.zero;
            _projectileShooterSnapshot = default;
            _maxTravelDistance = 0f;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            _owner = null;
        }

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_isReturningToPool)
                return;

            ContactPoint contact = collision.GetContact(0);
            transform.position = contact.point;
            HandleHit(collision.collider, contact.point, contact.normal);
        }

        public void HitImmediately(Collider other, Vector3 hitPoint, Vector3 hitNormal)
        {
            Debug.Assert(other != null, "Bullet: Immediate hit target is null.");
            transform.position = hitPoint;
            HandleHit(other, hitPoint, hitNormal);
        }

        private void CheckMaxTravelDistance()
        {
            if (_isReturningToPool || _maxTravelDistance <= 0f)
                return;

            float maxDistanceSqr = _maxTravelDistance * _maxTravelDistance;
            if ((transform.position - _spawnPosition).sqrMagnitude < maxDistanceSqr)
                return;

            _isReturningToPool = true;
            ReturnToPoolAfterDelay().Forget();
        }

        private void HandleHit(Collider other, Vector3 hitPoint, Vector3 hitNormal)
        {
            Debug.Assert(_myPool != null, "Bullet: Pool is not assigned.");

            if (_isReturningToPool)
                return;

            TryResolveDamageable(other, out Transform hitTransform, out IDamageable damageable);

            _isReturningToPool = true;
            PrepareForDespawn();

            Vector3 pos = hitPoint + hitNormal * hitOffset;

            if (damageable != null)
            {
                DamageCalcCompo calcCompo = _owner.Get<DamageCalcCompo>();

                float finalDamageMultiply = _projectileShooterSnapshot.DamageMultiplier;

                if (_owner.OnDamageCalc != null)
                {
                    foreach (var del in _owner.OnDamageCalc.GetInvocationList())
                    {
                        finalDamageMultiply += (float)del.DynamicInvoke(_owner, hitTransform);
                    }
                }

                DamageData damageData = calcCompo.CalculateDamage(_projectileShooterSnapshot.DefaultDamage,
                    finalDamageMultiply, _projectileShooterSnapshot.DefPierceLevel, DamageType.RANGE);

                DamageContext context = new DamageContext
                {
                    DamageData = damageData,
                    HitPoint = pos,
                    HitNormal = hitNormal,
                    Source = Dealer,
                    Attacker = Owner
                };

                damageable.ApplyDamage(context);
                _owner.LocalEventBus.Raise(new AttackHitEvent(damageable, context));
                _owner.OnAttack?.Invoke(_owner, damageable);
            }
            else
            {
                BulletHole hole = poolManager.Pop(bulletHole) as BulletHole;
                Debug.Assert(hole != null, "Bullet: BulletHole pool item could not be popped.");
                hole.InitHole(pos, hitNormal);
            }

            _bulletImpactEffect.PlayEffect(pos, hitNormal);

            ReturnToPoolAfterDelay().Forget();
        }

        private void PrepareForDespawn()
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        private async UniTaskVoid ReturnToPoolAfterDelay()
        {
            PrepareForDespawn();

            float delay = Mathf.Max(0f, despawnTime);
            if (delay > 0f)
                await UniTask.WaitForSeconds(delay);

            _myPool.Push(this);
        }

        private bool TryResolveDamageable(Collider other, out Transform hitTransform, out IDamageable damageable)
        {
            hitTransform = other.transform;
            damageable = null;

            Entity hitEntity = other.GetComponentInParent<Entity>();

            if (other.TryGetComponent(out damageable))
                return true;

            if (hitEntity != null && hitEntity.TryGetComponent(out damageable))
            {
                hitTransform = hitEntity.transform;
                return true;
            }

            return false;
        }

        public void SetVelocity(float percent)
        {
            percent = Mathf.Clamp01(percent);

            rb.linearVelocity = _onInitVelocity * percent;
        }

        public void PushBullet()
        {
            if (_isReturningToPool)
                return;

            _isReturningToPool = true;
            ReturnToPoolAfterDelay().Forget();
        }

        private readonly struct ProjectileShooterSnapshot : IProjectileShooter
        {
            public float DefaultDamage { get; }
            public float ProjectileSpeed { get; }
            public float ProjectileMaxRange { get; }
            public float DamageMultiplier { get; }
            public int DefPierceLevel { get; }

            public ProjectileShooterSnapshot(IProjectileShooter projectileShooter)
            {
                DefaultDamage = projectileShooter.DefaultDamage;
                ProjectileSpeed = projectileShooter.ProjectileSpeed;
                ProjectileMaxRange = projectileShooter.ProjectileMaxRange;
                DamageMultiplier = projectileShooter.DamageMultiplier;
                DefPierceLevel = projectileShooter.DefPierceLevel;
            }
        }
    }
}
