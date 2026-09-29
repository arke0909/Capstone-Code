using Chipmunk.GameEvents;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.Items;
using Scripts.Entities.Vitals;

namespace Code.GameEvents
{
    public struct AmmoUpdateEvent : IVitalEvent
    {
        public int CurrentAmmo { get; private set; }
        public int TotalAmmo { get; private set; }

        public float Value => CurrentAmmo;

        public float MaxValue => TotalAmmo;

        public AmmoUpdateEvent(int currentAmmo, int totalAmmo)
        {
            CurrentAmmo = currentAmmo;
            TotalAmmo = totalAmmo;
        }

        public void Init(float value, float maxValue)
        {
            CurrentAmmo = (int)value;
            TotalAmmo = (int)maxValue;
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