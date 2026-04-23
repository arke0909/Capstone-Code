using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Work.LKW.Code.Items;
using Work.LKW.Code.Items.ItemInfo;
using Work.LKW.Code.ItemContainers;

namespace Work.LKW.Code.ItemContainers
{
    public class ItemContainerManager : MonoBehaviour
    {
        [SerializeField] private ItemDataBaseSO itemDB;

        private void Start()
        {
            SetUpContainers();
        }

        private void SetUpContainers()
        {
            ItemContainer[] allContainers = FindObjectsOfType<ItemContainer>();

            foreach (var container in allContainers)
            {
                List<ItemDataSO> targetItems = new List<ItemDataSO>();
                foreach (var type in container.GetAllowedTypes())
                {
                    targetItems.AddRange(itemDB.GetItemByType(type));
                }

                int count = container.GetRandomCount();
                List<ItemDataSO> resultItems = new List<ItemDataSO>();
                resultItems.AddRange(itemDB.GetRandomItems(targetItems, container.AllowedSpawnArea, count));


                container.SetUpItem(resultItems);
            }
        }
    }
}