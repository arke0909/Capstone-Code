using Code.Items.ItemInfo;
using Code.UI.Core;
using UnityEngine;
using Work.Code.Craft;
using Work.Code.Craft.Installer;
using Work.Code.Craft.View;

namespace Work.Code.PlayerTasks
{
    public class CraftPinTask : PlayerTask
    {
        [SerializeField] private CraftTreeUI craftTreeUI;
        [SerializeField] private ItemType itemType;
        [SerializeField] private Rarity itemRarity;
        [SerializeField] private CraftMenuView craftMenuView;

        [Header("특정 트리를 집어넣을 시 다른 세팅은 무시됨")]
        [SerializeField] private CraftTreeSO targetTree;

        public override void StartTask()
        {
            base.StartTask();

            if (targetTree != null)
                craftTreeUI.RegisterTutorialCraftItem(targetTree.Item);
            else
                craftTreeUI.SetTutorialItemType(itemType, itemRarity, UIDefine.RedColor);
            
            craftMenuView.OnPinItem += HandlePinItem;
        }

        private void HandlePinItem(CraftItemUI pinItemUI, bool isActive)
        {
            CompleteTask();
        }

        protected override void StopTask()
        {
            craftTreeUI.ClearTutorialItemType();
            craftMenuView.OnPinItem -= HandlePinItem;
        }

        protected override string GetTaskText()
        {
            return $"아이템에 우클릭을 누르고 핀으로 고정하세요";
        }
    }
}
