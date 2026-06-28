using System;
using Chipmunk.ComponentContainers;
using Code.UI.Core;
using DewmoLib.Dependencies;
using Scripts.Players;
using Scripts.SkillSystem;
using Scripts.SkillSystem.Manage;
using TMPro;
using UnityEngine;
using Work.Code.SkillInventory;
using Code.Items;

namespace InGame.InventorySystem
{
    public class SkillUpgradeUI : UIBase, IUIElement<EquipableItem>
    {
        [SerializeField] private SkillSlot skillSlot;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private TextMeshProUGUI levelText;

        [Inject] private Player _player;
        private EquipableItem _equipableItem;
        private SkillManager _skillManager;

        private void Start()
        {
            _skillManager = _player.Get<SkillManager>();
            DisableUI();
        }

        public void EnableFor(EquipableItem item)
        {
            _equipableItem = item;
            RefreshUI(item);
            EnableUI();
        }

        private void RefreshUI(EquipableItem item)
        {
            if (!_skillManager.TryGetSkill(item.Skill, out var skill)) return;
            skillSlot.EnableFor(skill);

            // 스킬 레벨은 장비 등급(Rarity)에서 자동 결정됨: Common=1, Rare=2, Epic=3
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
