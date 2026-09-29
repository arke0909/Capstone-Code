using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Chipmunk.ComponentContainers;
using Code.UI.Core;
using DewmoLib.Dependencies;
using Scripts.Players;
using Scripts.SkillSystem.Manage;
using UnityEngine;
using Work.Code.SkillInventory;

namespace Scripts.SkillSystem.UI
{
    public class SkillSlotManager : MonoBehaviour
    {
        [SerializeField] private SkillEquipPanel equipPanel;
        [SerializeField] private SerializedDictionary<ActiveSlotType, ActiveSkilUI> skillSlots;
        [SerializeField] private PassiveSkillUI[] passiveSkillUIs;

        [Inject] private Player _player;
        private ActiveSkillComponent _activeCompo;
        private PassiveSkillComponent _passiveCompo;
        private int _passiveCnt;

        private readonly HashSet<Skill> _knownActiveSkills = new();
        private readonly HashSet<Skill> _knownPassiveSkills = new();
        private readonly HashSet<Skill> _unreviewedActiveSkills = new();
        private readonly HashSet<Skill> _unreviewedPassiveSkills = new();

        private static readonly ActiveSlotType[] ActiveAttentionSlotOrder =
        {
            ActiveSlotType.Q,
            ActiveSlotType.E,
            ActiveSlotType.C,
        };

        private void Start()
        {
            equipPanel.OnSkillChanged += HandleSkillChanged;
            equipPanel.OnToggleUI += HandleEquipPanelToggled;
            _activeCompo = _player.Get<ActiveSkillComponent>();
            _passiveCompo = _player.Get<PassiveSkillComponent>();

            _activeCompo.OnSkillsChanged += HandleActiveSkillsChanged;
            _passiveCompo.OnSkillsChanged += HandlePassiveSkillsChanged;
            CaptureKnownSkills(_activeCompo.Skills.Values, _knownActiveSkills);
            CaptureKnownSkills(_passiveCompo.Skills.Values, _knownPassiveSkills);

            ClearSkillSlots();
        }

        private void HandleSkillChanged(Skill[] skills)
        {
            ClearSkillSlots();

            foreach (Skill skill in skills)
            {
                if (skill == null)
                    continue;

                if (skill.SkillType == SkillType.Active)
                    ActiveSkillChanged(skill as ActiveSkill);
                else if (skill.SkillType == SkillType.Passive)
                    PassiveSkillChanged(skill as PassiveSkill);
            }

            RemoveEquippedSkillsFromAttention();
            RefreshAttentionSlots();
        }

        private void ActiveSkillChanged(ActiveSkill skill)
        {
            if (_activeCompo.TryGetSlotTypeBySkill(skill, out ActiveSlotType slotType))
            {
                ActiveSkillSocket socket = _activeCompo.GetSocket(skill) as ActiveSkillSocket;
                skillSlots[slotType].InitSlot(socket, slotType);
            }
        }

        private void PassiveSkillChanged(PassiveSkill skill)
        {
            if (_passiveCnt >= passiveSkillUIs.Length)
                return;

            PassiveSkillSocket socket = _passiveCompo.GetSocket(skill) as PassiveSkillSocket;
            passiveSkillUIs[_passiveCnt++].Init(socket);
        }

        private void HandleActiveSkillsChanged()
        {
            TrackNewSkills(
                _activeCompo.Skills.Values,
                _knownActiveSkills,
                _unreviewedActiveSkills,
                _activeCompo);
        }

        private void HandlePassiveSkillsChanged()
        {
            TrackNewSkills(
                _passiveCompo.Skills.Values,
                _knownPassiveSkills,
                _unreviewedPassiveSkills,
                _passiveCompo);
        }

        private void TrackNewSkills<TSlotType, TSocketType>(
            IEnumerable<Skill> skills,
            HashSet<Skill> knownSkills,
            HashSet<Skill> unreviewedSkills,
            SkillComponent<TSlotType, TSocketType> skillComponent)
            where TSlotType : Enum
            where TSocketType : SkillSocket, new()
        {
            HashSet<Skill> currentSkills = CollectEquippableSkills(skills);

            foreach (Skill skill in currentSkills)
            {
                if (!knownSkills.Contains(skill) && skillComponent.GetSocket(skill) == null)
                    unreviewedSkills.Add(skill);
            }

            unreviewedSkills.IntersectWith(currentSkills);
            knownSkills.Clear();
            knownSkills.UnionWith(currentSkills);

            RemoveEquippedSkillsFromAttention();
            RefreshAttentionSlots();
        }

        private static HashSet<Skill> CollectEquippableSkills(IEnumerable<Skill> skills)
        {
            HashSet<Skill> result = new();

            foreach (Skill skill in skills)
            {
                if (skill != null && skill.SkillData != null && !skill.SkillData.defaultSkill)
                    result.Add(skill);
            }

            return result;
        }

        private static void CaptureKnownSkills(IEnumerable<Skill> skills, HashSet<Skill> knownSkills)
        {
            knownSkills.Clear();
            knownSkills.UnionWith(CollectEquippableSkills(skills));
        }

        private void RemoveEquippedSkillsFromAttention()
        {
            _unreviewedActiveSkills.RemoveWhere(skill => _activeCompo.GetSocket(skill) != null);
            _unreviewedPassiveSkills.RemoveWhere(skill => _passiveCompo.GetSocket(skill) != null);
        }

        private void HandleEquipPanelToggled(UIBase ui, bool isFade)
        {
            if (!ui.IsActive)
                return;

            _unreviewedActiveSkills.Clear();
            _unreviewedPassiveSkills.Clear();
            StopAttentionSlots();
        }

        private void RefreshAttentionSlots()
        {
            StopAttentionSlots();

            if (equipPanel.IsActive)
                return;

            if (_unreviewedActiveSkills.Count > 0 && TryGetFirstEmptyActiveSlot(out ActiveSkilUI activeSlot))
                activeSlot.ShowEmptyAttention();

            if (_unreviewedPassiveSkills.Count > 0 && _passiveCnt < passiveSkillUIs.Length)
                passiveSkillUIs[_passiveCnt].ShowEmptyAttention();
        }

        private bool TryGetFirstEmptyActiveSlot(out ActiveSkilUI emptySlot)
        {
            foreach (ActiveSlotType slotType in ActiveAttentionSlotOrder)
            {
                if (!_activeCompo.Sockets.TryGetValue(slotType, out ActiveSkillSocket socket) ||
                    socket.CurrentSkill != null)
                    continue;

                if (skillSlots.TryGetValue(slotType, out emptySlot) && emptySlot != null)
                    return true;
            }

            emptySlot = null;
            return false;
        }

        private void StopAttentionSlots()
        {
            foreach (var activeSlot in skillSlots)
            {
                if (activeSlot.Value != null)
                    activeSlot.Value.StopAttention();
            }

            foreach (PassiveSkillUI passiveSlot in passiveSkillUIs)
            {
                if (passiveSlot != null)
                    passiveSlot.StopAttention();
            }
        }

        private void ClearSkillSlots()
        {
            _passiveCnt = 0;

            foreach (var activeSlot in skillSlots)
            {
                activeSlot.Value.InitSlot(null, activeSlot.Key);
            }

            foreach (PassiveSkillUI passiveSlot in passiveSkillUIs)
            {
                passiveSlot.DisableUI();
            }
        }

        private void OnDestroy()
        {
            if (equipPanel != null)
            {
                equipPanel.OnSkillChanged -= HandleSkillChanged;
                equipPanel.OnToggleUI -= HandleEquipPanelToggled;
            }

            if (_activeCompo != null)
                _activeCompo.OnSkillsChanged -= HandleActiveSkillsChanged;

            if (_passiveCompo != null)
                _passiveCompo.OnSkillsChanged -= HandlePassiveSkillsChanged;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            foreach (var slot in skillSlots)
            {
                if (slot.Value != null)
                    slot.Value.name = $"SkillUI_{slot.Key}";
            }
        }
#endif
    }
}
