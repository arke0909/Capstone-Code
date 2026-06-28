using Chipmunk.Library.Utility.GameEvents.Local;
using Scripts.Combat.Datas;

namespace SHS.Scripts.Crosshairs
{
    public struct CrosshairChangeEvent : ILocalEvent
    {
        public GunDataSO GunData { get; }
        public CrosshairSO CrosshairData { get; }

        public CrosshairChangeEvent(GunDataSO gunData, CrosshairSO crosshairData)
        {
            GunData = gunData;
            CrosshairData = crosshairData;
        }
    }
}
