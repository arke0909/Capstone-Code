using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies.FSM;

namespace Code.SHS.Entities.Enemies.FSM.BehaviourState
{
    public class EnemyBehaviourState : EnemyState
    {
        public EnemyBehaviourState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
        }
    }
}
