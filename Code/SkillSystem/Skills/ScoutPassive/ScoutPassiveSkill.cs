using Scripts.Entities;
using Scripts.SkillSystem;
using UnityEngine;

namespace Code.SkillSystem.Skills.ScoutPassive
{
    public class ScoutPassiveSkill : PassiveSkill
    {
        [SerializeField] private float passiveRange = 5;
        [SerializeField] private float additionalDamagePercent = 0.5f;

        public override void EnableSkill()
        {
            base.EnableSkill();
            _owner.OnDamageCalc += HandleOnDamageCalc;
        }

        public override void DisableSkill()
        {
            _owner.OnDamageCalc -= HandleOnDamageCalc;
            base.DisableSkill();
        }


        private float HandleOnDamageCalc(Entity dealer, Transform target)
        {
            Vector3 dealerPos = dealer.transform.position;
            Vector3 targetPos = target.position;

            targetPos.y = dealerPos.y;
            
            float distance = Vector3.Distance(dealerPos, targetPos);
            float ratio = distance / passiveRange;
            
            float additionalDamageMultiply = Mathf.Max(ratio * additionalDamagePercent, additionalDamagePercent);

            return additionalDamageMultiply;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, passiveRange / 2);
        }
    }
}