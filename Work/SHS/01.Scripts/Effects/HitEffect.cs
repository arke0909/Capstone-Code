using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat.Datas;
using Scripts.Effects;
using SHS.Scripts.Combats.Events;
using UnityEngine;

namespace SHS.Scripts.Effects
{
    public class HitEffect : MonoBehaviour, IContainerComponent, ILocalEventSubscriber<DamagedEvent>
    {
        [SerializeField] private PoolItemSO bloodEffectPoolItem;
        [SerializeField] private PoolManagerSO poolManager;

        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            Debug.Assert(bloodEffectPoolItem != null, "HitEffect: BloodEffectPoolItem is not assigned.");
            Debug.Assert(poolManager != null, "HitEffect: PoolManager is not assigned.");
        }

        public void OnLocalEvent(DamagedEvent eventData)
        {
            if (eventData.DamageData.damageType != DamageType.DOT)
                PlayEffect(bloodEffectPoolItem, eventData.HitPoint, eventData.HitNormal);

            if (eventData.DamageData.hitEffectPoolItem != null)
                PlayEffect(eventData.DamageData.hitEffectPoolItem, eventData.HitPoint, eventData.HitNormal);
        }

        private void PlayEffect(PoolItemSO effectPoolItem, Vector3 hitPoint, Vector3 hitNormal)
        {
            PoolingEffect hitEffect = poolManager.Pop(effectPoolItem) as PoolingEffect;
            Debug.Assert(hitEffect != null, $"HitEffect: PoolItem ({effectPoolItem.name}) is not a PoolingEffect.");
            hitEffect.PlayVFX(hitPoint, Quaternion.LookRotation(hitNormal));
        }
    }
}
