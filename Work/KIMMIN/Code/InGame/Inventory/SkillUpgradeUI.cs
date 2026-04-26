using System;
using Chipmunk.ComponentContainers;
using Code.UI.Core;
using DewmoLib.Dependencies;
using Scripts.Players;
using Scripts.SkillSystem;
using Scripts.SkillSystem.Manage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Work.Code.SkillInventory;
using Work.LKW.Code.Items;

namespace InGame.InventorySystem
{
    public class SkillUpgradeUI : UIBase, IUIElement<EquipableItem>
    {
        [SerializeField] private SkillSlot skillSlot;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Button upgradeButton;

        [Inject] private Player _player;
        private EquipableItem _equipableItem;
        private SkillManager _skillManager;

        private void Start()
        {
            _skillManager = _player.Get<SkillManager>();
            upgradeButton.onClick.AddListener(HandleUpgradeClick);
            DisableUI();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            upgradeButton.onClick.RemoveListener(HandleUpgradeClick);
        }

        private void HandleUpgradeClick()
        {
            if (_equipableItem == null) return;
            _equipableItem.LevelUpSkill();
            RefreshUI(_equipableItem);
        }

        public void EnableFor(EquipableItem item)//Skill이 아니라 EquipableItem으로 바꿔야함
        {
            _equipableItem = item;
            RefreshUI(item);
            EnableUI();
        }

        private void RefreshUI(EquipableItem item)
        {
            if (!_skillManager.TryGetSkill(item.Skill, out var skill)) return;
            skillSlot.EnableFor(skill);
            levelText.text = $"레벨 {item.SkillLevel}";
            
            description.text = item.Skill.upgradeList.Count > item.SkillLevel
                ? item.Skill.upgradeList[item.SkillLevel].upgradeDescription
                : string.Empty;
        }

        public void ClearUI()
        {
            _equipableItem = null;
            DisableUI(true);
        }
    }
}