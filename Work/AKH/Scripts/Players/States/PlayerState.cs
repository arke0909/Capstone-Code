using Chipmunk.ComponentContainers;
using Scripts.FSM;
using System;

namespace Scripts.Players.States
{
    public enum PlayerStateEnum
    {
        Idle,
        Walk,
        Aim,
        Skill,
        Sprint,
        Reload,
        Attack,
        ItemUse,
        AimSkill,
        Stun,
    }
    public abstract class PlayerState : State
    {
        protected CharacterMovement _movement;
        protected Player _player;
        protected readonly float _inputThreshold = 0.1f;

        public PlayerState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _player = container.GetCompo<Player>();
            _movement = container.GetCompo<CharacterMovement>();
        }
        public override void Enter()
        {
            base.Enter();
            _animator.OnControllerChanged.AddListener(HandleControllerChanged);
        }
        public override void Exit()
        {
            base.Exit();
            _animator.OnControllerChanged.RemoveListener(HandleControllerChanged);
        }

        private void HandleControllerChanged()
        {
            _player.ChangeState(PlayerStateEnum.Idle);
        }
    }
}

