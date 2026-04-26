using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.SkillSystem.Manage;
using Code.UI.Core;
using DewmoLib.Dependencies;
using Scripts.Players;
using Scripts.SkillSystem;
using UnityEngine;
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
        private SkillSlot _selectedSkill;
        private SkillEquipModel _model;

        public event Action<Skill[]> OnSkillChanged;

        private void Start()
        {
            skillInventory.Initialize(_player);

            var actives = activeSkillRoot.GetComponentsInChildren<SkillSlot>(true);
            var passives = passiveSkillRoot.GetComponentsInChildren<SkillSlot>(true);
            var inventories = skillInventory.GetComponentsInChildren<SkillSlot>(true);
            _skillUis = actives.Concat(passives).ToArray();

            SetupUI(actives, SkillType.Active);
            SetupUI(passives, SkillType.Passive);
            SetupUI(inventories, SkillType.None);
            
            _model = new SkillEquipModel();
            _model.OnSkillChanged += HandleSkillUpdated;
            _model.OnSkillUnequipped += UnEquipSkill;
            playerInput.OnSkillTreePressed += HandleSkillTreePressed;
            skillInventory.OnChangeInventory += HandleChangeInventory;
        }

        private void HandleSkillTreePressed()
        {
            ToggleUI(true);
        }

        private void UnEquipSkill(Skill skill)
        {
            skillInventory.RemoveSkill(skill);
            _model.RemoveSkill(skill);

            HandleSkillUpdated();
            _player.LocalEventBus.Raise(new UnEquipSkillEvnt(skill));
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
            ui.ClearUI();
        }

        private void UnbindUI(SkillSlot ui)
        {
            ui.OnDragStartEvent -= HandleDragSkill;
            ui.OnDragEndEvent -= HandleDragEnd;
            ui.OnDropSkill -= HandleDropSkill;
            ui.OnEquipped -= HandleEquip;
        }
        
        private void HandleEquip(Skill skill, int index)
        {
            _player.LocalEventBus.Raise(new EquipSkillEvent(skill, index));
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
            _model.OnSkillUnequipped -= UnEquipSkill;
            playerInput.OnSkillTreePressed -= HandleSkillTreePressed;
            skillInventory.OnChangeInventory -= HandleChangeInventory;
            
            foreach (var ui in _skillUis)
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