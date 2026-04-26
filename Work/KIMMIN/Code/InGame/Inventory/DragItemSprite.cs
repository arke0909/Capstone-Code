using Chipmunk.GameEvents;
using Code.UI.Core;
using UnityEngine;
using UnityEngine.UI;
using Work.Code.SkillInventory.GameEvents;

namespace InGame.InventorySystem
{
    public class DragItemSprite : UIBase
    {
        [SerializeField] private float strength = 0.03f;
        [SerializeField] private float maxStrength = 25f;
        [SerializeField] private float smooth = 15f;
        
        private Image _icon;
        private RectTransform _rect;
        private Vector2 _prevMousePos;
        private Vector3 _velocity;
        
        private bool _isDragging;

        protected override void Awake()
        {
            base.Awake();
            _icon = GetComponent<Image>();
            _rect = GetComponent<RectTransform>();
            _prevMousePos = Input.mousePosition;
            
            DisableUI();
            EventBus.Subscribe<DragEvent>(HandleDrag);
        }

        private void HandleDrag(DragEvent evt)
        {
            _icon.sprite = evt.Sprite;
            _isDragging = evt.IsDragStart;
            
            if (_isDragging)
                EnableUI();
            else
                DisableUI();
            
            _icon.transform.position = Input.mousePosition;
        }

        private void Update()
        {
            if (_isDragging)
                Drag();
        }

        public void Drag()
        {
            Vector2 mousePos = Input.mousePosition;
            transform.position = mousePos;
            _velocity = (mousePos - _prevMousePos) / Time.unscaledDeltaTime;
            _prevMousePos = mousePos;
            ApplyRotate();
        }
        
        private void ApplyRotate()
        {
            float targetZ = _velocity.x * strength;
            targetZ = Mathf.Clamp(targetZ, -maxStrength, maxStrength);

            Quaternion targetRot = Quaternion.Euler(0f, 0f, -targetZ);
            _rect.rotation = Quaternion.Lerp
                (_rect.rotation, targetRot, Time.unscaledDeltaTime * smooth);
        }
    }
}