using Code.UI.Minimap.Core;
using DewmoLib.ObjectPool.RunTime;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Minimap.SectionName
{
    public class SectionShowItem : MonoBehaviour, IPoolable
    {
         [field:SerializeField] public Image Image { get; set; }
         [field: SerializeField] public PoolItemSO PoolItem { get; private set; }
         public GameObject GameObject => gameObject;
         
         public void SetUpPool(Pool pool)
         {
             
         }

         public void ResetItem()
         {
         }
    }
}