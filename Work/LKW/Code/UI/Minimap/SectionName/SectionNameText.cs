using System;
using System.Collections.Generic;
using Chipmunk.GameEvents;
using Code.UI.Minimap.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Work.Code.GameEvents;
using Code.Items.ItemInfo;

namespace Code.UI.Minimap.SectionName
{
    public class SectionNameText : MinimapElement
    {
        [SerializeField] private SectionShowItem showItemPrefab;
        [SerializeField] private Transform parentTrm;
        
        public TextMeshProUGUI NameText { get; set; }
        public SpawnArea Area { get; set; }
        
        private List<ItemDataSO> _targetItems = new List<ItemDataSO>();
        
        private List<SectionShowItem> _showItems = new List<SectionShowItem>();
        
        protected override void Awake()
        {
            base.Awake();
            NameText = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            Bus.Subscribe<ShowItemsOnMap>(HandleShowItemsOnMap);
            Bus.Subscribe<ChangePinCountEvent>(HandleChangePinCount);
        }

       
        private void OnDisable()
        {
            Bus.Unsubscribe<ShowItemsOnMap>(HandleShowItemsOnMap);
            Bus.Unsubscribe<ChangePinCountEvent>(HandleChangePinCount);
        }
        
        private void HandleChangePinCount(ChangePinCountEvent evt)
        {
            if (evt.Count == 0)
            {
                ClearSectionItems();
            }
        }

        private void HandleShowItemsOnMap(ShowItemsOnMap evt)
        {
            ClearSectionItems();

            foreach (var item in evt.ItemList)
            {
                if ((item.spawnArea & Area) > 0)
                {
                    _targetItems.Add(item);
                }
            }

            for (int i = 0; i < _targetItems.Count; i++)
            {
                SectionShowItem item = Instantiate(showItemPrefab, parentTrm);
                item.ItemData = _targetItems[i];
                item.Image.sprite = _targetItems[i].itemImage;
                _showItems.Add(item);
            }
            
            // 나중에 핀해제 취소나 타겟 아이템 완성하면 아이템들 없애는 것도 해줘야함
        }

        private void ClearSectionItems()
        {
            for (int i = parentTrm.childCount - 1; i >= 0; i--)
            {
                Destroy(parentTrm.GetChild(i).gameObject);
            }

            _targetItems.Clear();  // 삭제 후 리스트 정리
            _showItems.Clear();
        }
    }
}