using Code.Items.ItemInfo;
using Code.UI.Core.Interaction;
using Code.UI.Minimap.Core;
using DewmoLib.ObjectPool.RunTime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Work.Code.UI.Core.Interaction;

namespace Code.UI.Minimap.SectionName
{
    public class SectionShowItem : InteractableUI
    {
         [field:SerializeField] public Image Image { get; set; }
         public ItemDataSO ItemData { get; set; }

         protected override void Awake()
         {
             base.Awake();
             BindTooltip(() => ItemData);
         }
    }
}