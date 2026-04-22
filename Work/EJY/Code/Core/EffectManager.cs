using System;
using Chipmunk.GameEvents;
using Code.GameEvents;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Effects;
using UnityEngine;

namespace Work.EJY.Code.Core
{
    public class EffectManager : MonoBehaviour
    {
        [Inject] private PoolManagerMono _poolManager;

        private void Awake()
        {
            Bus.Subscribe<PlayEffectEvent>(HandlePlayEffectEvent);
        }

        private void HandlePlayEffectEvent(PlayEffectEvent evt)
        {
            PoolingEffect effect = _poolManager.Pop<PoolingEffect>(evt.PoolItemSO);
            effect.PlayVFX(evt.Position, evt.Rotation, evt.Scale);
        }
    }
}