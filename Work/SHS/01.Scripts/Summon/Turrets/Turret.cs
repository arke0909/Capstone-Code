using Chipmunk.ComponentContainers;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Combat.Projectiles;
using Scripts.Entities;
using Scripts.FSM;
using Scripts.Players;
using SHS.Scripts.Summon.Turrets.FSM;
using UnityEngine;

namespace SHS.Scripts.Summon.Turrets
{
    public class Turret : Entity, ISummonable, IProjectileShooter
    {
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

        public float DamageMultiplier => bulletData.damageMultiplier;

        public int DefPierceLevel => bulletData.defPierceLevel;

        private Collider[] _detectedColliders = new Collider[10];
        private Player _targetPlayer;
        private int _currentAmmo;
        private Collider myCollider;
        [SerializeField] private StateMachine<TurretStateEnum> _stateMachine;

        public override void OnInitialize(ComponentContainer componentContainer)
        {
            base.OnInitialize(componentContainer);
            myCollider = GetComponentInChildren<Collider>();
            _stateMachine = new StateMachine<TurretStateEnum>(componentContainer, stateDatas);
            _currentAmmo = maxAmmo;
            SetAnimatorSpeeds();

            // 디스폰 날먹 코드
            Destroy(gameObject, 30f);
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
        }

        public void ReloadComplete()
        {
            _currentAmmo = maxAmmo;
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
