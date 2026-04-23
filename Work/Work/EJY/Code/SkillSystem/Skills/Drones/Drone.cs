using Chipmunk.ComponentContainers;
using Chipmunk.Modules.StatSystem;
using Code.StatusEffectSystem;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat;
using Scripts.Combat.Fovs;
using Scripts.Effects;
using UnityEngine;

namespace Code.SkillSystem.Skills.Drones
{
    public class Drone : MonoBehaviour, IPoolable, IContainerComponent
    {
        public StatSO temp;
        [SerializeField] private PoolItemSO scanEffectItem;
        [SerializeField] private PoolManagerSO poolManagerSO;
        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private PoolItemSO poolItem;
        [SerializeField] private PoolItemSO speedAreaItem;
        [SerializeField] private BuffSO damageDemodifyDecrease;
        [SerializeField] private OverlapBuffCaster buffCaster;
        [SerializeField] private FovCompo fovCompo;
        //[SerializeField] private float sightAngle = 360f;
        [SerializeField] private float moveSpeed = 15f;
        [SerializeField] private float destinationThreshold = 0.6f;

        private Pool _myPool;
        private Rigidbody _rigidbody;
        private Vector3 _targetPos;
        private SpeedArea _speedArea;
        private RaycastHit _hitInfo;

        private bool _isDamageDemodifyDecrease;
        private bool _needScan;
        private bool _createSpeedArea;
        private int _level;
        private float _sightRange;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void Init(Vector3 targetPos, bool needScan, int level, float sightRange)
        {
            _targetPos = targetPos;
            _targetPos.y = transform.position.y;
            _needScan = needScan;
            _level = level;
            _sightRange = sightRange;

            if (fovCompo.fovInfos.Length > 0)
            {
                FOVInfo info = fovCompo.fovInfos[0];
                info.viewRadius = _sightRange;
                fovCompo.fovInfos[0] = info;
            }
            buffCaster.SetRadius(_sightRange);

            Vector3 direction = (_targetPos - transform.position).normalized;
            transform.forward = direction;

            if (_createSpeedArea)
            {
                _speedArea = poolManagerSO.Pop(speedAreaItem) as SpeedArea;

                _speedArea.SetStartPos(GetGroundPos(transform.position).point);
                _speedArea.SetEndPos(GetGroundPos(_targetPos).point);
            }

            _rigidbody.linearVelocity = direction * moveSpeed;
        }
        public void SetIsDamageModifyDecrease(bool isDamageModifyDecrease) =>
            _isDamageDemodifyDecrease = isDamageModifyDecrease;
        public void SetCreateSpeedArea(bool createSpeedArea) =>
            _createSpeedArea = createSpeedArea;

        public PoolItemSO PoolItem => poolItem;
        public GameObject GameObject => gameObject;

        public ComponentContainer ComponentContainer { get; set; }

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        private RaycastHit GetGroundPos(Vector3 pos)
        {
            Physics.Raycast(pos, Vector3.down, out RaycastHit hit, Mathf.Infinity, whatIsGround);
            _hitInfo = hit;
            return _hitInfo;
        }

        private void Update()
        {
            if (Vector3.Distance(_targetPos, transform.position) < destinationThreshold)
            {
                _myPool.Push(this);

                if (_needScan)
                    ScanTarget();

                if (_createSpeedArea)
                    _speedArea.CreateArea();
            }
        }

        private void ScanTarget()
        {
            Vector3 overlapPos = _targetPos;
            overlapPos.y = GetGroundPos(transform.position).point.y;

            buffCaster.CastBuff(overlapPos, damageDemodifyDecrease.GetStatusEffectInfo(_level));

            PoolingEffect poolingEffect = poolManagerSO.Pop(scanEffectItem) as PoolingEffect;
            poolingEffect.PlayVFX(transform.position, Quaternion.identity);
        }

        public void ResetItem()
        {
        }
        public void OnInitialize(ComponentContainer componentContainer)
        {
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, _sightRange);
        }
#endif
    }
}