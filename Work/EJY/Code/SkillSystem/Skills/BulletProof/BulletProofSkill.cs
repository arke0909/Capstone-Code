using Ami.BroAudio;
using Chipmunk.ComponentContainers;
using Code.StatusEffectSystem;
using Scripts.SkillSystem;
using UnityEngine;

namespace Code.SkillSystem.Skills.BulletProof
{
    public class BulletProofSkill : ActiveSkill
    {
        [SerializeField] private BuffSO shieldBuff;
        [SerializeField] private BuffSO dmgIncreaseByShieldBuff; // temp
        [SerializeField] private BuffSO damageMultiIncreaseData;
        [SerializeField] private SoundID soundID;
        [SerializeField] private bool isDmgIncreaseByShield;
        [SerializeField] private bool isDmgIncreaseAtHaveShield;
        private EntityStatusEffect _entityStatusEffect;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _entityStatusEffect = container.Get<EntityStatusEffect>();
        }

        private void UpgradeDmgInCreaseAtHaveShield() => isDmgIncreaseAtHaveShield = true;
        private void RollbackDmgInCreaseAtHaveShield() => isDmgIncreaseAtHaveShield = false;
        private void UpgradeDmgIncreaseByShield() => isDmgIncreaseByShield = true;
        private void RollbackDmgIncreaseByShield() => isDmgIncreaseByShield = false;

        public override void StartSkill()
        {
            Debug.Assert(shieldBuff != null, $"{nameof(BulletProofSkill)} requires {nameof(shieldBuff)}.", this);
            if (shieldBuff == null)
                return;

            BroAudio.Play(soundID, _owner.transform.position);

            // temp
            if (isDmgIncreaseAtHaveShield)
            {
                _entityStatusEffect.AddStatusEffect(damageMultiIncreaseData, this);
            }
            // temp
            if (isDmgIncreaseByShield)
            {
                _entityStatusEffect.AddStatusEffect(dmgIncreaseByShieldBuff, this);
            }

            _entityStatusEffect.AddStatusEffect(shieldBuff, this);
        }
    }
}
