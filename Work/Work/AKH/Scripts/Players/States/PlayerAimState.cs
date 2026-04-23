using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Scripts.Combat.Datas;
using UnityEngine;

namespace Scripts.Players.States
{
    public struct NoAmmoSoundEvent : IEvent 
    { }
    public class PlayerAimState : PlayerLocomotionCombatState
    {
        private bool _isSound;
        public PlayerAimState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _myMoveType = MoveType.Aim;
        }
        public override void Update()
        {
            base.Update();
            if (_player.PlayerInput.AttackKey
                && _weapon != null
                && _weapon.IsEquipped
                && _weapon is IAttackable attackable)
            {
                if (attackable.CurrentAttackableState == AttackableState.CanAttack)
                    _player.ChangeState(PlayerStateEnum.Attack);
                else if (attackable.CurrentAttackableState == AttackableState.NeedAmmo && !_isSound)
                {
                    EventBus.Raise(new NoAmmoSoundEvent());
                    _isSound = true;
                }
            }
            if (!_player.PlayerInput.AttackKey)
                _isSound = false;
            if (!_player.PlayerInput.AimKey)
                _player.ChangeState(PlayerStateEnum.Idle);
        }
    }
}
