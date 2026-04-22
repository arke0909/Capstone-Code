using Chipmunk.ComponentContainers;
using Scripts.Entities.Vitals;
using UnityEngine;

namespace Scripts.Players.States
{
    public class PlayerSprintState : PlayerCombatState
    {
        float noiseTimer = 0f;
        float noiseInterval = 0.5f;
        float decStatPerSec = 10f;

        private StaminaCompo _staminaCompo;
        public PlayerSprintState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _myMoveType = MoveType.Sprint;
            _staminaCompo = container.Get<StaminaCompo>();
        }
        public override void Update()
        {
            Vector3 velocity = _movement.Velocity;
            velocity.y = 0;
            _movement.SetRotationInfo(velocity);
            _staminaCompo.ChangeValueWithTimer(-(decStatPerSec * Time.deltaTime), Time.deltaTime);
            if (!_player.PlayerInput.SprintKey || _player.PlayerInput.MovementKey == Vector2.zero)
            {
                _player.ChangeState(PlayerStateEnum.Idle);
            }

            // 달릴때 소음~ 쿵쿵 쾅쾅
            noiseTimer += Time.deltaTime;
            if (noiseTimer >= noiseInterval)
                _player.NoiseGenerator.GenerateNoise(_player, 8f);
            base.Update();
            if(_staminaCompo.CurrentValue <= 0f)
            {
                _player.ChangeState(PlayerStateEnum.Idle);
            }
        }
    }
}