using Scripts.SkillSystem.Skills;
using AYellowpaper.SerializedCollections;
using Chipmunk.ComponentContainers;
using UnityEngine;

namespace Scripts.SkillSystem.Manage
{
    public enum ActiveSlotType
    {
        E,
        C,
        Q,
        Space,
        None,
    }

    public class ActiveSkillComponent : SkillComponent<ActiveSlotType, ActiveSkillSocket>
    {
        public ActiveSkill CurrentSkill => CurrentSocket?.CurrentActiveSkill;
        public sealed override SkillType SkillType => SkillType.Active;
        [SerializeField] private SerializedDictionary<ActiveSlotType, ActiveSkill> initSkills;

        public ActiveSlotType CurrentSkillIndex
        {
            get => _currentSkillIndex;
            set
            {
                if (Sockets.TryGetValue(value, out ActiveSkillSocket socket))
                {
                    CurrentSocket = socket;
                    _currentSkillIndex = value;
                }
            }
        }
        private ActiveSlotType _currentSkillIndex;
        public override void OnInitialize(ComponentContainer componentContainer)
        {
            base.OnInitialize(componentContainer);
            foreach (var item in initSkills)
            {
                AddSkill(item.Value);
                ChangeSkill(item.Value.SkillData, item.Key);
            }
        }
        private void Update()
        {
            foreach (ActiveSkillSocket socket in Sockets.Values)
                socket.UpdateSocket();
        }

        public bool CanUseSkill()
            => CurrentSocket != null && CurrentSocket.CanUseSkill();

        public void UseSkill()
        {
            if (CurrentSocket?.CurrentActiveSkill == null)
            {
                Debug.LogWarning($"{CurrentSkill}, {CurrentSkill}");
                return;
            }

            if (CurrentSocket.CurrentActiveSkill is IUseStateSkill stateSkill && _stateEntity != null &&
                stateSkill.TargetState != null)
            {
                _stateEntity.ChangeState(stateSkill.TargetState);
            }
            else
            {
                CurrentSocket.CurrentActiveSkill.StartAndUseSkill();
            }

            CurrentSocket.SetCooldown();
        }

        public void SubscribeCooldown(Skill skill, OnCoolDown callback)
        {
            
        }
    }
}