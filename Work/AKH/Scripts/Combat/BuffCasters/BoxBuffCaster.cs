using Code.StatusEffectSystem;
using UnityEngine;

namespace Scripts.Combat
{
    public class BoxBuffCaster : BuffCaster
    {
        [SerializeField] private Vector3 castSize;
        [SerializeField] private Vector3 castOffset;
        [SerializeField] private int maxColliderCount = 5;

        private Collider[] _colliders;

        private void Awake()
        {
            _colliders = new Collider[maxColliderCount];
        }

        public override bool CastBuff(
            Vector3 position,
            BuffSO buff,
            int level = 0,
            float additionalTime = 0f)
        {
            Vector3 center = position + castOffset;
            int count = Physics.OverlapBoxNonAlloc(center, castSize, _colliders,
                Quaternion.identity, whatIsTarget);

            if (count <= 0) return false;

            for (int i = 0; i < count; i++)
            {
                ApplyBuff(_colliders[i].transform, buff, level, additionalTime);
            }
            return count > 0;
        }

        private void OnDrawGizmos()
        {
            Vector3 startPosition = transform.position + castOffset;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(startPosition, castSize);
        }
    }
}
