using Code.Items.ItemInfo;

namespace Code.AirDrop
{
    public readonly struct SupplyReward
    {
        public ItemDataSO ItemData { get; }
        public int Stack { get; }

        public SupplyReward(ItemDataSO itemData, int stack)
        {
            ItemData = itemData;
            Stack = stack;
        }
    }
}
