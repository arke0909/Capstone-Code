using System;
using Chipmunk.ComponentContainers;
using Code.GameEvents;
using Code.InventorySystems.Equipments;
using Code.Items;
using Code.Items.ItemInfo;
using Code.Players;
using Code.UI.Core;
using DewmoLib.Dependencies;
using Scripts.Combat.Datas;
using Work.Code.Craft;

namespace Code.UI.Inventory
{
    public class RightInventoryPanel : InventoryPanel<UpdateRightInventoryUIEvent>
    {
        [Inject] private CraftPinItemContainer _craftPinItemContainer;
        private PlayerEquipment _playerEquipment;

        private void Start()
        {
            _playerEquipment = _player.Get<PlayerEquipment>();

            if (_craftPinItemContainer != null)
                _craftPinItemContainer.OnChanged += RefreshHighlights;
        }

        protected override void OnDestroy()
        {
            if (_craftPinItemContainer != null)
                _craftPinItemContainer.OnChanged -= RefreshHighlights;

            base.OnDestroy();
        }

        protected override void UpdateSlotUI()
        {
            base.UpdateSlotUI();
            RefreshHighlights();
        }

        private void RefreshHighlights()
        {
            if (_slotUIs == null)
                return;

            StopAllHighlightSlots();
            HighlightPinnedCraftItems();
            HighlightCurrentGunBullets();
        }

        private void HighlightCurrentGunBullets()
        {
            if (_playerEquipment == null)
                return;
            
            EquipableItem equipItem = _playerEquipment.GetEquippedItem(EquipPartType.Hand);
            GunDataSO gunData = equipItem?.ItemData as GunDataSO;
            if(gunData == null) return;

            foreach (var slot in _slotUIs)
            {
                ItemBase item = slot.ItemSlot?.Item;
                if (item is { ItemData: BulletDataSO bulletData }
                    && bulletData.gunType == gunData.gunType)
                {
                    slot.PlayBackgroundEffect(UIDefine.GreenColor);
                }
            }
        }

        private void HighlightPinnedCraftItems()
        {
            if (_craftPinItemContainer == null)
                return;

            foreach (var slot in _slotUIs)
            {
                ItemBase item = slot.ItemSlot?.Item;

                if (item != null && _craftPinItemContainer.Contains(item.ItemData))
                {
                    slot.PlayBackgroundEffect(UIDefine.GreenColor);
                }
            }
        }
    }
}
