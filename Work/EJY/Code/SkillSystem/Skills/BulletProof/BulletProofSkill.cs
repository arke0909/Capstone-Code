﻿using Chipmunk.ComponentContainers;
using Code.StatusEffectSystem;
using Code.StatusEffectSystem.StatusEffects;
using Entities;
using Scripts.SkillSystem;
using UnityEngine;
namespace Code.SkillSystem.Skills.BulletProof
{
    public class BulletProofSkill : ActiveSkill
    {
        [SerializeField] private BuffSO shieldBuff;
        [SerializeField] private BuffSO dmgIncreaseByShieldBuff; // temp
        [SerializeField] private BuffSO damageMultiIncreaseData;
        [SerializeField] private bool isDmgIncreaseByShield;
        [SerializeField] private bool isDmgIncreaseAtHaveShield;
        private EntityStatusEffect _entityStatusEffect;
        private VFXComponent _vfxComponent;
        private AbstractStatusEffect _bulletProofShieldEffect;
        private bool _isBulletProofVfxPlaying;
        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _entityStatusEffect = container.Get<EntityStatusEffect>();
            _vfxComponent = container.Get<VFXComponent>();
            _entityStatusEffect.OnStatusEffectReleased += HandleStatusEffectReleased;
        }
        
        private void UpgradeDmgInCreaseAtHaveShield() => isDmgIncreaseAtHaveShield = true;
        private void RollbackDmgInCreaseAtHaveShield() => isDmgIncreaseAtHaveShield = false;
        private void UpgradeDmgIncreaseByShield() => isDmgIncreaseByShield = true;
        private void RollbackDmgIncreaseByShield() => isDmgIncreaseByShield = false;

        
        public override void StartAndUseSkill()
        {
            if (_isBulletProofVfxPlaying == false)
            {
                _vfxComponent.PlayVFX("BulletProof", transform.position, Quaternion.identity);
                _isBulletProofVfxPlaying = true;
            }
            // temp
            if (isDmgIncreaseAtHaveShield)
            {
                foreach (var info in damageMultiIncreaseData.GetStatusEffectInfo())
                {
                    _entityStatusEffect.AddStatusEffect(info);
                }
            }
            // temp
            if (isDmgIncreaseByShield)
            {
                foreach (var info in dmgIncreaseByShieldBuff.GetStatusEffectInfo())
                {
                    _entityStatusEffect.AddStatusEffect(info);
                }
            }
            
            foreach (var info in shieldBuff.GetStatusEffectInfo())
            {
                var appliedEffect = _entityStatusEffect.AddStatusEffect(info);
                if (info.StatusEffect == StatusEffectEnum.SHIELD && appliedEffect != null)
                {
                    _bulletProofShieldEffect = appliedEffect;
                }
            }
        }
        private void HandleStatusEffectReleased(AbstractStatusEffect effect)
        {
            if (effect != _bulletProofShieldEffect)
                return;
            if (_isBulletProofVfxPlaying == false)
                return;
            _vfxComponent.StopVFX("BulletProof");
            _bulletProofShieldEffect = null;
            _isBulletProofVfxPlaying = false;
        }
        private void OnDestroy()
        {
            if (_entityStatusEffect == null)
                return;
            _entityStatusEffect.OnStatusEffectReleased -= HandleStatusEffectReleased;
        }
    }
}