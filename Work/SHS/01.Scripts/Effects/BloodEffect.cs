using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat.Datas;
using Scripts.Effects;
using SHS.Scripts.Combats.Events;
using UnityEngine;

namespace SHS.Scripts.Effects
{
    public class BloodEffect : MonoBehaviour, IContainerComponent, ILocalEventSubscriber<DamagedEvent>
    {
        [SerializeField] private PoolItemSO bloodEffectPoolItem;
        [SerializeField] private PoolManagerSO poolManager;

        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            Debug.Assert(bloodEffectPoolItem != null, "BloodEffect: BloodEffectPoolItem is not assigned.");
            Debug.Assert(poolManager != null, "BloodEffect: PoolManager is not assigned.");
        }

        public void OnLocalEvent(DamagedEvent eventData)
        {
            PoolingEffect bloodEffect = poolManager.Pop(bloodEffectPoolItem) as PoolingEffect;
            if (bloodEffect == null || eventData.DamageData.damageType == DamageType.DOT)
                return;

            bloodEffect.PlayVFX(eventData.HitPoint, Quaternion.LookRotation(eventData.HitNormal));
        }
    }
}
