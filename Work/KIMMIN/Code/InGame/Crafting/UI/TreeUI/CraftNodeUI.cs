using Code.UI.Core;
using Code.UI.Core.Interaction;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Work.Code.Core.Extension;
using Work.Code.UI.Core.Interaction;

namespace Work.Code.Craft
{
    public struct CraftNodeData
    {
        public NodeData nodeData;
        public int count;
        public bool isNeedItem;

        public CraftNodeData(NodeData nodeData, int count = 1, bool isNeedItem = false)
        {
            this.nodeData = nodeData;
            this.count = count;
            this.isNeedItem = isNeedItem;
        }
    }
    
    public class CraftNodeUI : InteractableUI, IHoverable
    {
        [SerializeField] private Image icon; 
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private Image background;
        [SerializeField] private Image outline;

        private readonly string _tooltipText = "클릭해 하위 트리로 이동";
        private readonly float _animDuration = 0.4f;
        private Sequence _showSequence;
        
        [field: SerializeField] public RectTransform LineStartRect { get; set; }
        [field: SerializeField] public RectTransform LineEndRect { get; set; }
        [field: SerializeField] public Button NodeButton { get; set; }
        
        public NodeData Data { get; set; }

        public void InitUI(CraftNodeData nodeData, bool isNotificate = true)
        {
            if (background == null)
                return;
            
            ApplyVisualUpdate(nodeData);
            Data = nodeData.nodeData;

            if (isNotificate)
            {
                PlayShowAnimation();
            }
            
            SubscribeEvents();
            EnableUI();
        }

        private void ApplyVisualUpdate(CraftNodeData nodeData)
        {
            NodeData data = nodeData.nodeData;
            
            background.color = UIDefine.RarityColors[(int)data.Item.rarity];
            icon.sprite = data.Item.itemImage;
            
            if (nodeData.isNeedItem)
            {
                countText.text = $"{data.Count}개";
                countText.color = Color.white;
            }
            else
            {
                countText.text = $"{nodeData.count}/{data.Count}";
                countText.color = nodeData.count >= data.Count ? Color.white : Color.red;
            }
        }

        private void SubscribeEvents()
        {
            UnbindTooltip();
            BindTooltip(() => Data.Item);
        }

        public void Clear()
        { 
            NodeButton?.onClick.RemoveAllListeners();
            UnbindTooltip();
            DisableUI();
        }

        public void SubscribeClick(UnityAction action)
        {
            NodeButton.onClick.AddListener(action);
        }

        public void SubscribeTooltip()
        {
            BindTooltip(() => _tooltipText);
        }

        public void OnHoverEnter(PointerEventData eventData)
        {
            transform.DOKill();
            transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack);
        }

        public void OnHoverExit(PointerEventData eventData)
        {
            transform.DOKill();
            transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }
        
        private void PlayShowAnimation()
        {
            _showSequence?.Kill();

            background.transform.SetLocalScale(0.85f);
            icon.transform.SetLocalScale(0.85f);
            
            outline.color = new Color(outline.color.r, outline.color.g, outline.color.b, 0f);
            background.color = new Color(background.color.r, background.color.g, background.color.b, 0f);
            icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, 0f);

            _showSequence = DOTween.Sequence()
                .Join(background.transform.DOScale(1f, _animDuration).SetEase(Ease.OutCubic))
                .Join(icon.transform.DOScale(1f, _animDuration).SetEase(Ease.OutCubic))
                .Join(outline.DOFade(1f, _animDuration).SetEase(Ease.OutCubic))
                .Join(background.DOFade(1f, _animDuration).SetEase(Ease.OutCubic))
                .Join(icon.DOFade(1f, _animDuration).SetEase(Ease.OutCubic))
                .SetAutoKill(true);
        }
    }
}