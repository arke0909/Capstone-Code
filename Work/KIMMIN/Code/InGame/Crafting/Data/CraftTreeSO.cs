using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Craft
{
    [CreateAssetMenu(fileName = "NewCraftTree", menuName = "SO/CraftTreeSO", order = 0)]
    public class CraftTreeSO : ScriptableObject
    {
        public NodeData Root => nodeList.FirstOrDefault();
        public ItemDataSO Item => nodeList[0].Item;
        public int Count => nodeList[0].Count;
        
        public string treeName; 
        public bool isBinary = true;
        
        [HideInInspector] public List<NodeData> nodeList;
        private Dictionary<ItemDataSO, int> _itemCache;

        public Dictionary<ItemDataSO, int> ConsumeItems
        {
            get
            {
                if (_itemCache == null)
                {
                    _itemCache = new Dictionary<ItemDataSO, int>();
                    int count = isBinary ? 2 : 3;
                    
                    for (int i = 1; i <= count && i < nodeList.Count; i++)
                    {
                        var node = nodeList[i];
                        
                        if (node != null && node.Item != null)
                        {
                            _itemCache[node.Item] = node.Count;
                        }
                    }
                }

                return _itemCache;
            }
        }
        private static readonly Dictionary<ItemType, float> _craftTime = new()
        {
            {ItemType.Armor,3f },
            {ItemType.Bullet,3f },
            {ItemType.Food,3f },
            {ItemType.Gun,3f },
            {ItemType.Helmet,3f },
            {ItemType.Material,3f },
            {ItemType.Medicine,3f },
            {ItemType.MeleeWeapon,3f },
            {ItemType.None,3f },
            {ItemType.Throw,3f },
        };
        public float CraftTime => _craftTime[Item.itemType];
#if UNITY_EDITOR
        private void OnValidate()
        {
            _itemCache = null;
        }
#endif
    }
}