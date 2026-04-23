using System;

namespace Scripts.Combat.Datas
{
    [Flags]
    public enum DamageType
    {
        None = 0,
        MELEE = 1,
        RANGE = 2,
        MAGIC = 4,
        DOT = 8,
    }
    public struct DamageData
    {
        public float damage;
        public int defPierceLevel;
        public DamageType damageType;
    }
}
