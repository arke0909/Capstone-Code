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
using SHS.Scripts.Entities.Rigings;
using Work.Code.GameEvents;
using Code.Items;

namespace Scripts.Players.States
{
    public class PlayerReloadState : PlayerMoveState
    {
        private GunItem _gun;
        private PlayerEquipment _equipment;
        private EntityGunStatInfo _entityGunStatInfo;
        private ItemGrabRiggingController _itemGrabBehavior;

        private float _reloadTime;
        private readonly string _reloadText = "재장전..";
        private bool _isReloadCompleted;

        public PlayerReloadState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _myMoveType = MoveType.Walk;
            _equipment = container.Get<PlayerEquipment>();
            _entityGunStatInfo = container.Get<EntityGunStatInfo>();
            _itemGrabBehavior = container.Get<ItemGrabRiggingController>(true);
        }

        public override void Enter()
        {
            base.Enter();
            _gun = null;
            _isReloadCompleted = false;

            if (_equipment.TryGetEquippedItem(EquipPartType.Hand, out EquipableItem item) && item is GunItem gun)
            {
                _gun = gun;
                float reloadSpeedMultiplier = Mathf.Max(_entityGunStatInfo.ReloadSpeedMultiplier, 0.01f);
                _reloadTime = _gun.GunItemData.reloadTime / reloadSpeedMultiplier;
                BroAudio.Play(_gun.GunItemData.reloadSound, _gun.ItemObject.transform.position);
                _player.LocalEventBus.Raise(new OffReplaceBulletUI());
                EventBus.Raise(new PlayerGageEvent(_reloadText, _reloadTime, HandleCompleteReload));
            }
            else
            {
                Debug.Log("No equipment gun");
                _player.ChangeState(PlayerStateEnum.Idle);
            }
        }

        private void HandleCompleteReload()
        {
            if (_gun == null || !_gun.CanReload)
            {
                _player.LocalEventBus.Raise(new AmmoUpdateEvent(_gun?.CurrentBulletCnt ?? 0, _gun?.GunItemData.maxAmmoCapacity ?? 0));
                _player.ChangeState(PlayerStateEnum.Idle);
                return;
            }

            _isReloadCompleted = true;
            _player.ChangeState(PlayerStateEnum.Idle);
        }

        public override void Exit()
        {
            if (_gun == null)
            {
                Debug.LogError("총이 왜 없냐 예외처리 안함? 씁국현 진짜.");
                EventBus.Raise(new StopPlayerGageEvent());
                base.Exit();
                return;
            }

            if (_isReloadCompleted)
            {
                _gun.Reload();
                _player.LocalEventBus.Raise(new AmmoUpdateEvent(_gun.CurrentBulletCnt, _gun.GunItemData.maxAmmoCapacity));
            }
            else
            {
                EventBus.Raise(new StopPlayerGageEvent());
            }

            _gun = null;
            base.Exit();
        }
    }
}
