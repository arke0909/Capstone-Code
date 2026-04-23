using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Scripts.Entities;
using SHS.Scripts.Combats.Events;
using UnityEngine;
using UnityEngine.Serialization;

namespace SHS.Scripts
{
    public class HitFeedback : MonoBehaviour, IContainerComponent, ILocalEventSubscriber<DamagedEvent>
    {
        [SerializeField] private string hitParam;
        private int hitHash;
        private EntityAnimator _entityAnimator;

        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            hitHash = Animator.StringToHash(hitParam);
            _entityAnimator = componentContainer.Get<EntityAnimator>();
        }

        public void OnLocalEvent(DamagedEvent eventData)
        {
            _entityAnimator.SetParam(hitHash);
        }
    }
}