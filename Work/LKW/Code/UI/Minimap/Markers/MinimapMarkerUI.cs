using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Chipmunk.GameEvents;
using Code.Events;
using Code.UI.Minimap.Core;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Work.Code.GameEvents;

namespace Code.UI.Minimap.Markers
{

    public class MinimapMarkerUI : MonoBehaviour
    {
        [Header("UI Transforms")] 
        [SerializeField] private GameObject gridPanel;  // 버튼들이 생성될 부모 패널
        [SerializeField] private TextMeshProUGUI countText;
        
        [Header("Prefabs")]
        [SerializeField] private Button markerSelectBtnPrefab;  // 하단 선택 버튼

        [Header("Data")]
        [SerializeField] private MarkerIconListSO markerIconListSo;
        [SerializeField] private int maxMarkerCount = 10;

        [Header("Pool")]
        [SerializeField] private PoolItemSO supplyMarkerItem;  // 하단 선택 버튼

        [Inject] private MinimapSystem _minimapSystem;
        
        private Dictionary<Button, markerData> _markerDataDictByButton = new Dictionary<Button, markerData>();
        private markerData _selectedMarker = null;
        private int _currentCount = 0;

        private void Start()
        {
            InitMarkerSelectButtons();
            UpdateCountText();
            _selectedMarker = _markerDataDictByButton.First().Value;
        }

        private void OnEnable()
        {
            Bus.Subscribe<AirdropEvent>(HandleAirdropEvent);
        }
        
        private void OnDisable()
        {
            Bus.Unsubscribe<AirdropEvent>(HandleAirdropEvent);
        }
        
        private void HandleAirdropEvent(AirdropEvent evt)
        {
            MinimapUtil.AddToMinimap(this, ElementType.SupplyIcon, null, false, evt.Position);
        }

        private void Update()
        {
            HandleRightClick();
        }

        private void HandleRightClick()
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                if(_minimapSystem.IsActiveMinimap == false) return;
                
                Vector2 mousePos = Mouse.current.position.ReadValue();
                
                if (TryRemoveMarker(mousePos)) return;
                
                if (_selectedMarker != null)
                {
                    CreateMarker(mousePos);
                }
            }
        }

        private void InitMarkerSelectButtons()
        {
            if (markerIconListSo == null || markerIconListSo.markerList.Count == 0) return;

            foreach (var data in markerIconListSo.markerList)
            {
                Button selectBtn = Instantiate(markerSelectBtnPrefab, gridPanel.transform);
                
                selectBtn.image.sprite = data.markerIcon;
                
                _markerDataDictByButton.Add(selectBtn, data);
                selectBtn.onClick.AddListener(() => {
                    _selectedMarker = _markerDataDictByButton[selectBtn];
                });
            }

            _selectedMarker = markerIconListSo.markerList.First();
        }

        private void CreateMarker(Vector2 mousePos)
        {
            if (_currentCount >= maxMarkerCount) return;
            
            if (!RectTransformUtility.RectangleContainsScreenPoint(_minimapSystem.MinimapRect, mousePos)) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_minimapSystem.MinimapRect, mousePos, null, out var localPos);

            MinimapUtil.AddToMinimap(
                this,ElementType.Marker ,
                _selectedMarker.markerIcon,false, 
                _minimapSystem.MinimapToWorldPosition(localPos));
            
            _currentCount++;
            UpdateCountText();
        }

        private bool TryRemoveMarker(Vector2 mousePos)
        {
            foreach (Transform child in _minimapSystem.MinimapRect.transform)
            {
                if (child.TryGetComponent(out Marker marker))
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(marker.Rect, mousePos) && marker.gameObject.activeInHierarchy)
                    {
                        marker.RemoveSelf();
                        _currentCount = Mathf.Max(0, _currentCount - 1);
                        UpdateCountText();
                
                        return true;
                    }
                }
            }
            return false;
        }

        private void UpdateCountText()
        {
            if (countText != null)
                countText.SetText($"{_currentCount}/{maxMarkerCount}");
        }
    }
}