using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scripts.Enemies.EnemyBehaviours
{
    public class MoveForwardRandomPointBehaviour : EnemyBehaviour
    {
        [SerializeField] private float halfDeg;
        [SerializeField] private float degWeight;
        [SerializeField] private float minRadius;
        [SerializeField] private float maxRadius;
        private CharacterNavMovement _movement;
        public override void Init(Enemy enemy)
        {
            base.Init(enemy);
            _movement = enemy.Get<CharacterNavMovement>();
        }
        public override void Execute()
        {
            Vector3 direction = _enemy.transform.forward;
            float theta = Random.Range(-halfDeg, halfDeg);
            Quaternion rot = Quaternion.AngleAxis(theta + degWeight, Vector3.up);
            float randomRadius = Random.Range(minRadius, maxRadius);
            _movement.SetDestinationForce(_enemy.transform.position + rot * direction * randomRadius);
            SetCooldown();
        }
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 direction = _enemy == null ? transform.forward : _enemy.transform.forward;
            direction.Normalize();
            float weight = halfDeg / 10f;
            for (float i = -halfDeg; i <= halfDeg; i += weight)
            {
                i = Mathf.Clamp(i, -halfDeg, halfDeg);
                Quaternion rot = Quaternion.AngleAxis(i + degWeight, Vector3.up);
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, rot * direction * maxRadius);
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, rot * direction * minRadius);
            }
        }
#endif
    }
}
