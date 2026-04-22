using UnityEngine;

namespace Scripts.Combat
{
    public abstract class Caster : MonoBehaviour
    {
        [SerializeField] protected LayerMask whatIsTarget;

        protected bool IsPointInArc(Vector3 center, Vector3 startDir, Vector3 endDir, Vector3 point, float radius, float castAngle)
        {
            Vector3 toP = point - center;
            toP.y = 0;

            if (castAngle >= 360f - 0.0001f) return true;

            Vector3 u1 = startDir.normalized;
            Vector3 u2 = endDir.normalized;
            
            float sqrDist = toP.sqrMagnitude;
            if (sqrDist < 0.0001f) return true;
            
            Vector3 up = toP / Mathf.Sqrt(sqrDist);

            float cosArc = Vector3.Dot(u1, u2);
            float cosP   = Vector3.Dot(u1, up);

            return cosP >= cosArc;
        }
    }
}