using Ami.BroAudio;
using Chipmunk.ComponentContainers;
using Code.Players;
using Scripts.Combat.Datas;
using UnityEngine;
using Code.Combat;
using Code.InventorySystems.Equipments;
using SHS.Scripts.Entities.Players;
using SHS.Scripts.Entities.Rigings;
using Code.Items;
using Scripts.Enemies.States;

namespace Code.SHS.Entities.Enemies.FSM
{
    public class EnemyReloadState : EnemyExecuteBehaviourState
    {
        private GunItem _gun;
        private EntityEquipment _equipment;
        private EntityGunStatInfo _entityGunStatInfo;
        private ItemGrabRiggingController _itemGrabRiggingController;

        private float _reloadTime;
        private float _currentReloadTimer = 0;

        public override float ExecuteTimer => 0.1f;

        public EnemyReloadState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _equipment = container.GetSubclassComponent<EntityEquipment>();
            _entityGunStatInfo = container.Get<EntityGunStatInfo>();
            _itemGrabRiggingController = container.Get<ItemGrabRiggingController>(true);
        }

        public override void Enter()
        {
            base.Enter();
            _itemGrabRiggingController?.SetWeight(0);
            _currentReloadTimer = 0;
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
            UpdateCurrentWeaponAttack(false);

            _currentReloadTimer += Time.deltaTime * _entityGunStatInfo.ReloadSpeedMultiplier;
            if (_currentReloadTimer >= _reloadTime)
            {
                _enemy.ChangeState(Target ? EnemyStateEnum.Aim : EnemyStateEnum.Chase);
            }

            UpdateMovementAnimation();
        }

        public override void Exit()
        {
            if (_gun != null)
            {
                BulletDataSO bulletData = _enemy.CurrentBulletData;
                if (bulletData != null)
                {
                    _enemyInventory.TryAddItem(bulletData.CreateItem().Item,
                        _gun.GunItemData.maxAmmoCapacity);
                }

                _gun.Reload();
            }
            _itemGrabRiggingController?.SetWeight(1);

            base.Exit();
        }
    }
}
