using Scripts.Combat.Datas;
using Chipmunk.Library.Utility.GameEvents.Local;
using UnityEngine;

namespace SHS.Scripts.Combats.Events
{
    public struct GunAttackEvent : ILocalEvent
    {
        public GunDataSO GunData { get; }
        public Vector3 Position { get; }
        public float VerticalRecoil { get; }
        public float HorizontalRecoil { get; }

        public GunAttackEvent(GunDataSO gunData, Vector3 position, float verticalRecoil, float horizontalRecoil)
        {
            GunData = gunData;
            Position = position;
            VerticalRecoil = verticalRecoil;
            HorizontalRecoil = horizontalRecoil;
        }
    }
}
