using Chipmunk.ComponentContainers;
using UnityEngine;

namespace SHS.Scripts.Summon.Turrets.FSM
{
    public class TurretIdleState : TurretState
    {
        public TurretIdleState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _turret.SetTargetPlayer(null);
        }

        public override void Update()
        {
            base.Update();
            if (_turret.TargetPlayer != null || TryDetectPlayer())
            {
                _turret.ChangeState(TurretStateEnum.Combat);
            }
        }
    }
}