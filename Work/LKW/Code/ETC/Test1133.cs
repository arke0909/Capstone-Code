using System.Collections.Generic;
using Chipmunk.GameEvents;
using UnityEngine;
using Work.Code.GameEvents;
using Work.LKW.Code.Items.ItemInfo;

namespace Code.ETC
{
    public class Test1133 : MonoBehaviour
    {
        [SerializeField] private List<ItemDataSO> itmLis;
        
        [ContextMenu("ShowItemsOnmMap")]
        private void ShowItemsOnMap()
        {
            var evt  = new ShowItemsOnMap(itmLis);
            Bus.Raise(evt);
        }
    }
}