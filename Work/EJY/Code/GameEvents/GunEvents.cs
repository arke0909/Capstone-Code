using Chipmunk.GameEvents;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.Items;

namespace Code.GameEvents
{
    public struct AmmoUpdateEvent : ILocalEvent
    {
        public int CurrentAmmo { get; private set; }
        public int TotalAmmo { get; private set; }

        public AmmoUpdateEvent(int currentAmmo, int totalAmmo)
        {
            CurrentAmmo = currentAmmo;
            TotalAmmo = totalAmmo;
        }
    }
    
    public struct ChangeHandlingEvent : ILocalEvent
    {
        public EquipableItem EquipableItem { get; private set; }

        public ChangeHandlingEvent(EquipableItem equipableItem)
        {
            EquipableItem = equipableItem;
        }
    }
}