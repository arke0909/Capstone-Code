using Chipmunk.ComponentContainers;
using Code.InventorySystems.Equipments;
using Code.SHS.Entities.Enemies;
using Scripts.Combat.Datas;
using UnityEngine;
using Code.Items;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.SHS.Entities.Enemies.Events.Local;
using SHS.Scripts.Combats;

namespace Scripts.Combat
{
    public class AttackCompo : MonoBehaviour, IContainerComponent, ILocalEventSubscriber<EnemySpawnEvent>
    {
        public ComponentContainer ComponentContainer { get; set; }
        public bool IsAim { get; set; }
        public IAttackable CurrentAttackable
        {
            get
            {
                if (_enemyEquipment != null &&
                    _enemyEquipment.TryGetEquippedItem(EquipPartType.Hand, out var item) &&
                    item is IAttackable attackable)
                    return attackable;

                return _defaultAttack;
            }
        }

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

                return _defaultAttack != null ? _defaultAttack.AttackRange : 0f;
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
        private DefaultAttack _defaultAttack;
        public virtual void OnInitialize(ComponentContainer componentContainer)
        {
            _enemyEquipment = componentContainer.Get<EnemyEquipment>();
            componentContainer.TryGetComponent(out _defaultAttack);
        }

        public void OnLocalEvent(EnemySpawnEvent eventData)
        {
            IsAim = false;
        }
    }
}
