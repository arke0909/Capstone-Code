using Scripts.Combat.Datas;
using UnityEngine;

namespace Scripts.Combat
{
    [CreateAssetMenu(fileName = "AttackData", menuName = "SO/Combat/AttackData", order = 0)]
    public class AttackDataSO : ScriptableObject
    {
        public string attackName;
        public DamageType damageType = DamageType.MELEE;
        public MovementDataSO movementData;

        public float impulseForce;
        public MovementDataSO knockbackMovement;
        private void OnEnable()
        {
            attackName = this.name; //파일이름으로 AttackName 을 지정한다.
        }
    }
}