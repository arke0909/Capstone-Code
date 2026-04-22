using Work.LKW.Code.Items;
using Chipmunk.Library.Utility.GameEvents.Local;

namespace Work.SHS.Items.Events
{
    public struct ItemEquippedEvent : ILocalEvent
    {
        public IEquipable EquipableItem { get; }

        public ItemEquippedEvent(IEquipable equipableItem)
        {
            EquipableItem = equipableItem;
        }
    }
}