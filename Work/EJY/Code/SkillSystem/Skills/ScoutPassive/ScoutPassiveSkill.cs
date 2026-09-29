using Scripts.Entities;
using Scripts.SkillSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.SkillSystem.Skills.ScoutPassive
{
    public class ScoutPassiveSkill : PassiveSkill
    {
        [SerializeField, FormerlySerializedAs("passiveRange")]
        private float startDistance = 20f;
        [SerializeField] private float fullBonusDistance = 70f;
        [SerializeField, FormerlySerializedAs("additionalDamagePercent")]
        private float maxBonusDamageMultiplier = 0.5f;

        public override void EnableSkill()
        {
            base.EnableSkill();
            Debug.Assert(fullBonusDistance > startDistance,
                "ScoutPassiveSkill: fullBonusDistance must be greater than startDistance.");
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
            float ratio = Mathf.InverseLerp(startDistance, fullBonusDistance, distance);
            return ratio * maxBonusDamageMultiplier;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, startDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, fullBonusDistance);
        }
    }
}
