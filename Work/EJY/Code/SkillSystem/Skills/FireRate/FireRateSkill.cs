﻿using System;
using Chipmunk.ComponentContainers;
using Chipmunk.Modules.StatSystem;
using Code.StatusEffectSystem;
using Code.StatusEffectSystem.StatusEffects;
using Cysharp.Threading.Tasks;
using Entities;
using Scripts.Combat;
using Scripts.Entities;
using Scripts.SkillSystem;
using UnityEngine;

namespace Code.SkillSystem.Skills.FireRate
{
    public class FireRateSkill : ActiveSkill
    {
        [SerializeField] private BuffSO fireRateBuffSO;
        [SerializeField] private BuffSO bulletReduceRateBuffSO;
        [SerializeField] private StatSO fireRateStatSO;
        [SerializeField] private bool isOnHitAddFireRate;
        [SerializeField] private bool isBulletReduceRateDecrease;
        [SerializeField] private float onHitFireRateAmount = 0.025f, maxFireRate = 0.5f;
        
        private EntityStatusEffect _entityStatusEffect;
        private StatOverrideBehavior _stat;
        private VFXComponent _vfxComponent;
        private float _totalFireRate;
        
        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _entityStatusEffect = container.Get<EntityStatusEffect>();
            _stat = container.Get<StatOverrideBehavior>();
            _vfxComponent = container.Get<VFXComponent>();
        }
        
        private void UpgradeOnHitAddFireRate() => isOnHitAddFireRate = true;
        private void RollbackOnHitAddFireRate() => isOnHitAddFireRate = false;
        private void UpgradeBulletReduceRateDecrease() => isBulletReduceRateDecrease = true;
        private void RollbackBulletReduceRateDecrease() => isBulletReduceRateDecrease = false;


        public override async void StartAndUseSkill()
        {
            PlayVFX();
            
            foreach (var info in fireRateBuffSO.GetStatusEffectInfo())
            {
                _entityStatusEffect.AddStatusEffect(info);
            }

            if(isBulletReduceRateDecrease)
                foreach (var info in bulletReduceRateBuffSO.GetStatusEffectInfo())
                {
                    _entityStatusEffect.AddStatusEffect(info);
                }
            
            if (isOnHitAddFireRate)
            {
                var cts = this.GetCancellationTokenOnDestroy();   
                
                _owner.OnHit += OnHitAddFireRate;
                
                try 
                {
                    await UniTask.WaitForSeconds(fireRateBuffSO.applyTime, cancellationToken: cts);
                }
                catch (Exception e) 
                {
                    Debug.Log(e);
                    return;
                }
                finally 
                {
                    _owner.OnHit -= OnHitAddFireRate;
                    _totalFireRate = 0;
                }
            }
        }

        private void OnHitAddFireRate(Entity dealer, IDamageable target)
        {
            _totalFireRate += onHitFireRateAmount;
            _totalFireRate = Mathf.Min(_totalFireRate, maxFireRate);
            
            var targetStat = _stat.GetStat(fireRateStatSO);
            targetStat.RemoveModifier(this);
            targetStat.AddValueModifier(this,-_totalFireRate);
        }

        private async void PlayVFX()
        {
            _vfxComponent.PlayVFX("FireRate", transform.position, Quaternion.identity);
            await UniTask.WaitForSeconds(fireRateBuffSO.applyTime);
            _vfxComponent.StopVFX("FireRate");
            
        }
    }
}