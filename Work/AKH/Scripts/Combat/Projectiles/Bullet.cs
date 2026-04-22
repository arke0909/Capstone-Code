using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Cysharp.Threading.Tasks;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;
using Work.Code.GameEvents;
using SHS.Scripts;
using SHS.Scripts.Effects;
using Work.Code.Entities;
using Work.Code.Misc;

namespace Scripts.Combat.Projectiles
{
    public class Bullet : MonoBehaviour, IPoolable, IDamageDelaer
    {
        [SerializeField] private float hitOffset = 0f;
        [SerializeField] private bool UseFirePointRotation;
        [SerializeField] private Vector3 rotationOffset = new Vector3(0, 0, 0);
        [SerializeField] private ParticleSystem hitEffect;
        [SerializeField] private ParticleSystem flashEffect;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private GameObject[] Detached;
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private BulletImpactEffect _bulletImpactEffect;
        [SerializeField] private PoolItemSO bulletHole;
        [SerializeField] private PoolManagerSO poolManager;
        
        [field: SerializeField] public PoolItemSO PoolItem { get; private set; }
        public GameObject GameObject => gameObject;
        public GameObject Dealer => gameObject;
        public Entity Owner => _owner;
        public Vector3 Velocity => rb.linearVelocity;
        public IBulletShooter ProjectileShooter { get; private set; }
        
        private Pool _myPool;
        private Entity _owner;
        private Collider _collider;
        private Vector3 _previousPosition;
        private Vector3 _onInitVelocity;
        private bool _isReturningToPool;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        private void FixedUpdate()
        {
            _previousPosition = transform.position;
        }

        public void InitBullet(Entity owner, IBulletShooter projectileShooter, Vector3 initPos, Vector3 direction,
            LayerMask excludeLayer)
        {
            _collider.excludeLayers = excludeLayer;
            InitBullet(owner, projectileShooter, initPos, direction);
        }

        public void InitBullet(Entity owner, IBulletShooter projectileShooter, Vector3 initPos, Vector3 direction)
        {
            _owner = owner;
            ProjectileShooter = projectileShooter;
            _isReturningToPool = false;
            _previousPosition = initPos;

            transform.position = initPos;
            if (direction.sqrMagnitude > 0.0001f)
                transform.forward = direction.normalized;
            float speed = ProjectileShooter.ProjectileSpeed;
            rb.linearVelocity = direction.normalized * speed;
            _onInitVelocity = rb.linearVelocity;
            
            trail.Clear();

            if (flashEffect != null)
            {
                PlayMuzzleFlash().Forget();
            }
        }

        public void ResetItem()
        {
            _collider.excludeLayers = 0;
            _isReturningToPool = false;
            _previousPosition = transform.position;
            _onInitVelocity = Vector3.zero;
            
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (hitEffect != null)
            {
                hitEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                hitEffect.gameObject.SetActive(false);
                hitEffect.transform.SetParent(transform);
                hitEffect.transform.localPosition = Vector3.zero;
                hitEffect.transform.localRotation = Quaternion.identity;
            }

            if (flashEffect != null)
            {
                flashEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                flashEffect.gameObject.SetActive(false);
                flashEffect.transform.SetParent(transform);
                flashEffect.transform.localPosition = Vector3.zero;
                flashEffect.transform.localRotation = Quaternion.identity;
            }

            _owner = null;
            ProjectileShooter = null;
        }

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        private async void OnTriggerEnter(Collider other)
        {
            if (_myPool == null || _isReturningToPool || other == null)
                return;

            if (!TryResolveDamageable(other, out Transform hitTransform, out IDamageable damageable))
            {
                if (other.isTrigger)
                    return;
            }

            _isReturningToPool = true;
            ResolveHitInfo(other, out Vector3 point, out Vector3 normal);
            Vector3 pos = point + normal * hitOffset;

            if (hitEffect != null)
            {
                gameObject.SetActive(false);
                await PlayHitEffect(pos, normal);
            }

            if (damageable != null && ProjectileShooter !=null)
            {
                DamageCalcCompo calcCompo = _owner.Get<DamageCalcCompo>();
                DamageData damageData;

                BulletDataSO bulletData = ProjectileShooter.BulletData; //총알 장착이 이제 잇음

                float finalDamageMultiply = bulletData.damageMultiplier;

                if (_owner.OnDamageCalc != null)
                {
                    foreach (var del in _owner.OnDamageCalc.GetInvocationList())
                    {
                        finalDamageMultiply += (float)del.DynamicInvoke(_owner, hitTransform);
                    }
                }

                damageData = calcCompo.CalculateDamage(ProjectileShooter.DefaultDamage, finalDamageMultiply,
                    bulletData.defPierceLevel, DamageType.RANGE);

                DamageContext context = new DamageContext
                {
                    DamageData = damageData,
                    HitPoint = pos,
                    HitNormal = normal,
                    Source = Dealer,
                    Attacker = Owner
                };
                    
                damageable.ApplyDamage(context);
                _owner.OnHit?.Invoke(_owner, damageable);
            }
            else
            {
                BulletHole hole = poolManager.Pop(bulletHole) as BulletHole;
                hole?.InitHole(pos, normal);
            }

            _bulletImpactEffect?.PlayEffect(pos, normal);
           
            _myPool.Push(this);
        }

        private bool TryResolveDamageable(Collider other, out Transform hitTransform, out IDamageable damageable)
        {
            hitTransform = other.transform;
            damageable = null;

            Entity hitEntity = other.GetComponentInParent<Entity>();
            if (hitEntity != null && hitEntity == _owner)
                return false;

            if (other.TryGetComponent(out damageable))
                return true;

            if (hitEntity != null && hitEntity.TryGetComponent(out damageable))
            {
                hitTransform = hitEntity.transform;
                return true;
            }

            return false;
        }

        private void ResolveHitInfo(Collider other, out Vector3 point, out Vector3 normal)
        {
            Vector3 referencePoint = _previousPosition;
            point = other.ClosestPoint(referencePoint);

            if ((point - referencePoint).sqrMagnitude < 0.0001f)
                point = other.ClosestPoint(transform.position);

            normal = referencePoint - point;
            if (normal.sqrMagnitude < 0.0001f)
                normal = -transform.forward;
            else
                normal.Normalize();
        }
        
        private async UniTask PlayMuzzleFlash()
        {
            GameObject flashGo = flashEffect.gameObject;

            flashGo.SetActive(true);
            flashEffect.transform.SetParent(null);

            flashEffect.transform.position = transform.position;
            flashEffect.transform.forward = transform.forward;

            flashEffect.Play(true);

            float duration = flashEffect.main.duration;
            await UniTask.WaitForSeconds(duration);

            flashEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            flashGo.SetActive(false);

            flashEffect.transform.SetParent(transform);
            flashEffect.transform.localPosition = Vector3.zero;
            flashEffect.transform.localRotation = Quaternion.identity;
        }

        private async UniTask PlayHitEffect(Vector3 position, Vector3 normal)
        {
            Transform t = hitEffect.transform;
            t.SetParent(null);
            t.position = position - normal * 0.1f;

            if (UseFirePointRotation)
            {
                t.rotation = transform.rotation * Quaternion.Euler(0, 180f, 0);
            }
            else if (rotationOffset != Vector3.zero)
            {
                t.rotation = Quaternion.Euler(rotationOffset);
            }
            else
            {
                t.rotation = Quaternion.LookRotation(normal);
            }

            hitEffect.gameObject.SetActive(true);
            hitEffect.Play(true);

            float duration = hitEffect.main.duration;
            await UniTask.WaitForSeconds(duration);

            hitEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            hitEffect.gameObject.SetActive(false);

            t.SetParent(transform);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
        }

        public void SetVelocity(float percent)
        {
            percent = Mathf.Clamp01(percent);
            
            rb.linearVelocity = _onInitVelocity * percent;
        }
        
        public void PushBullet() => _myPool.Push(this);
    }
}
