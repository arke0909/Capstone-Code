using Chipmunk.ComponentContainers;
using Cysharp.Threading.Tasks;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.FSM;
using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.SkillSystem.Skills
{
    public class DashSkill : ActiveSkill
    {
        [SerializeField] private MovementDataSO movementData;
        private ISkillMovement _movement;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _movement = container.GetSubclassComponent<ISkillMovement>();
        }

        public override async void StartAndUseSkill()
        {
            base.StartAndUseSkill();
            _movement.CanMove = false;
            Vector3 velocity = _movement.Velocity;
            velocity.y = 0;
            Vector3 direction = Mathf.Approximately(velocity.magnitude, 0f)
                ? _owner.transform.forward : _movement.Velocity.normalized;
            _movement.ApplyMovementData(direction, movementData);
            await UniTask.WaitForSeconds(movementData.duration);
            _movement.CanMove = true;
        }
    }
}

