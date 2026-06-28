using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.SkillSystem.Manage;
using Code.UI.Core;
using DewmoLib.Dependencies;
using Scripts.Players;
using Scripts.SkillSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using Work.Code.SkillInventory.GameEvents;
using Work.Code.UI.Interaction;

namespace Work.Code.SkillInventory
{
    public class SkillEquipPanel : UIPanel
    {
        [SerializeField] private PlayerInputSO playerInput;
        [SerializeField] private Transform activeSkillRoot;
        [SerializeField] private Transform passiveSkillRoot;
        [SerializeField] private SkillInventoryUI skillInventory;
        
        [Inject] private Player _player;
        private SkillSlot[] _skillUis;
        private SkillSlot[] _inventoryUis;
        private SkillSlot _selectedSkill;
        private SkillSlot _hoveredInventorySlot;
        private SkillEquipModel _model;

        public event Action<Skill[]> OnSkillChanged;

        private void Start()
        {
            skillInventory.Initialize(_player);

            var actives = activeSkillRoot.GetComponentsInChildren<SkillSlot>(true);
            var passives = passiveSkillRoot.GetComponentsInChildren<SkillSlot>(true);
            var inventories = skillInventory.GetComponentsInChildren<SkillSlot>(true);
            _skillUis = actives.Concat(passives).ToArray();
            _inventoryUis = inventories;

            SetupUI(actives, SkillType.Active);
            SetupUI(passives, SkillType.Passive);
            SetupUI(inventories, SkillType.None);
            
            _model = new SkillEquipModel();
            _model.OnSkillChanged += HandleSkillUpdated;
            playerInput.OnSkillTreePressed += HandleSkillTreePressed;
            skillInventory.OnChangeInventory += HandleChangeInventory;
        }

        private void HandleSkillTreePressed()
        {
            ToggleUI(true);
        }

        private void SetupUI(SkillSlot[] skillUI, SkillType type)
        {
            int idx = 0;
            foreach (var ui in skillUI)
            {
                if (type != SkillType.None)
                    ui.SkillType = type;
                
                ui.Index = idx++;
                BindUI(ui);
            }
        }
        
        private void BindUI(SkillSlot ui)
        {
            ui.OnDragStartEvent += HandleDragSkill;
            ui.OnDragEndEvent += HandleDragEnd;
            ui.OnDropSkill += HandleDropSkill;
            ui.OnEquipped += HandleEquip;
            ui.OnHoverEntered += HandleHoverEntered;
            ui.OnHoverExited += HandleHoverExited;
            ui.ClearUI();
        }

        private void UnbindUI(SkillSlot ui)
        {
            ui.OnDragStartEvent -= HandleDragSkill;
            ui.OnDragEndEvent -= HandleDragEnd;
            ui.OnDropSkill -= HandleDropSkill;
            ui.OnEquipped -= HandleEquip;
            ui.OnHoverEntered -= HandleHoverEntered;
            ui.OnHoverExited -= HandleHoverExited;
        }
        
        private void HandleEquip(Skill skill, int index)
        {
            _player.LocalEventBus.Raise(new EquipSkillEvent(skill, index));
        }

        private void Update()
        {
            if (!IsActive || _hoveredInventorySlot == null || _hoveredInventorySlot.CurrentSkill == null)
                return;

            if (Keyboard.current.qKey.wasPressedThisFrame)
                EquipActiveSkill(_hoveredInventorySlot, ActiveSlotType.Q);
            else if (Keyboard.current.eKey.wasPressedThisFrame)
                EquipActiveSkill(_hoveredInventorySlot, ActiveSlotType.E);
            else if (Keyboard.current.cKey.wasPressedThisFrame)
                EquipActiveSkill(_hoveredInventorySlot, ActiveSlotType.C);
            else if (Keyboard.current.fKey.wasPressedThisFrame)
                EquipToFirstSlot(_hoveredInventorySlot);
        }

        private void HandleSkillUpdated()
        {
            List<Skill> skillList = new();
            foreach (var ui in _skillUis)
            {
                var skill = _model.GetSkill(ui.Index, ui.SkillType);
                ui.SetEquip(skill);

                if (skill != null)
                    skillList.Add(skill);
            }
            
            OnSkillChanged?.Invoke(skillList.ToArray());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _model.OnSkillChanged -= HandleSkillUpdated;
            playerInput.OnSkillTreePressed -= HandleSkillTreePressed;
            skillInventory.OnChangeInventory -= HandleChangeInventory;
            
            foreach (var ui in _skillUis)
            {
                UnbindUI(ui);
            }

            foreach (var ui in _inventoryUis)
            {
                UnbindUI(ui);
            }
        }

        private void HandleDragSkill(DraggableUI draggable)
        {
            if (draggable is not SkillSlot ui) return;
            _selectedSkill = ui;
            HighlightSkillUIs(ui, true);
        }

        private void HandleDragEnd()
        {
            HighlightSkillUIs(_selectedSkill, false);
        }
        
        private void HandleDropSkill(SkillSlot target, SkillSlot send)
        {
            if(target == null || target == send) return; 
            if (target.SkillType != send.SkillType) return;
            
            _model.Equip(send.CurrentSkill, target.CurrentSkill, 
                target.Index, target.SkillType, send.IsInventorySlot);
        }

        private void HandleHoverEntered(SkillSlot slot)
        {
            if (slot.IsInventorySlot)
                _hoveredInventorySlot = slot;
        }

        private void HandleHoverExited(SkillSlot slot)
        {
            if (_hoveredInventorySlot == slot)
                _hoveredInventorySlot = null;
        }

        private void EquipActiveSkill(SkillSlot send, ActiveSlotType slotType)
        {
            if (send.CurrentSkill.SkillType != SkillType.Active)
                return;

            int index = (int)slotType;
            Skill targetSkill = _model.GetSkill(index, SkillType.Active);
            _model.Equip(send.CurrentSkill, targetSkill, index, SkillType.Active, true);
        }

        private void EquipToFirstSlot(SkillSlot send)
        {
            SkillType skillType = send.CurrentSkill.SkillType;

            if (IsEquipped(send.CurrentSkill, skillType))
                return;

            int index = GetFirstEmptySlotIndex(skillType);
            Skill targetSkill = _model.GetSkill(index, skillType);
            _model.Equip(send.CurrentSkill, targetSkill, index, skillType, true);
        }

        private bool IsEquipped(Skill skill, SkillType skillType)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_model.GetSkill(i, skillType) == skill)
                    return true;
            }

            return false;
        }

        private int GetFirstEmptySlotIndex(SkillType skillType)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_model.GetSkill(i, skillType) == null)
                    return i;
            }

            return 0;
        }

        private void HighlightSkillUIs(SkillSlot skill, bool isOn)
        {
            if (skill == null) return;
            foreach (var ui in _skillUis)
            {
                if (ui.SkillType == skill.SkillType)
                {
                    ui.HighlightUI(isOn);
                }
            }
        }
        
        private void HandleChangeInventory(Skill[] skills)
        {
            var skillSet = new HashSet<Skill>(skills);
            bool isChanged = false;
            
            foreach (var ui in _skillUis)
            {
                if (ui.CurrentSkill != null && !skillSet.Contains(ui.CurrentSkill))
                {
                    _model.RemoveSkill(ui.CurrentSkill); 
                    ui.ClearUI();
                    isChanged = true;
                }
            }
            
            if (isChanged) HandleSkillUpdated();
        }
    }
}
