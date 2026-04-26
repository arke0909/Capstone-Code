using System;
using UnityEngine;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Craft
{
    [Serializable]
    public class NodeData
    {
        [field: SerializeField] public ItemDataSO Item { get; set; }
        [field: SerializeField] public CraftTreeSO Tree { get; set; }
        [field: SerializeField] public int Count { get; set; }
        [field: SerializeField] public bool IsChild { get; set; }
        
        public NodeData Clone()
        {
            return new NodeData
            {
                Item = this.Item,
                Count = this.Count,
                IsChild = this.IsChild,
                Tree = this.Tree,
            };
        }
    }
}