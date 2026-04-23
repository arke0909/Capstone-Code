using Chipmunk.ComponentContainers;
using UnityEngine;

namespace SHS.Scripts.Summon.Turrets.FSM
{
    public class TurretReloadState : TurretState
    {
        public TurretReloadState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
        }

        public override void Update()
        {
            base.Update();
             if (_isTriggerCall)
            {
                _turret.ReloadComplete();
                
                if (_turret.TargetPlayer != null && IsTargetInRange() && IsTargetVisible())
                {
                    _turret.ChangeState(TurretStateEnum.Combat);
                }
                else
                {
                    _turret.ChangeState(TurretStateEnum.Idle);
                }
            }
        }
    }
}
