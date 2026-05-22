using Chipmunk.ComponentContainers;
using Code.EnemySpawn;
using DewmoLib.ObjectPool.RunTime;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Scripts.Entities;
using Scripts.GameSystem;
using Scripts.Players;
using System;
using UnityEngine;
using Work.Code.MapEvents;

namespace Scripts.GameSystem.Structures
{
    public class InvokeCallbackStructure : InteractableStructure, ISpawnableStructure
    {
        public GameObject GameObject => gameObject;
        protected Action<Entity> _callback;
        protected Action _despawnCallback;
        public void Init(Action<Entity> interactCallback,Action despawnCallback)
        {
            _callback = interactCallback;
            _despawnCallback = despawnCallback;
        }
        public override void Interact(Entity interactor)
        {
            Debug.Assert(_callback != null, "Init이 호출안됐는데");
            _callback?.Invoke(interactor);
        }

        public void Spawn(Vector3 targetPos)
        {
            gameObject.SetActive(true);
            transform.position = targetPos - Vector3.one * 2;
            transform.DOMove(targetPos, 1f);
        }
        public void Despawn()
        {
            _despawnCallback?.Invoke();
            Vector3 targetPos = transform.position - Vector3.up * 2;
            transform.DOMove(targetPos, 1f).OnComplete(OnDespawnComplete);
        }

        protected virtual void OnDespawnComplete()
        {
            gameObject.SetActive(false);
        }
    }
}
