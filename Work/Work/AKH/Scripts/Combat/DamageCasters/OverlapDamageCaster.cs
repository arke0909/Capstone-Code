using System;
using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;

namespace Scripts.Combat
{
    public class OverlapDamageCaster : DamageCaster, ISphereCaster
    {
        [SerializeField] private float castRadius = 1f;
        [SerializeField] private float castAngle = 360f;
        [SerializeField] private int maxCollideCount = 1;

        private Collider[] _colliders;
        private float _originRadius;
        
        public float CastRadius => castRadius;

        private void Awake()
        {
            _originRadius = castRadius;
            _colliders = new Collider[maxCollideCount];
        }

        public override bool CastDamage(DamageData damageData, Vector3 position, Vector3 direction, MovementDataSO knockBackData)
        {
            int count = Physics.OverlapSphereNonAlloc(position, castRadius, _colliders, whatIsTarget);

            if (count <= 0) return false;

            float halfAngle = castAngle * 0.5f;
            Vector3 startDir = Quaternion.Euler(0f, -halfAngle, 0f) * direction;
            Vector3 endDir = Quaternion.Euler(0f, halfAngle, 0f) * direction;

            bool hitAny = false;

            for (int i = 0; i < count; i++)
            {
                Transform target = _colliders[i].transform;

                if (!IsPointInArc(position, startDir, endDir, target.position, castRadius, castAngle))
                    continue;
                Vector3 normal = (position - target.position).normalized;
                Vector3 hitPoint = _colliders[i].ClosestPoint(position);
                ApplyDamageAndKnockback(target, damageData, hitPoint, normal, knockBackData);
                hitAny = true;
            }

            return hitAny;
        }
        public void ResetRadius()
        {
            castRadius = _originRadius;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = transform.position;
            Vector3 forward = transform.forward;

            Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
            Gizmos.DrawWireSphere(center, castRadius);

            float halfAngle = castAngle * 0.5f;
            Vector3 startDir = Quaternion.Euler(0f, -halfAngle, 0f) * forward;
            Vector3 endDir = Quaternion.Euler(0f, halfAngle, 0f) * forward;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(center, center + startDir.normalized * castRadius);
            Gizmos.DrawLine(center, center + endDir.normalized * castRadius);

            Gizmos.color = Color.white;
            Gizmos.DrawLine(center, center + forward.normalized * castRadius);

            if (castAngle < 360f - 0.0001f)
            {
                Gizmos.color = new Color(1f, 0.2f, 0.2f, 1f);

                int segments = Mathf.Clamp(Mathf.CeilToInt(castAngle / 6f), 8, 120);
                float step = castAngle / segments;

                Vector3 prev = center + startDir.normalized * castRadius;

                for (int i = 1; i <= segments; i++)
                {
                    float a = -halfAngle + step * i;
                    Vector3 dir = Quaternion.Euler(0f, a, 0f) * forward;
                    Vector3 next = center + dir.normalized * castRadius;
                    Gizmos.DrawLine(prev, next);
                    prev = next;
                }
            }
        }

        public void SetRadius(float radius)
        {
            castRadius = radius;
        }
    }
}