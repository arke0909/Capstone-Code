using System;
using Chipmunk.Modules.StatSystem;
using Code.TimeSystem;
using Scripts.Entities;
using UnityEngine;

namespace SHS.Scripts.Entities.Levels.Growths
{
    [CreateAssetMenu(fileName = "StatGrowthSO", menuName = "ScriptableObject/StatGrowthSO")]
    public class StatGrowthSO : BaseGrowthSO
    {
        [Serializable]
        private class StatGrowthData
        {
            public StatSO targetStat;
            public float additiveValue = 0;
            public float multiplierValue = 1;
        }

        [SerializeField] private StatGrowthData[] _statGrowthDatas;

        protected override void ApplyGrowthEffect(Entity entity)
        {
            int currentDay = TimeController.Instance.CurrentDay;
            foreach (var data in _statGrowthDatas)
            {
                var statOverrideBehavior = entity.ComponentContainer.Get<StatOverrideBehavior>();
                StatSO targetStat = statOverrideBehavior.GetStat(data.targetStat);
                targetStat.AddValueModifier(this, data.additiveValue);
                targetStat.AddPercentModifier(this, data.multiplierValue);
            }
        }
    }
}