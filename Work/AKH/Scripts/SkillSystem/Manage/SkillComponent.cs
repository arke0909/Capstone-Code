using Chipmunk.ComponentContainers;
using Code.SkillSystem;
using Scripts.Entities;
using Scripts.Players;
using Scripts.SkillSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.SkillSystem.Manage
{
    public abstract class SkillComponent<TSlotType, TSocketType>
        : MonoBehaviour, IContainerComponent, ISkillCompo
        where TSlotType : Enum
        where TSocketType : SkillSocket, new()

    {
        public event Action OnSkillsChanged;
        public ComponentContainer ComponentContainer { get; set; }
        public Dictionary<TSlotType, TSocketType> Sockets { get; private set; } = new();
        public TSocketType CurrentSocket { get; protected set; }
        public Dictionary<SkillDataSO, Skill> Skills => _skills;

        public abstract SkillType SkillType { get; }

        protected readonly Dictionary<SkillDataSO, Skill> _skills = new();
        protected IStateEntity _stateEntity;
        protected Entity _ownerEntity;

        private Dictionary<Skill, SkillSocket> _socketBySkillDic = new();

        public virtual void OnInitialize(ComponentContainer componentContainer)
        {
            foreach (TSlotType slotType in Enum.GetValues(typeof(TSlotType)))
                EnsureSocket(slotType);
            _ownerEntity ??= ComponentContainer.GetSubclassComponent<Entity>();
            _stateEntity = _ownerEntity as IStateEntity;
        }

        public Skill GetSkill(SkillDataSO skillType)
        {
            return _skills.GetValueOrDefault(skillType);
        }

        private void EnsureSocket(TSlotType slot)
        {
            if (!Sockets.ContainsKey(slot))
                Sockets.Add(slot, new TSocketType());
        }

        public virtual void AddSkill(Skill skill)
        {
            SkillDataSO skillType = skill.SkillData;
            if (_skills.ContainsKey(skillType))
                return;
            _skills.Add(skillType, skill);
            
            OnSkillsChanged?.Invoke();
        }

        public virtual void RemoveSkill(Skill skill)
        {
            SkillDataSO skillType = skill.SkillData;
            if(_socketBySkillDic.TryGetValue(skill,out SkillSocket socket))
            {
                socket.ChangeItem(null);
                _socketBySkillDic.Remove(skill);
            }
            _skills.Remove(skillType);
            OnSkillsChanged?.Invoke();
        }

        public virtual void ClearSkills()
        {
            foreach (SkillSocket socket in Sockets.Values)
            {
                socket.ChangeItem(null);
            }

            _socketBySkillDic.Clear();
            _skills.Clear();
            OnSkillsChanged?.Invoke();
        }

        public void ChangeSkill(SkillDataSO skillData, TSlotType targetSlot)
        {
            if (!Sockets.TryGetValue(targetSlot, out TSocketType socket))
                return;
            if (socket.CurrentSkill != null)
                _socketBySkillDic.Remove(socket.CurrentSkill);
            if (_skills.TryGetValue(skillData, out Skill skill))
            {
                if (_socketBySkillDic.TryGetValue(skill, out SkillSocket beforeSocket))
                    beforeSocket.ChangeItem(null);
                socket.ChangeItem(skill);
                _socketBySkillDic[skill] = socket;
            }
            else
            {
                socket.ChangeItem(null);
            }
        }

        public virtual void ChangeSkill(SkillDataSO skillData, int slotType)
        {
            if (!Enum.IsDefined(typeof(TSlotType), slotType))
                return;
            ChangeSkill(skillData, (TSlotType)Enum.ToObject(typeof(TSlotType), slotType));
        }
        
        public SkillSocket GetSocket(Skill skill)
        {
            return _socketBySkillDic.GetValueOrDefault(skill);
        }
        public bool TryGetSlotTypeBySkill(Skill skill,out TSlotType slotType)
        {
            KeyValuePair<TSlotType, TSocketType> kvp = Sockets.FirstOrDefault(kvp => kvp.Value.CurrentSkill == skill);
            slotType = kvp.Key;
            return kvp.Value != null;
        }
    }
}
