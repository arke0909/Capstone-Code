using System;
using System.Collections.Generic;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;

namespace Chipmunk.Modules.StatSystem
{
    [Serializable]
    public struct StatOverride : IEquatable<StatOverride>
    {
        [SerializeField] private StatSO stat;
        public StatSO Stat => stat;
        [SerializeField] private bool isUseOverride;
#if ODIN_INSPECTOR
        [ShowIf("isUseOverride")]
#endif
        [SerializeField]
        private float overrideValue;

        public StatOverride(StatSO stat)
        {
            isUseOverride = false;
            overrideValue = 0;
            this.stat = stat;
        }

        public StatSO CreateStat()
        {
            if (stat == null)
            {
                Debug.LogWarning("StatOverride::CreateStat : stat is null");
                return null;
            }

            StatSO newStat = stat.Clone() as StatSO;
            ApplyOverride(newStat);
            return newStat;
        }

        public void ApplyOverride(StatSO targetStat)
        {
            if (targetStat == null)
            {
                Debug.LogWarning("StatOverride::ApplyOverride : target stat is null");
                return;
            }

            if (isUseOverride)
            {
                targetStat.BaseValue = overrideValue;
            }
        }

        public bool Equals(StatOverride other)
        =>  stat == other.stat;
        public override bool Equals(object obj)
        =>  obj is StatOverride other && Equals(other);
        public override int GetHashCode()
        =>  stat != null ? stat.GetInstanceID() : 0;

        public static bool operator ==(StatOverride left, StatOverride right) => left.Equals(right);
        public static bool operator !=(StatOverride left, StatOverride right) => !left.Equals(right);
    }
}