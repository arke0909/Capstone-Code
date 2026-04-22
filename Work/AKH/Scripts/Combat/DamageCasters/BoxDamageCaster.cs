using System;
using Scripts.Combat.Datas;
using UnityEngine;

namespace Scripts.Combat
{
    public class BoxDamageCaster : DamageCaster
    {
        [SerializeField] private Vector3 castSize;
        [SerializeField] private Vector3 castOffset;
        
        public override bool CastDamage(DamageData damageData, Vector3 position, Vector3 direction, MovementDataSO knockBackData)
        {
            Vector3 startPosition = position + castOffset;
            bool isHit = Physics.BoxCast(startPosition, castSize, transform.forward, out RaycastHit hit);
            if (isHit)
            {
                ApplyDamageAndKnockback(hit.collider.transform, damageData, hit.point, hit.normal);
            }
            
            return isHit;
        }

        private void OnDrawGizmos()
        {
            Vector3 startPosition = transform.position + castOffset;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(startPosition, castSize);
        }
    }
}