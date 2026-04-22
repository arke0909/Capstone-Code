using Assets.Work.AKH.Scripts.Entities.Vitals;
using Chipmunk.ComponentContainers;
using Scripts.Entities;
using UnityEngine;

namespace Code.StatusEffectSystem.StatusEffects
{
    public class DamageStoringStatusEffect : AbstractStatusEffect
    {
        private float _storedDamage = 0;
        private HealthCompo _healthCompo;
        
        public DamageStoringStatusEffect(Entity target, StatusEffectInfo statusEffectInfo) : base(target, statusEffectInfo)
        {
            _healthCompo = target.Get<HealthCompo>();
            SetValue(statusEffectInfo.Value);
        }

        public override void ApplyStatusEffect(Entity entity)
        {
            base.ApplyStatusEffect(entity);

            _healthCompo.OnTakeDamage += HandleStoringDamage;
        }

        public override void ReleaseStatusEffect(Entity entity)
        {
            
            _healthCompo.CurrentValue+=Mathf.RoundToInt(_storedDamage);
            _healthCompo.OnTakeDamage -= HandleStoringDamage;
        }

        private void HandleStoringDamage(float damage)
        {
            _storedDamage += damage * _value;
        }
    }
}