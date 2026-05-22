using Code.Items;
using UnityEngine;

namespace Code.Items.ItemInfo
{
    [CreateAssetMenu(fileName = "MaterialDataSO", menuName = "SO/Item/MaterialData", order = 0)]
    public class MaterialDataSO : ItemDataSO
    {
        public override ItemCreateData CreateItem()
            => new ItemCreateData(new MaterialItem(this), Random.Range(1, maxSpawnCount));
    }
}