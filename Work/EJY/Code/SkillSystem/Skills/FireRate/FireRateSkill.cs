using System.Linq;
using Chipmunk.ComponentContainers;
using Chipmunk.Modules.StatSystem;
using Code.StatusEffectSystem;
using Code.StatusEffectSystem.StatusEffects;
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
        [SerializeField] private StatusEffectCreateData bulletReduceRate;
        [SerializeField] private StatSO fireRateStatSO;
        [SerializeField] private FireRateSkillVFX fireRateSkillVFX;
        [SerializeField] private Transform vfxPos;
        [SerializeField] private bool isOnHitAddFireRate;
        [SerializeField] private bool isBulletReduceRateDecrease;
        [SerializeField] private float onHitFireRateAmount = 0.025f, maxFireRate = 0.5f;
        
        private EntityStatusEffect _entityStatusEffect;
        private StatOverrideBehavior _stat;
        private VFXComponent _vfxComponent;
        private AbstractStatusEffect _statusEffect;
        private float _totalFireRate;
        
        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _entityStatusEffect = container.Get<EntityStatusEffect>();
            _stat = container.Get<StatOverrideBehavior>();
            _vfxComponent = container.Get<VFXComponent>();
        }

        private void OnDestroy()
        {
            if (_entityStatusEffect != null)
                _entityStatusEffect.OnStatusEffectReleased -= HandleFireRateReleased;

            if (_owner != null)
                _owner.OnAttack -= OnHitAddFireRate;

            _stat?.GetStat(fireRateStatSO)?.RemoveModifier(this);
        }

        private void UpgradeOnHitAddFireRate() => isOnHitAddFireRate = true;
        private void RollbackOnHitAddFireRate() => isOnHitAddFireRate = false;
        private void UpgradeBulletReduceRateDecrease() => isBulletReduceRateDecrease = true;
        private void RollbackBulletReduceRateDecrease() => isBulletReduceRateDecrease = false;


        public override void StartAndUseSkill()
        {
            _vfxComponent.PlayVFX("FireRate", vfxPos.position, Quaternion.identity);
            
            var statusEffectInfos = fireRateBuffSO.GetStatusEffectInfo();

            if(isBulletReduceRateDecrease)
                statusEffectInfos.Add(new StatusEffectInfo(fireRateBuffSO,bulletReduceRate));
            
            _statusEffect = _entityStatusEffect.AddStatusEffect(statusEffectInfos)
                .FirstOrDefault(statusEffect => statusEffect.StatusEffectEnum == StatusEffectEnum.FIRERATE_STATUS);
            
            if (isOnHitAddFireRate)
            {
                _owner.OnAttack += OnHitAddFireRate;
            }
            
            _entityStatusEffect.OnStatusEffectReleased += HandleFireRateReleased;
        }

        private void HandleFireRateReleased(AbstractStatusEffect statusEffect)
        {
            if(_statusEffect != statusEffect) return;
            
            if (isOnHitAddFireRate)
            {
                _owner.OnAttack -= OnHitAddFireRate;
                _totalFireRate = 0;
                
                var targetStat = _stat.GetStat(fireRateStatSO);
                targetStat.RemoveModifier(this);
            }
            _vfxComponent.StopVFX("FireRate");
            _entityStatusEffect.OnStatusEffectReleased -= HandleFireRateReleased;
        }

        private void OnHitAddFireRate(Entity dealer, IDamageable target)
        {
            if (Mathf.Approximately(_totalFireRate, maxFireRate))
                return;

            _totalFireRate = Mathf.Min(_totalFireRate + onHitFireRateAmount, maxFireRate);
            fireRateSkillVFX.SetRateOverTime(_totalFireRate / maxFireRate);

            var targetStat = _stat.GetStat(fireRateStatSO);
            targetStat.RemoveModifier(this);
            targetStat.AddValueModifier(this, -_totalFireRate);
        }
    }
}