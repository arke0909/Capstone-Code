using Chipmunk.GameEvents;
using InGame.InventorySystem;

namespace Code.GameEvents
{
    public struct SwapEquipEvent : IEvent
    {
        public EquipSlot StartEquip { get; private set; }
        public EquipSlot TargetEquip { get; private set; }
        
        public SwapEquipEvent(EquipSlot startEquip, EquipSlot targetEquip)
        {
            StartEquip = startEquip;
            TargetEquip = targetEquip;
        }
    }
}