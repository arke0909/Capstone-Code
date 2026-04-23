using Scripts.Combat.Datas;
using UnityEngine;

namespace Scripts.Combat
{
    public class SphereDamageCaster : DamageCaster, ISphereCaster
    {
        [SerializeField, Range(0.5f, 3f)] private float castRadius = 1f;
        [SerializeField, Range(0f, 1f)] private float castinterpolation = 1f;
        [SerializeField, Range(0, 3f)] private float castRange = 1f;
        public float CastRadius => castRadius;
        public override bool CastDamage(DamageData damageData, Vector3 position, Vector3 direction, MovementDataSO knockBackData)
        {
            Vector3 startPosition = position + direction * -castinterpolation * 2f;
            bool isHit = Physics.SphereCast(startPosition, castRadius, transform.forward, out RaycastHit hit, castRange, whatIsTarget);
            if (isHit)
                ApplyDamageAndKnockback(hit.collider.transform, damageData, hit.point, hit.normal, knockBackData);
            return isHit;
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Vector3 startPosition = transform.position + transform.forward * -castinterpolation * 2f;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPosition, castRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(startPosition + transform.forward * castRange, castRadius);
        }
#endif
        public void SetRadius(float radius)
        {
            castRadius = radius;   
        }
    }
}
