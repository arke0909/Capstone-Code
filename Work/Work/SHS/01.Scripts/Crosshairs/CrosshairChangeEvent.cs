using Chipmunk.Library.Utility.GameEvents.Local;
using Scripts.Combat.Datas;

namespace SHS.Scripts.Crosshairs
{
    public struct CrosshairChangeEvent : ILocalEvent
    {
        public GunDataSO GunData { get; }

        public CrosshairChangeEvent(GunDataSO gunData)
        {
            GunData = gunData;
        }
    }
}