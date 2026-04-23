using Chipmunk.ComponentContainers;
using Code.ETC;
using Code.InventorySystems;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat.Datas;
using Scripts.Combat.Projectiles;
using Scripts.Entities;
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
        private ThrowableDataSO _throwableData;
        private IAimProvider _aimProvider;
        public override void InitObject(Entity owner, EquipableItem item)
        {
            base.InitObject(owner, item);
            _aimProvider = owner.GetSubclassCompo<IAimProvider>();
            _throwableData = item.EquipItemData as ThrowableDataSO;
        }
        public override void Attack()
        {
            Vector3 dir = BuildV0(transform.position, _aimProvider.GetAimPosition());
            var th = poolManager.Pop(throwItem) as Throw;
            th.InitThrow(_owner,transform.position,dir);
        }
        public Vector3 BuildV0(Vector3 origin,Vector3 targetPoint)
        {
            Vector3 to = targetPoint - origin;
            Vector3 toXZ = new Vector3(to.x, 0f, to.z);
            float dist = toXZ.magnitude;

            Vector3 dirXZ = (dist > 1e-6f) ? (toXZ / dist) : Vector3.forward;

            // 0~1 거리 정규화
            float t = Mathf.InverseLerp(0, _throwableData.attackRange, dist);

            // 커브 적용(없으면 기본 ease)
            float ts = (speedCurve != null) ? speedCurve.Evaluate(t) : t * t; // 가까운 구간을 더 "약하게"
            float tp = (pitchCurve != null) ? pitchCurve.Evaluate(t) : t;

            float speed = Mathf.Lerp(_throwableData.minSpeed, _throwableData.maxSpeed, ts);
            float pitch = Mathf.Lerp(_throwableData.minPitchDeg, _throwableData.maxPitchDeg, tp);

            float rad = pitch * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);

            Vector3 vXZ = dirXZ * (speed * cos);
            return new Vector3(vXZ.x, speed * sin, vXZ.z);
        }
    }
}
