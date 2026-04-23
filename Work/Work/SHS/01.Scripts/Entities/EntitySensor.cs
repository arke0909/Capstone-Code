using System;
using System.Collections.Generic;
using Chipmunk.ComponentContainers;
using Chipmunk.Modules.StatSystem;
using Scripts.Combat;
using Scripts.Entities;
using SHS.Scripts.Combats.Events;
using UnityEngine;

namespace SHS.Scripts.Entities.Players
{
    [RequireComponent(typeof(SphereCollider))]
    public class EntitySensor : MonoBehaviour, IContainerComponent
    {
        [SerializeField] private StatSO detectRangeStat;
        private Entity _owner;

        public delegate void OnDetectEventHandler(Entity detectedEntity);

        public event OnDetectEventHandler OnDetectEnter;
        public event OnDetectEventHandler OnDetectExit;
        private HashSet<Entity> _detectedEntities = new HashSet<Entity>();
        public IReadOnlyCollection<Entity> DetectedEntities => _detectedEntities;

        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _owner = componentContainer.GetSubclassComponent<Entity>();
            detectRangeStat = componentContainer.GetCompo<StatBehavior>(true).GetStat(detectRangeStat);
            SphereCollider sphereCollider = GetComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = detectRangeStat.Value;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Entity entity) && entity != _owner)
            {
                AddEntity(entity);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out Entity entity) && entity != _owner)
            {
                RemoveEntity(entity);
            }
        }

        private void AddEntity(Entity entity)
        {
            if (_detectedEntities.Add(entity))
            {
                OnDetectEnter?.Invoke(entity);
                entity.LocalEventBus.Subscribe<EntityDeadEvent>(HandleEntityDead);
            }
        }

        private void RemoveEntity(Entity entity)
        {
            if (_detectedEntities.Remove(entity))
            {
                OnDetectExit?.Invoke(entity);
                entity.LocalEventBus.Unsubscribe<EntityDeadEvent>(HandleEntityDead);
            }
        }

        private void HandleEntityDead(EntityDeadEvent obj)
        {
            if (obj.Entity == null) return;
            RemoveEntity(obj.Entity);
        }
    }
}