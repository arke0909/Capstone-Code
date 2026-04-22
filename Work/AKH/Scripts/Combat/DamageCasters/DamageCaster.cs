using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;

namespace Scripts.Combat
{
    public abstract class DamageCaster : Caster
    {

        protected Entity _owner;

        public virtual void InitCaster(Entity entity)
        {
            _owner = entity;
        }
        public virtual void ApplyDamageAndKnockback(Transform target, DamageData damageData, Vector3 position, Vector3 normal, MovementDataSO knockbackData = null)
        {
            if (target.TryGetComponent(out IDamageable damageable))
            {
                DamageContext context = new DamageContext
                {
                    DamageData = damageData,
                    HitPoint = position,
                    HitNormal = normal,
                    Source = _owner.gameObject,
                    Attacker = _owner
                };
                    
                damageable.ApplyDamage(context);
            }
            if (knockbackData != null && target.TryGetComponent(out IKnockbackable knockbackable))
            {
                //    Vector3 force = transform.forward * attackData.knockBackForce;
                knockbackable.KnockBack(transform.forward, knockbackData);
            }
        }
        public abstract bool CastDamage(DamageData damageData, Vector3 position, Vector3 direction, MovementDataSO knockBackData);
    }
}
