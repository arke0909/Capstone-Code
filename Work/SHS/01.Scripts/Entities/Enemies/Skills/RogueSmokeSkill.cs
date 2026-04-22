using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies.FSM;
using Scripts.SkillSystem;
using UnityEngine;
using UnityEngine.AI;

namespace Code.SHS.Entities.Enemies.Skills
{
    public class RogueSmokeSkill : ActiveSkill
    {
        [SerializeField] private GameObject smokeEffectPrefab;
        [SerializeField, Min(0f)] private float smokeDuration = 2.5f;
        [SerializeField, Min(0f)] private float repositionRadius = 5f;
        [SerializeField, Range(0f, 90f)] private float randomSideAngle = 40f;

        private Enemy _enemy;
        private CharacterNavMovement _movement;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _enemy = container.Get<Enemy>(true);
            _movement = container.Get<CharacterNavMovement>(true);
        }

        public override void StartAndUseSkill()
        {
            if (_enemy == null || _enemy.TargetProvider.CurrentTarget == null)
                return;

            if (smokeEffectPrefab != null)
            {
                GameObject smoke = Instantiate(smokeEffectPrefab, _enemy.transform.position, Quaternion.identity);
                Destroy(smoke, smokeDuration);
            }

            RepositionBehindTarget();
        }

        private void RepositionBehindTarget()
        {
            Transform target = _enemy.TargetProvider.CurrentTarget.transform;
            Vector3 backward = -target.forward;
            backward.y = 0f;

            if (backward.sqrMagnitude < 0.001f)
                backward = -_enemy.transform.forward;

            Vector3 direction = Quaternion.Euler(0f, Random.Range(-randomSideAngle, randomSideAngle), 0f) * backward.normalized;
            Vector3 desired = target.position + direction * repositionRadius;

            if (NavMesh.SamplePosition(desired, out NavMeshHit hit, repositionRadius + 1.5f, NavMesh.AllAreas))
            {
                _movement.SetDestinationForce(hit.position);
                _enemy.ChangeState(EnemyStateEnum.MoveTo, true);
            }
        }
    }
}

