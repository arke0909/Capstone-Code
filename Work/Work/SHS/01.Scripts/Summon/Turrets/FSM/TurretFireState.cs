using Chipmunk.ComponentContainers;
using UnityEngine;

namespace SHS.Scripts.Summon.Turrets.FSM
{
    public class TurretFireState : TurretState
    {
        public TurretFireState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _targetPlayer = _turret.TargetPlayer;

            if (_targetPlayer != null)
            {
                Vector3 targetPos = _targetPlayer.transform.position;
                _turret.LookAtTarget(targetPos);
                _turret.FireBullet();
            }
        }

        public override void Update()
        {
            base.Update();         

            if (_isTriggerCall)
            {
                if (_turret.TargetPlayer == null || !IsTargetInRange() || !IsTargetVisible())
                {
                    _turret.ChangeState(TurretStateEnum.Idle);
                }
                else if (!_turret.CanFire)
                {
                    _turret.ChangeState(TurretStateEnum.Reload);
                }
                else
                {
                    _turret.ChangeState(TurretStateEnum.Combat);
                }
            }
        }
    }
}

