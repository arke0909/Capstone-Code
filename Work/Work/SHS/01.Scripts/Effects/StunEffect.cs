using System;
using Chipmunk.Library.Utility.GameEvents.Local;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Effects;
using SHS.Scripts.Combats.Events;
using UnityEngine;

namespace SHS.Scripts.Effects
{
    public class StunEffect : MonoBehaviour, ILocalEventSubscriber<StunnedEvent>
    {
        [SerializeField] private PoolManagerSO poolManager;
        [SerializeField] private PoolItemSO stunExplosionEffectPoolItem;
        [SerializeField] private ParticleSystem stunEffect;

        private float _stunDuration;
        private float _elapsedTime;

        public void OnLocalEvent(StunnedEvent eventData)
        {
            PoolingEffect stunExplosionEffect = poolManager.Pop(stunExplosionEffectPoolItem) as PoolingEffect;
            if (stunExplosionEffect == null)
                return;

            stunExplosionEffect.PlayVFX(transform.position, Quaternion.identity);
            _stunDuration = eventData.StunDuration;
            _elapsedTime = 0f;
            this.stunEffect.Play();
        }

        private void Update()
        {
            if (_elapsedTime < _stunDuration)
            {
                _elapsedTime += Time.deltaTime;
                if (_elapsedTime >= _stunDuration)
                {
                    this.stunEffect.Stop();
                }
            }
        }
    }
}