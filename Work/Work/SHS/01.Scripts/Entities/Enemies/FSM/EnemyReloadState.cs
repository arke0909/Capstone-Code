using Ami.BroAudio;
using Chipmunk.ComponentContainers;
using Code.Players;
using Scripts.Combat.Datas;
using UnityEngine;
using Code.Combat;
using Code.InventorySystems.Equipments;
using SHS.Scripts.Entities.Players;
using Work.LKW.Code.Items;

namespace Code.SHS.Entities.Enemies.FSM
{
    public class EnemyReloadState : EnemyState
    {
        private GunItem _gun;
        private EnemyEquipment _equipment;
        private EntityGunStatInfo _entityGunStatInfo;
        private ItemGrabBehavior _itemGrabBehavior;

        private float _reloadTime;
        private float _currentTimer = 0;

        public EnemyReloadState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _equipment = container.Get<EnemyEquipment>();
            _entityGunStatInfo = container.Get<EntityGunStatInfo>();
            _itemGrabBehavior = container.Get<ItemGrabBehavior>();
        }

        public override void Enter()
        {
            base.Enter();
            _itemGrabBehavior?.SetWeight(0);
            _currentTimer = 0;
            _gun = null;

            if (_equipment.TryGetEquippedItem(EquipPartType.Hand, out EquipableItem item) && item is GunItem gun)
            {
                _gun = gun;
                _reloadTime = _gun.GunItemData.reloadTime;
                BroAudio.Play(_gun.GunItemData.reloadSound, _gun.Owner.transform.position);
            }
            else
            {
                Debug.Log("No equipment gun");
                _enemy.ChangeState(EnemyStateEnum.Patrol);
            }
        }

        public override void Update()
        {
            base.Update();

            _currentTimer += Time.deltaTime * _entityGunStatInfo.ReloadSpeedMultiplier;
            if (_currentTimer >= _reloadTime)
            {
                _enemy.ChangeState(Target ? EnemyStateEnum.Aim : EnemyStateEnum.Chase);
            }

            UpdateMovementAnimation();
        }

        public override void Exit()
        {
            if (_gun != null)
            {
                _enemyInventory.TryAddItem(_enemy.EnemyData.bulletData.CreateItem().Item,
                    _gun.GunItemData.maxAmmoCapacity);
                _gun.Reload();
            }
            _itemGrabBehavior?.SetWeight(1);

            base.Exit();
        }
    }
}