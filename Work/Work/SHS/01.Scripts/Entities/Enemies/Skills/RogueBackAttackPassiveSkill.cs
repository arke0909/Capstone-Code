using Chipmunk.ComponentContainers;
using Scripts.Entities;
using Scripts.SkillSystem;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.Skills
{
    public class RogueBackAttackPassiveSkill : PassiveSkill
    {
        [SerializeField, Range(-1f, 1f)] private float backAttackDotThreshold = -0.15f;
        [SerializeField, Min(0f)] private float backAttackBonusMultiplier = 0.55f;
        [SerializeField, Min(0f)] private float stealthBonusMultiplier = 0.35f;

        private RogueStealthSkill _stealthSkill;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _stealthSkill = _owner.GetComponentInChildren<RogueStealthSkill>(true);
        }

        public override void EnableSkill()
        {
            base.EnableSkill();
            _owner.OnDamageCalc += HandleDamageCalc;
        }

        public override void DisableSkill()
        {
            _owner.OnDamageCalc -= HandleDamageCalc;
            base.DisableSkill();
        }

        private float HandleDamageCalc(Entity dealer, Transform target)
        {
            if (dealer == null || target == null)
                return 0f;

            Vector3 toDealer = dealer.transform.position - target.position;
            toDealer.y = 0f;

            Vector3 targetForward = target.forward;
            targetForward.y = 0f;

            if (toDealer.sqrMagnitude < 0.001f || targetForward.sqrMagnitude < 0.001f)
                return 0f;

            float dot = Vector3.Dot(targetForward.normalized, toDealer.normalized);
            if (dot > backAttackDotThreshold)
                return 0f;

            float bonus = backAttackBonusMultiplier;
            if (_stealthSkill != null && _stealthSkill.IsStealthed)
                bonus += stealthBonusMultiplier;

            return bonus;
        }
    }
}
