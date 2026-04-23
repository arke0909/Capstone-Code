using System;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Effects;
using UnityEngine;

namespace SHS.Scripts.Effects
{
    public class BulletImpactEffect : MonoBehaviour
    {
        [SerializeField] private PoolItemSO bulletImpactEffectPoolItem;
        [SerializeField] private PoolManagerSO poolManager;

        private void OnEnable()
        {
            Debug.Assert(bulletImpactEffectPoolItem != null,
                "BulletImpactEffect: BulletImpactEffectPoolItem is not assigned.");
            Debug.Assert(poolManager != null, "BulletImpactEffect: PoolManager is not assigned.");
        }

        public void PlayEffect(Vector3 position, Vector3 normal)
        {
            PoolingEffect bulletImpactEffect = poolManager.Pop(bulletImpactEffectPoolItem) as PoolingEffect;
            bulletImpactEffect.PlayVFX(position, Quaternion.LookRotation(normal));
        }
    }
}