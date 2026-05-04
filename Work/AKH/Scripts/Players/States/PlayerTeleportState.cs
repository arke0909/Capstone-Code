using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Scripts.GameSystem.Teleports;
using System;
using UnityEngine;
using Work.Code.Craft;
using Work.Code.GameEvents;

namespace Scripts.Players.States
{
    public class TeleportContext
    {
        public float duration;
        public TeleportStructure targetStructure;
        public TeleportContext(float duration, TeleportStructure targetStructure)
        {
            this.duration = duration;
            this.targetStructure = targetStructure;
        }
        public void Deconstruct(out float duration, out TeleportStructure targetStructure)
        {
            duration = this.duration;
            targetStructure = this.targetStructure;
        }
    }
    public class PlayerTeleportState : PlayerState
    {
        private float _duration;
        private TeleportStructure _targetStructure;
        public PlayerTeleportState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
        }
        public override void Enter()
        {
            base.Enter();
            (_duration, _targetStructure) = _blackboard.GetOrDefault<TeleportContext>("TeleportContext");
            Debug.Assert(_targetStructure != null, $"{_targetStructure}");
            EventBus.Raise(new PlayerGageEvent("텔포중", _duration, HandleCompleteCraft));

        }

        private void HandleCompleteCraft()
        {
            _movement.SetPosition(_targetStructure.transform.position);
            _player.ChangeState(PlayerStateEnum.Idle);
        }
    }
}
