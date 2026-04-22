// using Chipmunk.ComponentContainers;
// using Scripts.Combat;
// using Scripts.Combat.Datas;
// using UnityEngine;
//
// namespace Code.SHS.Entities.Enemies.Combat
// {
//     public class EnemyAttackCaster : MonoBehaviour, IExcludeContainerComponent
//     {
//         [SerializeField] private LayerMask targetLayer;
//         [SerializeField] private float damageAmount;
//
//         public ComponentContainer ComponentContainer { get; set; }
//
//         public Enemy Owner { get; private set; }
//
//         public void OnInitialize(ComponentContainer componentContainer)
//         {
//             Owner = componentContainer.Get<Enemy>(true);
//         }
//
//         private void OnDrawGizmosSelected()
//         {
//             Gizmos.color = Color.red;
//             Gizmos.DrawWireSphere(transform.position, 1.5f);
//         }
//     }
// }