namespace Scripts.Enemies.EnemyBehaviourConditions
{
    public class IsTargetExistCondition : EnemyBehaviourCondition
    {
        public override bool Condition()
        {
            return _enemy.TargetProvider.Target != null;
        }
    }
}
