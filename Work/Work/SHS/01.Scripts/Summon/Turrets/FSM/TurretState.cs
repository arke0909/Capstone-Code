using Chipmunk.ComponentContainers;
using Scripts.FSM;
using Scripts.Players;
using UnityEngine;

namespace SHS.Scripts.Summon.Turrets.FSM
{
    public enum TurretStateEnum
    {
        Idle,
        Combat,
        Fire,
        Reload
    }

    public class TurretState : State
    {
        protected Turret _turret;
        protected Player _targetPlayer;

        public TurretState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _turret = container.GetCompo<Turret>();
        }

        protected bool TryDetectPlayer()
        {
            
            int cnt = Physics.OverlapSphereNonAlloc(
                _turret.transform.position,
                _turret.DetectionRange,
                _turret.DetectedColliders,
                _turret.TargetLayer);

            for (int i = 0; i < cnt; i++)
            {
                GameObject detectedObject = _turret.DetectedColliders[i].gameObject;
                Player player = detectedObject.GetComponent<Player>();
                if (player != null && !_turret.WallExistsBetweenTarget(detectedObject.transform.position))
                {
                    _turret.SetTargetPlayer(player);
                    return true;
                }
            }

            _turret.SetTargetPlayer(null);

            return false;
        }

        protected bool IsTargetInRange()
        {
            if (_turret.TargetPlayer == null)
                return false;

            float distance = Vector3.Distance(_turret.transform.position, _turret.TargetPlayer.transform.position);
            return distance <= _turret.DetectionRange;
        }

        protected bool IsTargetVisible()
        {
            if (_turret.TargetPlayer == null)
                return false;

            return !_turret.WallExistsBetweenTarget(_turret.TargetPlayer.transform.position);
        }
    }
}