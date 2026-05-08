using Chipmunk.ComponentContainers;
using Code.EnemySpawn;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Entities;
using Scripts.Players;
using UnityEngine;
using Work.Code.MapEvents;

namespace Scripts.GameSystem.GameEvents
{
    public class PortalStructure : InteractableStructure, IPoolable, IDropStructure
    {
        [field:SerializeField] public PoolItemSO PoolItem { get; set; }

        public GameObject GameObject => gameObject;
        private BossSpawner _targetSpawner;
        private Pool _myPool;
        public void Init(BossSpawner targetSpawner,Vector3 initPos)
        {
            transform.position = initPos;
            _targetSpawner = targetSpawner;
        }
        public override void Interact(Entity interactor)
        {
            Debug.Assert(_targetSpawner != null, "Init이 호출안됐는데");
            _targetSpawner.Enter(interactor);
        }

        public void ResetItem()
        {
            _targetSpawner = null;
        }

        public void Cancel()
        {
            Debug.Log("ASDSAD");
            _targetSpawner.Canceled();
            _myPool.Push(this);
        }

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }
    }
}
