using Code.UI.Core;
using Code.UI.Core.Interaction;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Work.Code.UI.Core.Interaction;

namespace Work.Code.Crafting
{
    public struct CraftNodeData
    {
        public NodeData data;
        public int count;
        public bool isNeedItem;

        public CraftNodeData(NodeData data, int count = 1, bool isNeedItem = false)
        {
            this.data = data;
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

        private string _tooltipText = "클릭해 하위 트리로 이동";
        private float _duration = 0.4f;
        private Sequence _enableSeq;
        
        [field: SerializeField] public RectTransform LineStartRect { get; set; }
        [field: SerializeField] public RectTransform LineEndRect { get; set; }
        [field: SerializeField] public Button NodeButton { get; set; }
        
        public RectTransform Rect => icon.transform as RectTransform;
        public NodeData Data { get; set; }

        public void InitUI(CraftNodeData nodeData, bool isNotificate = true)
        {
            if (background == null) return;
            
            var data = nodeData.data;
            gameObject.SetActive(true);
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

            Data = data;

            SubscribeEvents();

            if (isNotificate)
                EnableTween();
        }

        private void SubscribeEvents()
        {
            UnbindTooltip();
            BindTooltip(() => Data.Item);
        }
        
        private void EnableTween()
        {
            _enableSeq?.Kill();

            background.transform.localScale = Vector3.one * 0.85f;
            icon.transform.localScale = Vector3.one * 0.85f;
            outline.color = new Color(outline.color.r, outline.color.g, outline.color.b, 0f);
            background.color = new Color(background.color.r, background.color.g, background.color.b, 0f);
            icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, 0f);

            _enableSeq = DOTween.Sequence();
            _enableSeq.Join(background.transform.DOScale(1f, _duration).SetEase(Ease.OutCubic));
            _enableSeq.Join(icon.transform.DOScale(1f, _duration).SetEase(Ease.OutCubic));
            _enableSeq.Join(outline.DOFade(1f, _duration).SetEase(Ease.OutCubic));
            _enableSeq.Join(background.DOFade(1f, _duration).SetEase(Ease.OutCubic));
            _enableSeq.Join(icon.DOFade(1f, _duration).SetEase(Ease.OutCubic));
            _enableSeq.SetAutoKill(true);
        }

        public void Clear()
        { 
            gameObject.SetActive(false);
            NodeButton?.onClick.RemoveAllListeners();
            UnbindTooltip();
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
    }
}