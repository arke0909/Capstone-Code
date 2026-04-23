using Scripts.Combat.Datas;
using Chipmunk.Library.Utility.GameEvents.Local;
using UnityEngine;

namespace SHS.Scripts.Combats.Events
{
    public struct GunAttackEvent : ILocalEvent
    {
        public GunDataSO GunData { get; }
        public float SpreadAngleDeg { get; }
        public Vector3 Position { get; }

        public GunAttackEvent(GunDataSO gunData, float spreadAngleDeg, Vector3 position)
        {
            GunData = gunData;
            SpreadAngleDeg = spreadAngleDeg;
            Position = position;
        }
    }
}
