using Scripts.Combat.Datas;
using UnityEngine;

namespace Scripts.Combat
{
    public interface IKnockbackable
    {
        public void KnockBack(Vector3 direction, MovementDataSO movementData);
    }
}
