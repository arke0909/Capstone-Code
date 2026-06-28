using Ami.BroAudio;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Effects;
using UnityEngine;
using Work.Code.MapEvents;

namespace Code.ETC.MapObjects
{
    [DisallowMultipleComponent]
    public class Trap : MonoBehaviour,IPoolable,ISpawnableStructure
    {
        private const float DestroyDelay = 0.04f;

        [Header("Reference")]
        [SerializeField] private LayerMask whatIsTriggerTarget;
        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private PoolItemSO explosiveItem;
        [SerializeField] private PoolManagerSO poolManager;
        [SerializeField] private OverlapDamageCaster damageCaster;
        [SerializeField] private MovementDataSO movementData;
        [SerializeField] private SoundID explosiveSoundID;

        [Header("Setting")]
        [SerializeField] private float damage = 60f;

        [field:SerializeField]public PoolItemSO PoolItem { get; set; }

        public GameObject GameObject => gameObject;
        private Pool _myPool;
        private void Awake()
        {
            if (damageCaster != null)
                damageCaster.InitCaster(null);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsInLayer(other.gameObject.layer, whatIsTriggerTarget)) return;

            Explode();
        }

        public void Explode()
        {
            if (damageCaster != null)
            {
                DamageData damageData = new DamageData
                {
                    damage = damage,
                    damageType = DamageType.MAGIC,
                    defPierceLevel = 1
                };
                damageCaster.CastDamage(damageData, transform.position, transform.forward, movementData);
            }

            if (poolManager != null && explosiveItem != null)
            {
                PoolingEffect effect = poolManager.Pop(explosiveItem) as PoolingEffect;
                if (effect != null)
                    effect.PlayVFX(transform.position, Quaternion.identity);
            }

            BroAudio.Play(explosiveSoundID, transform.position);

            _myPool.Push(this);
        }

        private static bool IsInLayer(int layer, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        public void ResetItem()
        {
        }

        public void Spawn(Vector3 targetPos)
        {
            if(Physics.Raycast(targetPos, Vector3.down, out RaycastHit hit, 10, whatIsGround))
            {
                targetPos.y = hit.point.y;
                transform.position = targetPos;
            }
        }

        public void Despawn()
        {
        }
    }
}
