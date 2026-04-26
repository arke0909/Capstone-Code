using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Work.Code.Craft
{
    [CreateAssetMenu(fileName = "TreeList", menuName = "SO/CraftTree/TreeList", order = 0)]
    public class CraftTreeListSO : ScriptableObject
    {
        public List<CraftTreeSO> list = new();
    }
}