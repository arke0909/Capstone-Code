// using Scripts.Enemies.EnemyBehaviours;
// using UnityEngine;
//
// namespace Code.SHS.Enemies.Behaviors
// {
//     public class TargetDistanceSkillCondition : SkillUseCondition
//     {
//         [SerializeField] private float minDistance = 0f;
//         [SerializeField] private float maxDistance = 999f;
//
//         public void Configure(float min, float max)
//         {
//             minDistance = Mathf.Max(0f, min);
//             maxDistance = Mathf.Max(minDistance, max);
//         }
//
//         public override bool IsSatisfied(Enemy enemy)
//         {
//             if (enemy == null || enemy.TargetPlayer == null)
//                 return false;
//
//             float distance = enemy.GetTargetDistance();
//             return distance >= minDistance && distance <= maxDistance;
//         }
//     }
// }
