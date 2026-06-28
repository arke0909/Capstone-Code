using System;
using Code.UI.Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Work.Code.UI.ContextMenu;
using Code.Items.ItemInfo;
using Work.Code.UI.Slots;

namespace Work.Code.Craft
{
    public class CraftItemUI : BaseSlotUI
    {
        [SerializeField] private ContextMenuSO craftItemMenu;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Image icon;
        [SerializeField] private Image pin;
        [SerializeField] private Image background;
        [SerializeField] private Image star;
        [SerializeField] private float disableDarkAmount = 0.65f;
        
        private Sequence _showAnimSeq;
        private LayoutElement _layoutElement;
        private Color _iconColor;
        private Color _backgroundColor;
        
        private readonly float _animDuration = 0.3f;
        private const string TooltipText = "우클릭으로 메뉴 열기";

        [field: SerializeField] public Button ItemButton { get; set; }
        public CraftTreeSO Tree { get; private set; }
        public bool IsFavorite { get; private set; }
        public bool IsPinned { get; private set; }
        public bool IsInteractable => ItemButton.interactable;
        
        public event Action<CraftItemUI, bool> OnPinItem;
        public event Action<CraftTreeSO> OnRequestCraft;

        protected override void Awake()
        {
            base.Awake();
            _layoutElement = GetComponent<LayoutElement>();
            BindTooltip(() => TooltipText, 0.5f);
            BindTooltip(() => Tree.Item, 0.5f);
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
            _iconColor = Color.white;
            _backgroundColor = UIDefine.RarityColors[(int)item.rarity];
            icon.color = _iconColor;
            background.color = _backgroundColor;
            title.text = item.itemName;
            star.gameObject.SetActive(IsFavorite);
            RefreshInteractableColor();
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

        public void SetInteractable(bool isInteractable)
        {
            ItemButton.interactable = isInteractable;
            RefreshInteractableColor();
        }

        public void RequestCraft()
        {
            if (!IsInteractable)
                return;

            OnRequestCraft?.Invoke(Tree);
        }

        public void RefreshCraftableEffect(bool isCraftable, Color effectColor)
        {
            if (isCraftable)
            {
                if (IsBackgroundEffectPlaying)
                    return;

                PlayBackgroundEffect(effectColor);
                return;
            }

            if (!IsBackgroundEffectPlaying)
                return;

            StopBackgroundEffect();
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

        private void RefreshInteractableColor()
        {
            if (IsInteractable)
            {
                icon.color = _iconColor;
                background.color = _backgroundColor;
                return;
            }

            icon.color = GetDarkColor(_iconColor);
            background.color = GetDarkColor(_backgroundColor);
        }

        private Color GetDarkColor(Color color)
        {
            Color darkColor = Color.Lerp(color, Color.black, disableDarkAmount);
            darkColor.a = color.a;
            return darkColor;
        }
    }
}
