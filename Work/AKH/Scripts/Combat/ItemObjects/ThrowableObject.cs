using Chipmunk.ComponentContainers;
using Code.ETC;
using Code.InventorySystems;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat.Datas;
using Scripts.Combat.Projectiles;
using Scripts.Entities;
using SHS.Scripts;
using UnityEngine;
using Work.LKW.Code.Items;

namespace Scripts.Combat.ItemObjects
{
    public class ThrowableObject : WeaponObject
    {
        [SerializeField] private AnimationCurve speedCurve;
        [SerializeField] private AnimationCurve pitchCurve;
        [SerializeField] private PoolItemSO throwItem;
        [SerializeField] private PoolManagerSO poolManager;
        [SerializeField] private LayerMask ignoreLayer;
        private ThrowableItem _throwableItem;
        private IAimProvider _aimProvider;

        public override void InitObject(Entity owner, EquipableItem item)
        {
            base.InitObject(owner, item);
            _aimProvider = owner.GetSubclassCompo<IAimProvider>();
            Debug.Assert(item is ThrowableItem, $"Invalid Throwable Type : {item.GetType()}");
            _throwableItem = item as ThrowableItem;
        }
        public override void Attack()
        {
            Vector3 dir = BuildV0(transform.position, _aimProvider.GetAimPosition());
            var th = poolManager.Pop(throwItem) as Throw;
            th.InitProjectile(_owner,_throwableItem,transform.position,dir,ignoreLayer);
        }
        public Vector3 BuildV0(Vector3 origin,Vector3 targetPoint)
        {
            ThrowableDataSO throwableData = _throwableItem.ThrowableData;
            Vector3 to = targetPoint - origin;
            Vector3 toXZ = new Vector3(to.x, 0f, to.z);
            float dist = toXZ.magnitude;

            Vector3 dirXZ = (dist > 1e-6f) ? (toXZ / dist) : Vector3.forward;

            // 0~1 거리 정규화
            float t = Mathf.InverseLerp(0, throwableData.attackRange, dist);

            // 커브 적용(없으면 기본 ease)
            float ts = (speedCurve != null) ? speedCurve.Evaluate(t) : t * t; // 가까운 구간을 더 "약하게"
            float tp = (pitchCurve != null) ? pitchCurve.Evaluate(t) : t;

            float speed = Mathf.Lerp(throwableData.minSpeed, throwableData.maxSpeed, ts);
            float pitch = Mathf.Lerp(throwableData.minPitchDeg, throwableData.maxPitchDeg, tp);

            float rad = pitch * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);

            Vector3 vXZ = dirXZ * (speed * cos);
            return new Vector3(vXZ.x, speed * sin, vXZ.z);
        }
    }
}
