using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Crafting
{
    [CreateAssetMenu(fileName = "NewCraftTree", menuName = "SO/CraftTreeSO", order = 0)]
    public class CraftTreeSO : ScriptableObject
    {
        public NodeData Root => nodeList.FirstOrDefault();
        public ItemDataSO Item => nodeList[0].Item;
        public int Count => nodeList[0].Count;
        
        public string treeName; 
        public List<NodeData> nodeList;
        public bool isBinary = true;
        
        private Dictionary<ItemDataSO, int> _childCache;

        public Dictionary<ItemDataSO, int> CosumeItems
        {
            get
            {
                if (_childCache == null)
                {
                    _childCache = new Dictionary<ItemDataSO, int>();

                    int count = isBinary ? 2 : 3;
                    for (int i = 1; i <= count && i < nodeList.Count; i++)
                    {
                        var node = nodeList[i];
                        if (node != null && node.Item != null)
                        {
                            _childCache[node.Item] = node.Count;
                        }
                    }
                }

                return _childCache;
            }
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            _childCache = null;
        }
#endif
    }
}