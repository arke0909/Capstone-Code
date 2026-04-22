using Code.SHS.Entities.Enemies.FSM;
using UnityEngine;

namespace Scripts.Enemies.EnemyBehaviours
{
    public class ChangeStateBehaviour : EnemyBehaviour
    {
        [SerializeField] private EnemyStateEnum targetState;
        public override void Execute()
        {
            _enemy.ChangeState(targetState);
        }
    }
}
