using Chipmunk.ComponentContainers;
using Code.InventorySystems.Equipments;
using Code.Players;
using Code.SHS.Entities.Enemies;
using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;
using Work.LKW.Code.Items;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.SHS.Entities.Enemies.Events.Local;

namespace Scripts.Combat
{
    public class AttackCompo : MonoBehaviour, IContainerComponent, ILocalEventSubscriber<EnemySpawnEvent>
    {
        public ComponentContainer ComponentContainer { get; set; }
        public bool IsAim { get; set; }
        public float AttackRange
        {
            get
            {
                if (_enemyEquipment != null && _enemyEquipment.TryGetEquippedItem(EquipPartType.Hand, out var weapon))
                {
                    if (weapon is Weapon gunItem)
                    {
                        return gunItem.WeaponData.attackRange;
                    }
                }

                return 0f;
            }
            set { }
        }
        public T GetCurrentWeapon<T>() where T : Weapon
        {
            _enemyEquipment.TryGetEquippedItem(EquipPartType.Hand, out var weapon);
            if (weapon is T target)
                return target;
            return null;
        }
        
        private EnemyEquipment _enemyEquipment;
        private Entity _entity;
        public virtual void OnInitialize(ComponentContainer componentContainer)
        {
            _enemyEquipment = componentContainer.Get<EnemyEquipment>();
        }

        public void OnLocalEvent(EnemySpawnEvent eventData)
        {
            IsAim = false;
        }
    }
}
