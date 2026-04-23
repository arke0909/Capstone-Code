using Chipmunk.ComponentContainers;
using UnityEngine;

namespace Scripts.Players.States
{
    public class PlayerStunState : PlayerState
    {
        private float _stunDuration = 1f;
        private float _stunTimer;

        public PlayerStunState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _stunTimer = 0f;
            _movement.StopImmediately();
            _movement.MoveType = MoveType.Idle;
        }

        public override void Update()
        {
            base.Update();
            _movement.StopImmediately();
            _stunTimer += Time.deltaTime;
            if (_stunTimer >= _stunDuration)
            {
                _player.ChangeState(PlayerStateEnum.Idle);
            }
        }

        public void SetStunDuration(float duration)
        {
            _stunDuration = Mathf.Max(0f, duration);
        }
    }
}
