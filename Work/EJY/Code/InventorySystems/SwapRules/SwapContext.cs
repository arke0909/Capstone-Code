using Code.InventorySystems;
using Code.InventorySystems.Items;
using InGame.InventorySystem;
using Work.LKW.Code.Items;
using static Code.InventorySystems.InventoryUtility;

public class SwapContext
    {
        public ItemSlot StartSlot { get; }
        public ItemSlot TargetSlot { get; }

        public ItemBase StartItem { get; }
        public ItemBase TargetItem { get; }

        public Inventory StartInventory { get; }
        public Inventory TargetInventory { get; }

        public SlotType StartSlotType { get; }
        public SlotType TargetSlotType { get; }

        public int StartLocalIndex { get; }
        public int TargetLocalIndex { get; }

        public EquipSlot StartEquipSlot { get; }
        public EquipSlot TargetEquipSlot { get; }

        public bool IsSameInventory => StartInventory == TargetInventory;
        public bool IsSameSlot => StartSlot == TargetSlot;
        public bool IsTargetBlank => TargetItem == null;
        public bool IsStartBlank => StartItem == null;

        public bool IsStartEquip => StartEquipSlot != null;
        public bool IsTargetEquip => TargetEquipSlot != null;

        public bool IsStartHotbar => StartSlotType == SlotType.Hotbar;
        public bool IsTargetHotbar => TargetSlotType == SlotType.Hotbar;
        public bool IsStartStorage => StartSlotType == SlotType.Inventory;
        public bool IsTargetStorage => TargetSlotType == SlotType.Inventory;

        public SwapContext(ItemSlot startSlot, ItemSlot targetSlot)
        {
            StartSlot = startSlot;
            TargetSlot = targetSlot;

            StartItem = startSlot?.Item;
            TargetItem = targetSlot?.Item;

            StartInventory = startSlot?.OwnerInventory;
            TargetInventory = targetSlot?.OwnerInventory;

            StartSlotType = startSlot != null ? GetSlotType(startSlot.Index) : SlotType.None;
            TargetSlotType = targetSlot != null ? GetSlotType(targetSlot.Index) : SlotType.None;

            StartLocalIndex = startSlot != null ? GetLocalIndex(startSlot.Index) : -1;
            TargetLocalIndex = targetSlot != null ? GetLocalIndex(targetSlot.Index) : -1;

            if (StartSlotType == SlotType.Equip)
                StartEquipSlot = startSlot as EquipSlot;

            if (TargetSlotType == SlotType.Equip)
                TargetEquipSlot = targetSlot as EquipSlot;
        }
    }
