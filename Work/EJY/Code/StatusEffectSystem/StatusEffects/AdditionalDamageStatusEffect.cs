using Assets.Work.AKH.Scripts.Entities.Vitals;
using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;

namespace Code.StatusEffectSystem.StatusEffects
{
    public class AdditionalDamageStatusEffect : AbstractStatusEffect
    {
        private HealthCompo _health;
        private DamageData _damageData;
        
        public AdditionalDamageStatusEffect(Entity target, StatusEffectInfo statusEffectInfo) : base(target, statusEffectInfo)
        {
            _damageData = new DamageData { damage = statusEffectInfo.Value, damageType = DamageType.DOT, defPierceLevel = 1 };
        }
        
        public override void ApplyStatusEffect(Entity entity)
        {
            base.ApplyStatusEffect(entity);

            _health = entity.ComponentContainer.Get<HealthCompo>();
            
            Debug.Assert(_health != null, "Entity has not Health compo");
            
            entity.OnHitEvent.AddListener(HandleOnHit);
        }

        public override void ReleaseStatusEffect(Entity entity)
        {
            entity.OnHitEvent.RemoveListener(HandleOnHit);
        }

        private void HandleOnHit()
        {
            _health.ApplyDamage(_damageData);
        }
    }
}