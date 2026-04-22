using Scripts.Enemies.EnemyBehaviours;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.Behaviors
{
    public abstract class SkillUseCondition : MonoBehaviour
    {
        public abstract bool IsSatisfied(Enemy enemy);
    }
}
