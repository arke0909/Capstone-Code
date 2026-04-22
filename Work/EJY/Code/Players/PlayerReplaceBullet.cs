using System;
using System.Collections.Generic;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InventorySystems.Equipments;
using Scripts.Combat.Datas;
using Scripts.Players;
using Scripts.Players.States;
using UnityEngine;

namespace Code.Players
{
    public struct ReplaceBulletData
    {
        public BulletItem bulletItem;
        public int bulletCnt;
    }
    
    public class PlayerReplaceBullet : MonoBehaviour, IContainerComponent
    {
        public ComponentContainer ComponentContainer { get; set; }
        
        private Player _player;
        private PlayerInventory _playerInventory;
        private PlayerEquipment _equipment;
        public void OnInitialize(ComponentContainer componentContainer)
        {
            _player = componentContainer.Get<Player>();
            _playerInventory = componentContainer.Get<PlayerInventory>();
            _equipment = componentContainer.Get<PlayerEquipment>();
            
            _player.PlayerInput.OnBulletShowPressed += HandleShowBullet;
            EventBus.Subscribe<ReplaceBulletEvent>(HandleReplaceBullet);
        }

        private void OnDestroy()
        {
            _player.PlayerInput.OnBulletShowPressed -= HandleShowBullet;
            EventBus.Unsubscribe<ReplaceBulletEvent>(HandleReplaceBullet);
        }
        
        public void HandleShowBullet()
        {
            if (_equipment.GetEquippedItem(EquipPartType.Hand) is not GunItem currentGun || _player.StateMachine.CurrentState is PlayerReloadState) return;

            List<ReplaceBulletData> data = new List<ReplaceBulletData>();
            HashSet<BulletDataSO> dataHash = new HashSet<BulletDataSO>();
            List<BulletItem> bullets = _playerInventory.GetItems<BulletItem>();

            if (bullets.Count == 0) return;

            void addBulletData(BulletItem bullet, int cnt)
            {
                if (dataHash.Add(bullet.bulletDataSO))
                    data.Add(new ReplaceBulletData { bulletItem = bullet, bulletCnt = cnt });
            }

            foreach (var bullet in bullets)
            {
                if (bullet.bulletDataSO.gunType == currentGun.GunItemData.gunType &&
                    !dataHash.Contains(bullet.bulletDataSO))
                {
                    int cnt = _playerInventory.GetItemCount(bullet.ItemData);
                    addBulletData(bullet, cnt);
                }
            }

            //총에 장착은 돼있지만 인벤토리에 없을 때
            if (currentGun.currentBulletItem != null && !_playerInventory.ContainsItem(currentGun.currentBulletItem))
                addBulletData(currentGun.currentBulletItem, 0);

            data.Sort((x, y) =>
            {
                int rarityCompare = x.bulletItem.ItemData.rarity.CompareTo(y.bulletItem.ItemData.rarity);
                if (rarityCompare != 0)
                    return rarityCompare;

                return string.Compare(
                    x.bulletItem.ItemData.itemName,
                    y.bulletItem.ItemData.itemName,
                    StringComparison.Ordinal
                );
            });

            int idx = 0;

            for (int i = 0; i < data.Count; i++)
            {
                if (currentGun.currentBulletItem != null &&
                    data[i].bulletItem.bulletDataSO == currentGun.currentBulletItem.bulletDataSO)
                {
                    idx = i;
                    break;
                }
            }

            EventBus.Subscribe<OffReplaceBulletUI>(HandleOffReplaceBulletUI);
            _player.PlayerInput.OnBulletShowPressed -= HandleShowBullet;
            _player.PlayerInput.OnBulletShowPressed += HandleCloseReplaceBulletUI;
            EventBus<ReplaceBulletListEvent>.Raise(new ReplaceBulletListEvent(data, idx));
        }

        private void HandleCloseReplaceBulletUI()
        {
            EventBus.Unsubscribe<OffReplaceBulletUI>(HandleOffReplaceBulletUI);
            _player.PlayerInput.OnBulletShowPressed += HandleShowBullet;
            _player.PlayerInput.OnBulletShowPressed -= HandleCloseReplaceBulletUI;
        }
        
        private void HandleOffReplaceBulletUI(OffReplaceBulletUI evt)
        {
            HandleCloseReplaceBulletUI();
        }
        
        private void HandleReplaceBullet(ReplaceBulletEvent evt)
        {
            var currentHandleItem = _equipment.GetEquippedItem(EquipPartType.Hand);

            if (currentHandleItem is GunItem gun && evt.Bullet.bulletDataSO != gun.currentBulletItem?.bulletDataSO)
            {
                gun.ChangeBullet(evt.Bullet);
                _player.ChangeState(PlayerStateEnum.Reload, true);
            }
        }
    }
}