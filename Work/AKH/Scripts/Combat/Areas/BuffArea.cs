using Code.StatusEffectSystem;
using UnityEngine;

namespace Scripts.Combat.Areas
{
    public class BuffArea : Area
    {
        [SerializeField] private BuffCaster buffCaster;
        [SerializeField] private BuffSO targetBuff;
        [SerializeField] private float buffDuration;
        protected override void TickElapsed()
        {
            buffCaster.CastBuff(transform.position, targetBuff.GetStatusEffectInfo());
        }
    }
}
