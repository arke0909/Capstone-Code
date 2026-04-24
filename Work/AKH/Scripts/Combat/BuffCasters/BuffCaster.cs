using System.Collections;
using System.Collections.Generic;
using Chipmunk.ComponentContainers;
using Code.StatusEffectSystem;
using UnityEngine;

namespace Scripts.Combat
{
    public abstract class BuffCaster : Caster
    {
        public virtual void ApplyBuff(Transform target, IEnumerable<StatusEffectInfo> infos)
        {
            if (target.TryGetComponent(out ComponentContainer container)
                && container.TryGetComponent(out EntityStatusEffect buffCompo))
            {
                Debug.Log($"{target.name} has been applied to buff {buffCompo.GetType()}");
                buffCompo.AddStatusEffect(infos);
            }
        }
        public abstract bool CastBuff(Vector3 position, IEnumerable<StatusEffectInfo> infos);

    }
}