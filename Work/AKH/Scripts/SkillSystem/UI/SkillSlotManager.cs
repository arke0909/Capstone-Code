using Scripts.SkillSystem.Manage;
using AYellowpaper.SerializedCollections;
using Chipmunk.ComponentContainers;
using DewmoLib.Dependencies;
using Scripts.Players;
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
        private int _passiveCnt = 0;
        
        
        private void Start()
        {
            equipPanel.OnSkillChanged += HandleSkillChanged;
            _activeCompo = _player.Get<ActiveSkillComponent>();
            _passiveCompo = _player.Get<PassiveSkillComponent>();
            
            ClearSkillSlots();
        }

        private void HandleSkillChanged(Skill[] skills)
        {
            ClearSkillSlots();

            foreach (var skill in skills)
            {
                if(skill == null) continue;
                
                if(skill.SkillType == SkillType.Active)
                    ActiveSkillChanged(skill as ActiveSkill);
                else if(skill.SkillType == SkillType.Passive)
                    PassiveSkillChanged(skill as PassiveSkill);
            }
        }

        private void ActiveSkillChanged(ActiveSkill skill)
        {
            if(_activeCompo.TryGetSlotTypeBySkill(skill, out ActiveSlotType slotType))
            {
                var socket = _activeCompo.GetSocket(skill) as ActiveSkillSocket;
                skillSlots[slotType].InitSlot(socket, slotType);
            }
        }
        
        private void PassiveSkillChanged(PassiveSkill skill)
        {
            var socket = _passiveCompo.GetSocket(skill) as PassiveSkillSocket;
            passiveSkillUIs[_passiveCnt++].Init(socket);
        }

        private void ClearSkillSlots()
        {
            _passiveCnt = 0;
            
            foreach (var activeSlot in skillSlots.Values)
            {
                activeSlot.DisableUI();
            }

            foreach (var passiveSlot in passiveSkillUIs)
            {
                passiveSlot.DisableUI();
            }
        }
        
        private void OnDestroy()
        {
            equipPanel.OnSkillChanged -= HandleSkillChanged;
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            foreach (var slot in skillSlots)
            {
                if(slot.Value != null) 
                    slot.Value.name = $"SkillUI_{slot.Key.ToString()}";
            }
        }
        #endif
    }
}
