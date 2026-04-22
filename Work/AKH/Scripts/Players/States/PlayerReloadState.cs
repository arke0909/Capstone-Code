using Ami.BroAudio;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.Players;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;
using Code.Combat;
using Code.InventorySystems.Equipments;
using SHS.Scripts.Entities.Players;
using Work.Code.GameEvents;
using Work.LKW.Code.Items;

namespace Scripts.Players.States
{
    public class PlayerReloadState : PlayerMoveState
    {
        private GunItem _gun;
        private PlayerEquipment _equipment;
        private EntityGunStatInfo _entityGunStatInfo;
        private ItemGrabBehavior _itemGrabBehavior;

        private float _reloadTime;
        private readonly string _reloadText = "재장전..";

        public PlayerReloadState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _myMoveType = MoveType.Walk;
            _equipment = container.Get<PlayerEquipment>();
            _entityGunStatInfo = container.Get<EntityGunStatInfo>();
            _itemGrabBehavior = container.Get<ItemGrabBehavior>();
        }

        public override void Enter()
        {
            base.Enter();

            if (_equipment.TryGetEquippedItem(EquipPartType.Hand, out EquipableItem item) && item is GunItem gun)
            {
                _gun = gun;
                _reloadTime = _gun.GunItemData.reloadTime / _entityGunStatInfo.ReloadSpeedMultiplier;
                BroAudio.Play(_gun.GunItemData.reloadSound, _gun.ItemObject.transform.position);
                EventBus.Raise(new OffReplaceBulletUI());
                EventBus.Raise(new PlayerGageEvent(_reloadText, _reloadTime, HandleCompleteReload));
            }
            else
            {
                Debug.Log("No equipment gun");
                _player.ChangeState(PlayerStateEnum.Idle);
            }
        }

        public override void Update()
        {
            base.Update();
            //StopPlayerGageEvent로 게이지 중단 가능
        }

        private void HandleCompleteReload()
        {
            _player.ChangeState(PlayerStateEnum.Idle);
        }

        public override void Exit()
        {
            if (_gun == null)
            {
                Debug.LogError("총이 왜 없냐 예외처리 안함? 씁국현 진짜.");
                return;
            }

            _gun.Reload();
            EventBus.Raise(new AmmoUpdateEvent(_gun.CurrentBulletCnt, _gun.GunItemData.maxAmmoCapacity));
            base.Exit();
        }
    }
}