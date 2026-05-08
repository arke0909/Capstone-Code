using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Work.LKW.Code.Items;
using Work.LKW.Code.Items.ItemInfo;
using Work.LKW.Code.ItemContainers;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Work.Code.GameEvents;

namespace Work.LKW.Code.ItemContainers
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
                List<ItemDataSO> targetItems = new List<ItemDataSO>();
                foreach (var type in container.GetAllowedTypes())
                {
                    targetItems.AddRange(itemDB.GetItemByType(type));
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