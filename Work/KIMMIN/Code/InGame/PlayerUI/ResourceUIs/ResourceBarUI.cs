using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using DewmoLib.Dependencies;
using Scripts.Entities;
using Scripts.Entities.Vitals;
using Scripts.Players;
using UnityEngine;
using UnityEngine.Serialization;

namespace InGame.PlayerUI.ResourceUIs
{
    public class ResourceBarUI<T> : MonoBehaviour,IContainerComponent where T : IVitalEvent
    {
        [FormerlySerializedAs("resourceUI")]
        [SerializeField] private ResourceUI resourceUI;
        [SerializeField] private Canvas canvas;
        [SerializeField] private Vector3 offset = new Vector3(1f, 0.5f, 0.25f);
        private Entity _owner;
        private LocalEventBus _eventBus;
        private Camera _cam;

        public ComponentContainer ComponentContainer { get ; set; }

        private void Start()
        {
            _cam = Camera.main;
            canvas.worldCamera = _cam;

            _eventBus.Subscribe<T>(HandleResourceChange);
            resourceUI.DisableUI();
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<T>(HandleResourceChange);
        }

        protected void HandleResourceChange(T evt)
        {
            if (evt.Value >= evt.MaxValue)
            {
                if (resourceUI.IsActive)
                {
                    resourceUI.DisableUI(true);
                }

                return;
            }

            if (!resourceUI.IsActive)
            {
                resourceUI.EnableUI(true);
            }

            resourceUI.SetFill(evt.Value / evt.MaxValue);
        }

        private void LateUpdate()
        {
            if (_cam == null)
                return;

            gameObject.transform.forward = _cam.transform.forward;
            gameObject.transform.position = _owner.transform.position + offset;
        }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _owner = componentContainer.GetSubclassComponent<Entity>();
            _eventBus = _owner.Get<LocalEventBus>();
        }
    }
}
