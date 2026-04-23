using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies.FSM;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.FSM
{
    class EnemyStunState : EnemyState
    {
        private float stunDuration = 2f;
        private float stunTimer = 0;

        public EnemyStunState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
        }

        public override void Enter()
        {
            base.Enter();
            stunTimer = 0;
        }

        public override void Update()
        {
            base.Update();
            stunTimer += Time.deltaTime;
            if (stunTimer >= stunDuration)
            {
                _enemy.ChangeState(EnemyStateEnum.Idle);
            }
        }

        public override void Exit()
        {
            base.Exit();
        }

        public void SetStunDuration(float duration)
        {
            stunDuration = duration;
        }
    }
}