using Code.SHS.Entities.Enemies;
using Scripts.Combat.Projectiles;
using System;
using UnityEngine;

namespace Scripts.Enemies.EnemyBehaviourConditions
{
    public class IsTargetedBulletCondition : EnemyBehaviourCondition
    {
        [SerializeField] private float detectRadius = 5f;
        [SerializeField] private LayerMask bulletLayer;
        [SerializeField] private float reactionWindow = 0.25f;
        [SerializeField] private float dangerRadius = 1.5f;
        [SerializeField] private float minDot = 0.75f;

        private Collider[] bullets;
        public override void Init(Enemy enemy)
        {
            base.Init(enemy);
            bullets = new Collider[8];
        }
        public override bool Condition()
        {
            Array.Clear(bullets, 0, bullets.Length);
            Vector3 enemyPos = _enemy.transform.position;
            int cnt = Physics.OverlapSphereNonAlloc(enemyPos, detectRadius, bullets, bulletLayer, QueryTriggerInteraction.Collide);
            enemyPos.y = 0;
            for (int i = 0; i < cnt; i++)
            {
                Collider bulletCol = bullets[i];
                if (bulletCol == null)
                    continue;

                if (!bulletCol.TryGetComponent(out Bullet bullet))
                    continue;

                Vector3 bulletPos = bullet.transform.position;
                bulletPos.y = 0;
                Vector3 bulletVelocity = bullet.Velocity;

                float speed = bulletVelocity.magnitude;
                if (speed < 0.01f)
                    continue;

                Vector3 bulletDir = bulletVelocity / speed;
                Vector3 toEnemy = enemyPos - bulletPos;

                float forwardDistance = Vector3.Dot(toEnemy, bulletDir);
                if (forwardDistance < 0f)
                    continue;

                float dot = Vector3.Dot(bulletDir, toEnemy.normalized);
                if (dot < minDot)
                    continue;

                float timeToReach = forwardDistance / speed;
                if (timeToReach > reactionWindow)
                    continue;
                Vector3 futurePos = bulletPos + bulletVelocity * reactionWindow;
                Vector3 segment = futurePos - bulletPos;

                float t = Mathf.Clamp01(
                    Vector3.Dot(enemyPos - bulletPos, segment) / segment.sqrMagnitude
                );

                Vector3 closest = bulletPos + segment * t;
                float distSqr = (enemyPos - closest).sqrMagnitude;
                return distSqr <= dangerRadius * dangerRadius;
            }

            return false;
        }
#if UNITY_EDITOR
        public override void DrawGizmos(Transform trm)
        {
            base.DrawGizmos(trm);
            Gizmos.color = Color.blue;
            if (_enemy == null)
                Gizmos.DrawWireSphere(trm.position, detectRadius);
            else
                Gizmos.DrawWireSphere(_enemy.transform.position, detectRadius);
        }
#endif
    }
}
