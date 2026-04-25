using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.Hotbar;
using Code.InventorySystems.Items;
using Code.Players;
using InGame.InventorySystem;
using Scripts.Combat.Datas;
using UnityEngine;
using Work.LKW.Code.Items;
using static Code.InventorySystems.InventoryUtility;

namespace Code.InventorySystems
{
    public class InventorySwapInteract : MonoBehaviour, IContainerComponent
    {
        public ComponentContainer ComponentContainer { get; set; }
        private Inventory _inventory;

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _inventory = componentContainer.GetSubclassComponent<Inventory>();

            EventBus.Subscribe<SwapItemSlotEvent>(HandleSwapItemSlot);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<SwapItemSlotEvent>(HandleSwapItemSlot);
        }

        private bool IsResponsibleForSwap(ItemSlot startSlot, ItemSlot targetSlot)
        {
            Inventory eventOwner = startSlot.OwnerInventory != null ? startSlot.OwnerInventory : targetSlot.OwnerInventory;

            if (eventOwner != null)
                return eventOwner == _inventory;

            return _inventory is PlayerInventory;
        }

        private static void UpdateRelatedInventories(ItemSlot startSlot, ItemSlot targetSlot)
        {
            startSlot.OwnerInventory?.UpdateInventory();

            if (targetSlot.OwnerInventory != null && targetSlot.OwnerInventory != startSlot.OwnerInventory)
                targetSlot.OwnerInventory.UpdateInventory();
        }

        private void HandleSwapItemSlot(SwapItemSlotEvent evt)
        {
            ItemSlot startSlot = evt.StartSlot;
            ItemSlot targetSlot = evt.TargetSlot;

            if (startSlot == null || targetSlot == null)
            {
                Debug.Log("start slot or target slot is null");
                return;
            }

            if (startSlot == targetSlot)
                return;

            if (!IsResponsibleForSwap(startSlot, targetSlot))
                return;

            ItemBase startSlotItem = startSlot.Item;
            ItemBase targetSlotItem = targetSlot.Item;

            if (TryGetSlot(startSlot, SlotType.Equip, out EquipSlot startEquip) && TryGetSlot(targetSlot, SlotType.Equip,out EquipSlot targetEquip))
            {
                if (targetEquip.CanEquip(startSlotItem) && startEquip.CanEquip(targetSlotItem))
                    EventBus.Raise(new SwapEquipEvent(startEquip, targetEquip));
            }
            else if (TryGetSlot(startSlot, SlotType.Equip, out EquipSlot startEquipSlot))
            {
                if (targetSlotItem == null)
                    EventBus.Raise(new UnEquipByDragEvent(startSlotItem, startEquipSlot, targetSlot));
            }
            else if (TryGetSlot(targetSlot,SlotType.Equip ,out EquipSlot targetEquipSlot))
            {
                if (targetEquipSlot.CanEquip(startSlotItem))
                {
                    void OnEquipByDragSuccess()
                    {
                        if (startSlot.OwnerInventory != targetSlot.OwnerInventory)
                            startSlot.OwnerInventory.RemoveItem(startSlotItem, 1, false);
                    }

                    EventBus.Raise(
                        new EquipByDragEvent(startSlotItem, GetLocalIndex(targetEquipSlot.Index), startSlot, OnEquipByDragSuccess));
                }
            }
            else if (CheckSlotType(startSlot.Index, SlotType.Hotbar))
            {
                if (targetSlotItem == null && startSlot.OwnerInventory == targetSlot.OwnerInventory)
                    EventBus.Raise(new UnEquipHotbarEvent(GetLocalIndex(startSlot.Index)));
            }
            else if (CheckSlotType(targetSlot.Index, SlotType.Hotbar))
            {
                if (targetSlotItem == null &&
                    startSlot.OwnerInventory == targetSlot.OwnerInventory &&
                    startSlotItem is ThrowableItem or UsableItem)
                {
                    EventBus.Raise(new EquipHotbarEvent(GetLocalIndex(targetSlot.Index), startSlotItem));
                }
            }
            else
            {
                int targetSlotStack = targetSlot.Stack;
                int startSlotStack = startSlot.Stack;

                startSlot.SetData(targetSlotItem, targetSlotStack);
                targetSlot.SetData(startSlotItem, startSlotStack);
            }

            UpdateRelatedInventories(startSlot, targetSlot);
        }
    }
}