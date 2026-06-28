using System;
using System.Collections.Generic;
using Chipmunk.ComponentContainers;
using UnityEngine;

namespace SHS.Scripts.Summon.Turrets
{
    public class EngineerTurretTracker : MonoBehaviour, IContainerComponent
    {
        private readonly List<GameObject> _turrets = new();

        public ComponentContainer ComponentContainer { get; set; }
        public int ActiveTurretCount
        {
            get
            {
                RemoveDestroyedTurrets();
                return _turrets.Count;
            }
        }

        public void OnInitialize(ComponentContainer componentContainer)
        {
        }

        public void Register(GameObject turretObject)
        {
            if (turretObject == null)
                throw new ArgumentNullException(nameof(turretObject));

            if (_turrets.Contains(turretObject))
                return;

            _turrets.Add(turretObject);
        }

        public void Unregister(GameObject turretObject)
        {
            _turrets.Remove(turretObject);
        }

        private void LateUpdate()
        {
            RemoveDestroyedTurrets();
        }

        private void RemoveDestroyedTurrets()
        {
            _turrets.RemoveAll(turretObject => turretObject == null);
        }
    }
}
