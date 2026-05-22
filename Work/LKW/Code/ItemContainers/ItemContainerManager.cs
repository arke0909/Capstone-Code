using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Code.Items;
using Code.Items.ItemInfo;
using Chipmunk.GameEvents;
using Work.Code.GameEvents;

namespace Code.ItemContainers
{
    public class ItemContainerManager : MonoBehaviour
    {
        [SerializeField] private ItemDataBaseSO itemDB;

        private List<ItemContainer> _allContainers = new List<ItemContainer>();


        private void Start()
        {
            _allContainers = FindObjectsByType<ItemContainer>(FindObjectsSortMode.None).ToList();
            SetUpContainers();
        }

        private void OnEnable()
        {
            Bus.Subscribe<DayChangeEvent>(HandleDayChange);
        }

        private void OnDisable()
        {
            Bus.Unsubscribe<DayChangeEvent>(HandleDayChange);
        }

        private void HandleDayChange(DayChangeEvent evt)
        {
            SetUpContainers();
        }

        private void SetUpContainers()
        {
            foreach (var container in _allContainers)
            {
                if(container.IsSelfInitialized) continue;
                
                List<ItemDataSO> targetItems = new List<ItemDataSO>();
                foreach (var type in container.GetAllowedTypes())
                {
                    targetItems.AddRange(itemDB.GetItemsByType(type));
                }

                int count = container.GetRandomCount();
                List<ItemDataSO> resultItems = new List<ItemDataSO>();
                
                resultItems.AddRange(itemDB.GetRandomItems(targetItems, container.AllowedSpawnArea, count));

                var inventory = container.Inventory;
                
                if(resultItems.Count > 0)
                    inventory.SetUpItem(resultItems);
            }
        }
    }
}