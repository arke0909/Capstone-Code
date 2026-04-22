using Work.LKW.Code.Items;
using Chipmunk.Library.Utility.GameEvents.Local;

namespace Work.SHS.Items.Events
{
    public struct ItemUnEquippedEvent : ILocalEvent
    {
        public IEquipable EquipableItem { get; }

        public ItemUnEquippedEvent(IEquipable equipableItem)
        {
            EquipableItem = equipableItem;
        }
    }
}