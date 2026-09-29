using Chipmunk.ComponentContainers;
using Code.StatusEffectSystem;
using UnityEngine;

namespace Scripts.Combat
{
    public abstract class BuffCaster : Caster
    {
        public virtual void ApplyBuff(
            Transform target,
            BuffSO buff,
            int level = 0,
            float additionalTime = 0f)
        {
            if (target.TryGetComponent(out ComponentContainer container)
                && container.TryGetComponent(out EntityStatusEffect buffCompo))
            {
                Debug.Log($"{target.name} has been applied to buff {buffCompo.GetType()}");
                buffCompo.AddStatusEffect(buff, this, level, additionalTime);
            }
        }
        public abstract bool CastBuff(
            Vector3 position,
            BuffSO buff,
            int level = 0,
            float additionalTime = 0f);

    }
}
