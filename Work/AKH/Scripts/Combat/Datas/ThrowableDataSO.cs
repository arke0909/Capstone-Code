using UnityEngine;
using Work.LKW.Code.Items;
using Work.LKW.Code.Items.ItemInfo;

namespace Scripts.Combat.Datas
{
    [CreateAssetMenu(fileName = "ThrowableDataSO", menuName = "SO/Item/ThrowableData", order = 0)]
    public class ThrowableDataSO : WeaponDataSO
    {
        public float minSpeed;
        public float maxSpeed;
        public float minPitchDeg;
        public float maxPitchDeg;
        public AnimationCurve speedCurve;
        public AnimationCurve pitchCurve;
        public override ItemCreateData CreateItem()
        {
            return new ItemCreateData(new ThrowableItem(this), 1);
        }
    }
}
