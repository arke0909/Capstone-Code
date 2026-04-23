using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities;

namespace Code.StatusEffectSystem.StatusEffects
{
    public class AdditionalDamageStatusEffect : AbstractStatusEffect
    {
        private readonly DamageData _damageData;
        
        public AdditionalDamageStatusEffect(Entity target, StatusEffectInfo statusEffectInfo) : base(target, statusEffectInfo)
        {
            _damageData = new DamageData { damage = statusEffectInfo.Value, damageType = DamageType.DOT, defPierceLevel = 1 };
        }
        
        public override void ApplyStatusEffect(Entity entity)
        {
            base.ApplyStatusEffect(entity);
            entity.OnHit += HandleOnHit;
        }

        public override void ReleaseStatusEffect(Entity entity)
        {
            entity.OnHit -= HandleOnHit;
        }

        private void HandleOnHit(Entity dealer, IDamageable target)
        {
            target?.ApplyDamage(_damageData, dealer);
        }
    }
}