using System;
using Code.UI.Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Work.Code.UI.ContextMenu;
using Work.Code.UI.Core.Interaction;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Craft
{
    public class CraftItemUI : InteractableUI
    {
        [SerializeField] private ContextMenuSO craftItemMenu;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Image icon;
        [SerializeField] private Image pin;
        [SerializeField] private Image background;
        [SerializeField] private Image outline;
        [SerializeField] private Image star;
        
        private Sequence _showAnimSeq;
        private LayoutElement _layoutElement;
        
        private readonly float _animDuration = 0.3f;
        private const string TooltipText = "우클릭으로 메뉴 열기";

        [field: SerializeField] public Button ItemButton { get; set; }
        public CraftTreeSO Tree { get; private set; }
        public bool IsFavorite { get; private set; }
        public bool IsPinned { get; private set; }
        
        public event Action<CraftItemUI, bool> OnPinItem;
        public event Action<CraftTreeSO> OnRequestCraft;

        protected override void Awake()
        {
            base.Awake();
            _layoutElement = GetComponent<LayoutElement>();
            BindTooltip(() => TooltipText, 0.5f);
            BindContextMenu(craftItemMenu, () => this);
        }

        public void ToggleFavorite()
        {
            IsFavorite = !IsFavorite;
            star.gameObject.SetActive(IsFavorite);
        }

        public void RefreshUI(ItemDataSO item, bool hasAnim)
        {
            EnableUI();

            if (hasAnim)
                EnableTween();

            icon.sprite = item.itemImage;
            background.color = UIDefine.RarityColors[(int)item.rarity];
            title.text = item.itemName;
            star.gameObject.SetActive(IsFavorite);
        }

        public override void EnableUI(bool isFade = false)
        {
            base.EnableUI(isFade);
            _layoutElement.ignoreLayout = false;
        }

        public override void DisableUI(bool isFade = false)
        {
            base.DisableUI(isFade);
            _layoutElement.ignoreLayout = true;
        }

        public void SetTree(CraftTreeSO tree) => Tree = tree;

        public void SetPin(bool isPinned)
        {
            IsPinned = isPinned;
            pin.gameObject.SetActive(isPinned);
        }

        public void TogglePin()
        {
            OnPinItem?.Invoke(this, !IsPinned);
        }

        public void RequestCraft()
        {
            OnRequestCraft?.Invoke(Tree);
        }
        
        private void EnableTween()
        {
            background.DOKill();
            icon.DOKill();
            outline.DOKill();
            _showAnimSeq?.Kill();

            background.transform.localScale = Vector3.one * 0.925f;
            icon.transform.localScale = Vector3.one * 0.85f;
            outline.color = new Color(outline.color.r, outline.color.g, outline.color.b, 0f);
            background.color = new Color(background.color.r, background.color.g, background.color.b, 0f);
            icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, 0f);

            _showAnimSeq = DOTween.Sequence();
            _showAnimSeq.Join(background.transform.DOScale(1f, _animDuration).SetEase(Ease.OutCubic));
            _showAnimSeq.Join(background.DOFade(1f, _animDuration).SetEase(Ease.OutCubic));
            _showAnimSeq.Join(icon.transform.DOScale(1f, _animDuration).SetEase(Ease.OutCubic));
            _showAnimSeq.Join(icon.DOFade(1f, _animDuration).SetEase(Ease.OutCubic));
            _showAnimSeq.Join(outline.DOFade(1f, _animDuration).SetEase(Ease.OutCubic));
            _showAnimSeq.SetAutoKill(true);
        }
    }
}