using Chipmunk.ComponentContainers;
using UnityEngine;

namespace SHS.Scripts.Summon.Turrets.FSM
{
    public class TurretCombatState : TurretState
    {

        public TurretCombatState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _targetPlayer = _turret.TargetPlayer;
            Update();
        }

        public override void Update()
        {
            base.Update();

            if (_targetPlayer == null || !IsTargetInRange() || !IsTargetVisible())
            {
                _turret.ChangeState(TurretStateEnum.Idle);
                return;
            }

            Vector3 targetPos = _targetPlayer.transform.position;
            _turret.LookAtTarget(targetPos);

            if (!_turret.CanFire)
            {
                _turret.ChangeState(TurretStateEnum.Reload);
                return;
            }
            _turret.ChangeState(TurretStateEnum.Fire);
        }
    }
}