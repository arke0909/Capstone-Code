using Chipmunk.ComponentContainers;
using Scripts.Entities;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Work.LKW.Code.ItemContainers;

namespace Scripts.Players
{
    public class PlayerInteract : MonoBehaviour, IContainerComponent
    {
        [SerializeField] private float radius;
        [SerializeField] private LayerMask interactLayer;
        public ComponentContainer ComponentContainer { get; set; }
        private Collider[] _colliders = new Collider[5];
        private IInteractable _target;
        private Player _player;
        public void OnInitialize(ComponentContainer componentContainer)
        {
            _player = componentContainer.Get<Player>();
            _player.PlayerInput.OnInteractPressed += HandleInteract;
        }
        private void OnDestroy()
        {
            _player.PlayerInput.OnInteractPressed -= HandleInteract;
        }
        private void Update()
        {
            int cnt = Physics.OverlapSphereNonAlloc(transform.position, radius,_colliders, interactLayer);
            float closestDistance = float.MaxValue;
            if (cnt == 0)
            {
                _target?.DeSelect();
                _target = null;
            }
            for (int i = 0; i < cnt; i++)
            {
                if (_colliders[i].TryGetComponent(out IInteractable interactable) == false)
                    continue;

                float distance = Vector3.Distance(transform.position, _colliders[i].transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    if (_target != interactable)
                        _target?.DeSelect();
                    _target = interactable;
                    _target.Select();
                }
            }
        }
        private void HandleInteract()
        {
            _target?.Interact(_player);
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
#endif
    }
}
