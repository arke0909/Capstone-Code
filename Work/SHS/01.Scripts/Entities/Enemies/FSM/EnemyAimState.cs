using Chipmunk.ComponentContainers;
using Cysharp.Threading.Tasks;
using Scripts.Combat;
using Scripts.Enemies.States;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.FSM
{
    public class EnemyAimState : EnemyExecuteBehaviourState
    {
        //private float _minAimTime = 0f;
        //private float _maxAimTime = 0f;

        public override float ExecuteTimer => 0f;

        //private float _optimalRangeRatio = 0.7f;

        public EnemyAimState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _attackCompo = container.GetCompo<AttackCompo>();
        }

        public override void Enter()
        {
            base.Enter();
            _attackCompo.IsAim = true;
            _movement.MoveType = NavMoveType.Aim;
        }

        public override void Update()
        {
            if(Target == null)
            {
                _enemy.ChangeState(EnemyStateEnum.Chase);
                return;
            }
            float distance = Vector3.Distance(_enemy.transform.position, Target.transform.position);
            if (distance > _attackRange)
            {
                _enemy.ChangeState(EnemyStateEnum.Chase);
                return;
            }

            _movement.SetLookAtTarget(Target.transform);
            UpdateMovementAnimation();
            base.Update();
        }

        public override void Exit()
        {
            base.Exit();
            _attackCompo.IsAim = false;
        }
    }
}