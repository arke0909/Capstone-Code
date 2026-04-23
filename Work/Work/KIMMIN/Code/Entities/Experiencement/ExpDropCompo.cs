using System;
using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Entities;
using UnityEngine;

namespace Work.Code.Entities.Experiencement
{
    public class ExpDropCompo : MonoBehaviour, IContainerComponent
    {
        [SerializeField] private PoolManagerSO poolManager;
        [SerializeField] private PoolItemSO orbPool;
        [SerializeField] private int dropExp;
        [SerializeField] private float dropRange;

        public ComponentContainer ComponentContainer { get; set; }
        private Entity _owner;
        
        public void OnInitialize(ComponentContainer componentContainer)
        {
            _owner = componentContainer.GetSubclassComponent<Entity>();
            _owner?.OnDeadEvent.AddListener(HandleDeadEvent);
        }

        private void OnDestroy()
        {
            _owner?.OnDeadEvent.RemoveListener(HandleDeadEvent);
        }

        private void HandleDeadEvent()
        {
            int count = dropExp / 5;
            for (int i = 0; i < count; i++)
            {
                var orb = poolManager.Pop(orbPool) as ExpOrb;
                orb?.InitOrb(_owner.transform.position, dropRange);
                ExpManager.Instance?.RegisterOrb(orb);
            }
        }
    }
}