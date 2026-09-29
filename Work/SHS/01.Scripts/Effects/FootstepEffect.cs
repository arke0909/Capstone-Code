using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.SHS.Entities.Enemies;
using Code.SHS.Entities.Enemies.FSM;
using DewmoLib.ObjectPool.RunTime;
using Scripts.FSM.Events;
using Scripts.Players;
using Scripts.Players.States;
using UnityEngine;

namespace SHS.Scripts.Effects
{
    public class FootstepEffect : MonoBehaviour, IContainerComponent,
        ILocalEventSubscriber<FootstepEvent>,
        ILocalEventSubscriber<StateChangedEvent<PlayerStateEnum>>,
        ILocalEventSubscriber<StateChangedEvent<EnemyStateEnum>>
    {
        [SerializeField] private ParticleSystem sprintEffect;
        [SerializeField] private PoolManagerSO poolManager;
        [SerializeField] private PoolItemSO footprintPoolItem;
        [SerializeField] private float groundCheckDistance = 0.75f;

        private CharacterMovement _playerMovement;
        private CharacterNavMovement _enemyMovement;
        private bool _isSprinting;

        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            componentContainer.TryGetComponent(out _playerMovement);
            componentContainer.TryGetComponent(out _enemyMovement);

            Debug.Assert(_playerMovement != null || _enemyMovement != null,
                $"{nameof(FootstepEffect)} requires a character movement component.", this);
            Debug.Assert(sprintEffect != null,
                $"{nameof(FootstepEffect)} requires a sprint particle effect.", this);
            Debug.Assert(poolManager != null && footprintPoolItem != null,
                $"{nameof(FootstepEffect)} requires a footprint pool item.", this);

            sprintEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void OnLocalEvent(FootstepEvent eventData)
        {
            RefreshSprintEffect();

            if (!_isSprinting)
                PlaceFootprint(eventData);
        }

        public void OnLocalEvent(StateChangedEvent<PlayerStateEnum> eventData)
        {
            SetSprintEffect(eventData.CurrentState == PlayerStateEnum.Sprint);
        }

        public void OnLocalEvent(StateChangedEvent<EnemyStateEnum> eventData)
        {
            RefreshSprintEffect();
        }

        private void RefreshSprintEffect()
        {
            bool isSprinting = _playerMovement != null
                ? _playerMovement.MoveType == MoveType.Sprint
                : _enemyMovement.MoveType == NavMoveType.Sprint;

            SetSprintEffect(isSprinting);
        }

        private void SetSprintEffect(bool isSprinting)
        {
            if (_isSprinting == isSprinting)
                return;

            _isSprinting = isSprinting;
            if (_isSprinting)
                sprintEffect.Play(true);
            else
                sprintEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void PlaceFootprint(FootstepEvent eventData)
        {
            Vector3 rayOrigin = eventData.Position + Vector3.up * 0.25f;
            if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit,
                    groundCheckDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                return;

            Vector3 groundForward = Vector3.ProjectOnPlane(eventData.Forward, hit.normal).normalized;
            Quaternion rotation = Quaternion.LookRotation(-hit.normal, groundForward);

            Footprint footprint = poolManager.Pop(footprintPoolItem) as Footprint;
            Debug.Assert(footprint != null,
                $"Pool item ({footprintPoolItem.name}) is not a {nameof(Footprint)}.", this);
            footprint.Place(hit.point + hit.normal * 0.01f, rotation);
        }
    }
}
