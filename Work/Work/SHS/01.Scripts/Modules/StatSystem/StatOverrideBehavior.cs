using Chipmunk.ComponentContainers;
using UnityEngine;

namespace Chipmunk.Modules.StatSystem
{
    public class StatOverrideBehavior : StatBehavior
    {
        public StatOverride[] StatOverrides => statOverrides;
        [SerializeField] private StatOverride[] statOverrides;

        public override void OnInitialize(ComponentContainer componentContainer)
        {
            base.OnInitialize(componentContainer);

            stats.Clear();
            if (statOverrides == null || statOverrides.Length == 0)
            {
                return;
            }

            for (int index = 0; index < statOverrides.Length; index++)
            {
                StatOverride statOverride = statOverrides[index];

                StatSO stat = statOverride.CreateStat();
                AddStat(stat);
            }
        }
    }
}