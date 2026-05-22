using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Scripts.GameSystem.Structures;
using System;
using UnityEngine;
using Work.Code.Craft;
using Work.Code.GameEvents;

namespace Scripts.Players.States
{
    public class TeleportContext
    {
        public float duration;
        public Vector3 targetPosition;

        public void Deconstruct(out float duration, out Vector3 targetPosition)
        {
            duration = this.duration;
            targetPosition = this.targetPosition;
        }
    }
    public class PlayerTeleportState : PlayerState
    {
        private float _duration;
        private Vector3 _targetPosition;
        public PlayerTeleportState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
        }
        public override void Enter()
        {
            base.Enter();
            (_duration, _targetPosition) = _blackboard.GetOrDefault<TeleportContext>("TeleportContext");
            EventBus.Raise(new PlayerGageEvent("텔포중", _duration, HandleCompleteCraft));

        }

        private void HandleCompleteCraft()
        {
            _movement.SetPosition(_targetPosition);
            _player.ChangeState(PlayerStateEnum.Idle);
        }
    }
}
