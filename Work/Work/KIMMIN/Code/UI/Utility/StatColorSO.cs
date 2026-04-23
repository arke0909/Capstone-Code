using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Work.Code.UI.Utility
{
    public enum StatType
    {
        Damage,
        FireRate,
        ReloadTime,
        BulletSpeed,
        BulletCount,
        FireCount,
        Range,
        AimSpeed,
        VerticalRecoil,
        HorizontalRecoil,
        Spread,
        Capacity,
        Health,
        Defense,
        Hunger,
        Thirst
    }
    
    [System.Serializable]
    public class StatSetting
    {
        public StatType statType;

        public string label;
        public string prefix; 
        public string suffix; 
        public bool invert;

        [FormerlySerializedAs("negMin")]
        public float badMin;
        [FormerlySerializedAs("negMax")]
        public float badMax;
        [FormerlySerializedAs("posMin")]
        public float goodMin;
        [FormerlySerializedAs("posMax")]
        public float goodMax;
    }
    
    [CreateAssetMenu(menuName = "SO/StatColorSettings")]
    public class StatColorSO : ScriptableObject
    {
        public List<StatSetting> settings;

        private Dictionary<StatType, StatSetting> _map;

        public void Init()
        {
            _map = new Dictionary<StatType, StatSetting>();

            foreach (var s in settings)
            {
                _map[s.statType] = s;
            }
        }

        public StatSetting Get(StatType type)
        {
            if (_map == null) Init();
            return _map[type];
        }
    }
}