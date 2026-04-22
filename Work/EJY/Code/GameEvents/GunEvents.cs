using Chipmunk.GameEvents;
using Work.LKW.Code.Items;

namespace Code.GameEvents
{
    public struct AmmoUpdateEvent : IEvent
    {
        public int CurrentAmmo { get; private set; }
        public int TotalAmmo { get; private set; }

        public AmmoUpdateEvent(int currentAmmo, int totalAmmo)
        {
            CurrentAmmo = currentAmmo;
            TotalAmmo = totalAmmo;
        }
    }
    
    public struct ChangeHandlingEvent : IEvent
    {
        public EquipableItem EquipableItem { get; private set; }

        public ChangeHandlingEvent(EquipableItem equipableItem)
        {
            EquipableItem = equipableItem;
        }
    }
}