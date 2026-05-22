using EPOOutline;
using Scripts.Entities;
using UnityEngine;
using Work.Code.UI;
using Work.Code.UI.Interaction;
using Code.ItemContainers;

namespace Scripts.GameSystem
{
    public abstract class InteractableStructure : MonoBehaviour, IInteractable
    {
        [SerializeField] private AppearEffect helpText;
        [SerializeField] private InteractVisualUI interactVisualUI;

        [field: SerializeField] public Outlinable Outlinable { get; private set; }

        private Camera _cam;
        private bool _isSelected;
        protected virtual void Awake()
        {
            _cam = Camera.main;
        }
        protected virtual void Start()
        {
            Outlinable.enabled = false;
            helpText.Disappear();
            interactVisualUI.StopHighlight();
        }

        private void LateUpdate()
        {
            if (_isSelected)
            {
                helpText.transform.forward = _cam.transform.forward;
            }
            
            interactVisualUI.transform.forward = _cam.transform.forward;
        }
        
        public virtual void Select()
        {
            if (_isSelected) return;

            helpText.Appear();
            interactVisualUI.PlayHighlight();
            
            _isSelected = true;
            Outlinable.enabled = true;
        }

        public virtual void DeSelect()
        {
            helpText.Disappear();
            interactVisualUI.StopHighlight();
            
            _isSelected = false;
            Outlinable.enabled = false;
        }

        public abstract void Interact(Entity interactor);
    }
}
