using System;
using Chipmunk.Library.Utility.GameEvents.Local;
using SHS.Scripts.Combats.Events;
using System.Collections.Generic;
using UnityEngine;

namespace Code.SHS.Entities.Enemies
{
    public class DeathRagdollBehavior : MonoBehaviour, ILocalEventSubscriber<EntityDeadEvent>
    {
        private Animator _animator;
        private Rigidbody[] _rigidbodies;
        private Collider[] _colliders;

        private bool _isRagdollActive;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rigidbodies = GetComponentsInChildren<Rigidbody>();
            _colliders = GetComponentsInChildren<Collider>();
            foreach (var rb in _rigidbodies)
            {
                rb.isKinematic = true;
            }

            foreach (var col in _colliders)
            {
                col.enabled = false;
            }
        }

        public void OnLocalEvent(EntityDeadEvent eventData)
        {
            Ragdoll(eventData.HitPoint, -eventData.HitNormal, 10f);
        }

        public void Ragdoll(Vector3 hitPoint, Vector3 direction, float force)
        {
            if (_isRagdollActive) return;

            _isRagdollActive = true;
            _animator.enabled = false;

            foreach (var rb in _rigidbodies)
            {
                rb.isKinematic = false;
                rb.AddForceAtPosition(direction.normalized * force, hitPoint, ForceMode.Impulse);
            }

            foreach (var col in _colliders)
            {
                col.enabled = true;
            }
        }
    }
}