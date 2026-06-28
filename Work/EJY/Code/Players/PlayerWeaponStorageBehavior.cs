using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.GameEvents;
using Code.InventorySystems;
using Code.Items;
using UnityEngine;
using static Code.InventorySystems.InventoryUtility;

namespace Code.Players
{
    public class PlayerWeaponStorageBehavior : MonoBehaviour, IContainerComponent,
        ILocalEventSubscriber<EquipItemEvent>,
        ILocalEventSubscriber<UnequipItemEvent>,
        ILocalEventSubscriber<ChangeHandlingEvent>
    {
        [SerializeField] private Transform[] storageParents;

        private PlayerEquipment _playerEquipment;
        private HandlingComponent _handlingComponent;
        private GameObject[] _storageObjects;
        private Weapon[] _storedWeapons;

        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _playerEquipment = componentContainer.Get<PlayerEquipment>();
            _handlingComponent = componentContainer.Get<HandlingComponent>();
            _storageObjects = new GameObject[storageParents.Length];
            _storedWeapons = new Weapon[storageParents.Length];

            EventBus<SwapEquipEvent>.OnComplete += HandleSwapEquipCompleted;
        }

        private void Start()
        {
            RefreshStorage();
        }

        private void OnDestroy()
        {
            EventBus<SwapEquipEvent>.OnComplete -= HandleSwapEquipCompleted;
        }

        public void OnLocalEvent(EquipItemEvent eventData)
        {
            RefreshStorage();
        }

        public void OnLocalEvent(UnequipItemEvent eventData)
        {
            RefreshStorage();
        }

        public void OnLocalEvent(ChangeHandlingEvent eventData)
        {
            RefreshStorage();
        }

        private void HandleSwapEquipCompleted(SwapEquipEvent evt)
        {
            RefreshStorage();
        }

        private void RefreshStorage()
        {
            foreach (var equipSlot in _playerEquipment.EquipSlots)
            {
                if (!equipSlot.CanHandle)
                    continue;

                int localIndex = GetLocalIndex(equipSlot.Index);
                Weapon weapon = equipSlot.Equipable as Weapon;

                if (weapon == null || weapon == _handlingComponent.CurrentHandItem)
                {
                    ClearStorage(localIndex);
                    continue;
                }

                if (_storedWeapons[localIndex] == weapon)
                    continue;

                ClearStorage(localIndex);
                CreateStorage(localIndex, weapon);
            }
        }

        private void CreateStorage(int localIndex, Weapon weapon)
        {
            GameObject storageObject = Instantiate(
                weapon.EquipItemData.equipmentPrefab,
                storageParents[localIndex],
                false);
            RemoveRuntimeBehaviours(storageObject);

            _storageObjects[localIndex] = storageObject;
            _storedWeapons[localIndex] = weapon;
        }

        private void RemoveRuntimeBehaviours(GameObject storageObject)
        {
            MonoBehaviour[] behaviours = storageObject.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                Destroy(behaviours[i]);
            }
        }

        private void ClearStorage(int localIndex)
        {
            if (_storageObjects[localIndex] != null)
                Destroy(_storageObjects[localIndex]);

            _storageObjects[localIndex] = null;
            _storedWeapons[localIndex] = null;
        }
    }
}
