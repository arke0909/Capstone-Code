using System;
using Code.Items;
using Code.Items.ItemInfo;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.ETC.MapObjects
{
    [Serializable]
    public class FixedItemDropSetting
    {
        public ItemDataSO fixedItem;
        [Min(1)] public int minDropCount = 1;
        [Min(1)] public int maxDropCount = 1;
        [Range(0f, 1f)] public float probability = 1f;
    }

    public class ItemFixDropper : ItemDropper
    {
        [Header("Fixed Drop Settings")]
        [SerializeField] private FixedItemDropSetting[] dropSettings;

        public override PreviewItem Drop(Vector3 from, Vector3 to)
        {
            if (dropSettings == null || dropSettings.Length == 0)
                return null;

            PreviewItem firstDroppedItem = null;
            foreach (var setting in dropSettings)
            {
                if (setting == null || setting.fixedItem == null || setting.probability <= 0f ||
                    setting.probability < 1f && Random.value >= setting.probability)
                    continue;

                int minCount = Mathf.Max(1, setting.minDropCount);
                int maxCount = Mathf.Max(minCount, setting.maxDropCount);
                var createData = setting.fixedItem.CreateItem();
                createData.Stack = Random.Range(minCount, maxCount + 1);

                var droppedItem = SpawnItem(from, to, createData);
                if (firstDroppedItem == null)
                    firstDroppedItem = droppedItem;
            }

            return firstDroppedItem;
        }

        private void OnValidate()
        {
            if (dropSettings == null)
                return;

            foreach (var setting in dropSettings)
            {
                if (setting == null)
                    continue;

                setting.minDropCount = Mathf.Max(1, setting.minDropCount);
                setting.maxDropCount = Mathf.Max(setting.minDropCount, setting.maxDropCount);
                setting.probability = Mathf.Clamp01(setting.probability);
            }
        }
    }
}
