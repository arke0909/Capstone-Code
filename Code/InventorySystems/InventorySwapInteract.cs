using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.Hotbar;
using Code.InventorySystems.Items;
using InGame.InventorySystem;
using Scripts.Combat.Datas;
using UnityEngine;
using Work.LKW.Code.Items;

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
            {
                return;
            }

            ItemBase startSlotItem = startSlot.Item;
            ItemBase targetSlotItem = targetSlot.Item;

            if (startSlot is EquipSlot startEquip && targetSlot is EquipSlot targetEquip)
            {
                if (targetEquip.CanEquip(startSlotItem) && startEquip.CanEquip(targetSlotItem))
                {
                    EventBus.Raise(new SwapEquipEvent(startEquip, targetEquip));
                }
            }
            else if (startSlot is EquipSlot startEquipSlot)
            {
                if (targetSlotItem == null)
                    EventBus.Raise(new UnEquipByDragEvent(startSlotItem, startEquipSlot, targetSlot));
            }
            // 드래그 끝점이 장착UI일 때
            else if (targetSlot is EquipSlot equipSlot)
            {
                if (equipSlot.CanEquip(startSlotItem))
                {
                    void OnEquipByDragSuccess()
                    {
                        // 다른 상자에서 무기를 바로 장착할 때
                        if (startSlot.OwnerInventory != targetSlot.OwnerInventory)
                        {
                            startSlot.OwnerInventory.RemoveItem(startSlotItem, 1, false);
                        }
                    }

                    EventBus.Raise(
                        new EquipByDragEvent(startSlotItem, equipSlot.Index, startSlot, OnEquipByDragSuccess));
                }
            }
            else if (startSlot is HotbarSlot unquipSlot)
            {
                if (targetSlotItem == null && startSlot.OwnerInventory == targetSlot.OwnerInventory)
                    EventBus.Raise(new UnEquipHotbarEvent(unquipSlot.Index));
            }
            else if (targetSlot is HotbarSlot hotbarSlot)
            {
                if (targetSlotItem == null && startSlot.OwnerInventory == targetSlot.OwnerInventory &&
                    startSlotItem is ThrowableItem or UsableItem)
                    EventBus.Raise(new EquipHotbarEvent(hotbarSlot.Index, startSlotItem));
            }
            else
            {
                int targetSlotStack = targetSlot.Stack;
                int startSlotStack = startSlot.Stack;

                startSlot.SetData(targetSlotItem, targetSlotStack);
                targetSlot.SetData(startSlotItem, startSlotStack);
            }

            _inventory.UpdateInventory();
        }
    }
}