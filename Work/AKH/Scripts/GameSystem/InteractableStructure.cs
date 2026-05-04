using EPOOutline;
using Scripts.Entities;
using Unity.AppUI.UI;
using UnityEngine;
using Work.Code.UI;
using Work.LKW.Code.ItemContainers;

namespace Scripts.GameSystem
{
    public abstract class InteractableStructure : MonoBehaviour, IInteractable
    {
        [SerializeField] private AppearEffect helpText;

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
        }

        private void LateUpdate()
        {
            if (_isSelected)
            {
                helpText.transform.forward = _cam.transform.forward;
            }
        }
        public void Select()
        {
            _isSelected = true;
            helpText.Appear();
            Outlinable.enabled = true;
        }

        public void DeSelect()
        {
            helpText.Disappear();
            _isSelected = false;
            Outlinable.enabled = false;
        }

        public abstract void Interact(Entity interactor);

    }
}
