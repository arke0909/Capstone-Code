using Chipmunk.ComponentContainers;
using Scripts.Modules.Blackboards;
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
            IBlackboardOwner blackboardOwner = componentContainer.GetSubclassComponent<IBlackboardOwner>();
            Blackboard blackboard = blackboardOwner?.Blackboard;
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
                blackboard?.Set<StatSO>(stat.statName, stat);
            }
        }
    }
}