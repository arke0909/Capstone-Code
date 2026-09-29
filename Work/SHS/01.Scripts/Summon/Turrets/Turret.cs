using Ami.BroAudio;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat.Datas;
using Scripts.Combat.Projectiles;
using Scripts.Entities;
using Scripts.Entities.Vitals;
using Scripts.FSM;
using Scripts.Players;
using SHS.Scripts.Summon.Turrets.FSM;
using System.Collections;
using UnityEngine;

namespace SHS.Scripts.Summon.Turrets
{
    public class Turret : Entity, ISummonable, IProjectileShooter, IPoolable
    {
        [SerializeField] private SoundID turretFireSound;
        [SerializeField] private SoundID turretReloadSound;
        [SerializeField] private PoolItemSO onDeadParticle;

        [Header("Pool Settings")]
        [SerializeField] private PoolItemSO poolItem;
        [SerializeField, Min(0f)] private float lifetime = 30f;

        [Header("Detection")] [SerializeField] private LayerMask targetLayer;
        [SerializeField] private LayerMask wallLayer;

        [Header("Stats")] [SerializeField] private float detectionRange;
        [SerializeField] private int damage;
        [SerializeField] private float bulletSpeed;
        [SerializeField] private float fireRate = 1f;
        [SerializeField] private float reloadDuration = 3f;

        [Header("Combat")] [SerializeField] private BulletDataSO bulletData;
        [SerializeField] private int maxAmmo;
        [SerializeField] private Transform[] firePoints;
        [SerializeField] private PoolManagerSO poolManager;
        [SerializeField] private PoolItemSO bulletPrefab;

        [Header("FSM")] [SerializeField] private StateDataSO[] stateDatas;

        [Header("Visual")] [SerializeField] private ParticleSystem[] fireEffects;
        [SerializeField] private string fireSpeedAnimParam = "FireSpeed";
        [SerializeField] private string reloadSpeedAnimParam = "ReloadSpeed";
        [SerializeField] private Transform headerTransform;

        public float DefaultDamage => damage;
        public float ProjectileSpeed => bulletSpeed;
        public float ProjectileMaxRange => detectionRange;
        public Transform CurrentFirePoint => firePoints[_currentAmmo % firePoints.Length];
        public float DetectionRange => detectionRange;
        public LayerMask TargetLayer => targetLayer;
        public Collider[] DetectedColliders => _detectedColliders;
        public Player TargetPlayer => _targetPlayer;
        public bool CanFire => _currentAmmo > 0;
        public PoolItemSO PoolItem => poolItem;
        public GameObject GameObject => gameObject;

        public float DamageMultiplier => bulletData.damageMultiplier;

        public int DefPierceLevel => bulletData.defPierceLevel;

        private Collider[] _detectedColliders = new Collider[10];
        private Player _targetPlayer;
        private int _currentAmmo;
        private Collider myCollider;
        private IVitalResettable _vital;
        private EngineerTurretTracker _turretTracker;
        private Pool _myPool;
        private Coroutine _lifetimeRoutine;
        private int _defaultLayer;
        private bool _isReleased;
        [SerializeField] private StateMachine<TurretStateEnum> _stateMachine;

        public override void OnInitialize(ComponentContainer componentContainer)
        {
            base.OnInitialize(componentContainer);
            myCollider = GetComponentInChildren<Collider>();
            _vital = componentContainer.GetSubclassComponent<IVitalResettable>();
            _stateMachine = new StateMachine<TurretStateEnum>(componentContainer, stateDatas);
            _currentAmmo = maxAmmo;
            _defaultLayer = gameObject.layer;
            SetAnimatorSpeeds();
            OnDeadEvent.AddListener(HandleTurretDead);
        }

        private void SetAnimatorSpeeds()
        {
            EntityAnimator animator = this.GetContainerComponent<EntityAnimator>();
            animator.SetParam(Animator.StringToHash(fireSpeedAnimParam), fireRate);
            animator.SetParam(Animator.StringToHash(reloadSpeedAnimParam), 1 / reloadDuration);
        }

        private void Start()
        {
            ChangeState(TurretStateEnum.Idle);
        }

        public void SetUpPool(Pool pool)
            => _myPool = pool;

        public void ResetItem()
        {
            _isReleased = false;
            IsDead = false;
            gameObject.layer = _defaultLayer;
            _targetPlayer = null;
            _currentAmmo = maxAmmo;
            _vital?.ResetVital();

            if (myCollider != null)
                myCollider.enabled = true;

            ChangeState(TurretStateEnum.Idle, true);

            if (_lifetimeRoutine != null)
                StopCoroutine(_lifetimeRoutine);

            _lifetimeRoutine = StartCoroutine(ReturnAfterLifetime());
        }

        public void SetTracker(EngineerTurretTracker turretTracker)
            => _turretTracker = turretTracker;

        protected virtual void Update()
        {
            _stateMachine?.UpdateStateMachine();
        }

        public void ChangeState(TurretStateEnum newState, bool forced = false)
        {
            _stateMachine?.ChangeState(newState, forced);
        }

        public void SetTargetPlayer(Player player)
        {
            _targetPlayer = player;
        }

        public void LookAtTarget(Vector3 targetPos)
        {
            Vector3 lookDirection = targetPos - transform.position;
            lookDirection.y = 0;
            headerTransform.LookAt(headerTransform.position + lookDirection);
        }

        public void FireBullet()
        {
            if (_targetPlayer == null) return;

            Vector3 direction = _targetPlayer.transform.position - transform.position;
            _currentAmmo--;
            Bullet bullet = poolManager.Pop(bulletPrefab) as Bullet;
            Debug.Assert(bullet != null, $"Projectile Pool is empty : Pool Item ({bulletPrefab.name})");
            bullet.InitProjectile(this, this, CurrentFirePoint.position, direction, 1 << gameObject.layer);
            fireEffects[_currentAmmo % fireEffects.Length].Play();
            BroAudio.Play(turretFireSound, transform.position);
        }

        public void ReloadComplete()
        {
            _currentAmmo = maxAmmo;
        }

        public void PlayReloadSound()
        {
            if (turretReloadSound.IsValid())
                BroAudio.Play(turretReloadSound, transform.position);
        }

        private void HandleTurretDead()
        {
            IsDead = true;
            gameObject.layer = LayerMask.NameToLayer("AvoidEntity");

            if (myCollider != null)
                myCollider.enabled = false;

            Bus.Raise(new PlayEffectEvent(onDeadParticle, transform.position, Quaternion.identity));
            ReleaseToPool();
        }

        private IEnumerator ReturnAfterLifetime()
        {
            yield return new WaitForSeconds(lifetime);
            _lifetimeRoutine = null;
            ReleaseToPool();
        }

        private void ReleaseToPool()
        {
            if (_isReleased)
                return;

            _isReleased = true;

            if (_lifetimeRoutine != null)
            {
                StopCoroutine(_lifetimeRoutine);
                _lifetimeRoutine = null;
            }

            _turretTracker?.Unregister(gameObject);
            _turretTracker = null;

            if (_myPool != null)
            {
                _myPool.Push(this);
                return;
            }

            Destroy(gameObject);
        }

        public bool WallExistsBetweenTarget(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - transform.position;
            float distance = direction.magnitude;
            if (Physics.Raycast(transform.position + Vector3.up, direction, out RaycastHit hit, detectionRange, wallLayer) &&
                hit.distance < distance)
                return true;
            return false;
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}
