using System.Collections.Generic;
using Code.UI.Core;
using UnityEngine;
using Work.Code.Crafting;
using Work.LKW.Code.Items.ItemInfo;

namespace Code.UI.Tooltip
{
    public class CraftableItemTooltip : BaseTooltip<List<ItemDataSO>>
    {
        [SerializeField] private CraftableItemUI craftableItemUI;
        [SerializeField] private Transform root;

        private List<CraftableItemUI> _uiCache = new();
        
        protected override void ShowTooltip(List<ItemDataSO> targetItem)
        {
            int idx = 0;
            foreach (ItemDataSO item in targetItem)
            {
                if (idx >= _uiCache.Count)
                {
                    var ui = Instantiate(craftableItemUI, root);
                    ui.EnableFor(item);
                    _uiCache.Add(ui);
                }
                else
                    _uiCache[idx].EnableFor(item);
                
                idx++;
            }
        }

        public override void DisableUI(bool hasTween = false)
        {
            base.DisableUI(hasTween);
            
            foreach (var ui in _uiCache)
            {
                ui.DisableUI();
            }
        }
    }
}