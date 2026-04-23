using Work.LKW.Code.Items.ItemInfo;

namespace Work.LKW.Code.Items
{
    public class ArmorItem : EquipableItem
    {
        public int MaxDurability { get; set; }

        public ArmorItem(ItemDataSO itemData, int maxDurability) : base(itemData)
        {
            MaxDurability = maxDurability;
        }
    }
}