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
        public Action onComplete;

        public void Deconstruct(out float duration, out Vector3 targetPosition)
        {
            duration = this.duration;
            targetPosition = this.targetPosition;
        }
    }
    public class PlayerTeleportState : PlayerState
    {
        private TeleportContext _context;
        
        public PlayerTeleportState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
        }
        public override void Enter()
        {
            base.Enter();
            _movement.StopImmediately();
            _context = _blackboard.GetOrDefault<TeleportContext>("TeleportContext");
            EventBus.Raise(new PlayerGageEvent("이동중", _context.duration, HandleCompleteCraft));

        }

        private void HandleCompleteCraft()
        {
            _movement.SetPositionImmediately(_context.targetPosition);
            _context.onComplete?.Invoke();
            _context.onComplete = null;
            _player.ChangeState(PlayerStateEnum.Idle);
        }
    }
}
