using Chipmunk.GameEvents;
using Code.Hotbar;
using Code.InventorySystems.Items;
using Code.UI.Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Work.LKW.Code.Items;
using static Code.InventorySystems.InventoryUtility;

namespace Code.InGame.Hotbar
{
    public class HotbarUI : MonoBehaviour, IUIElement<ItemSlot>
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI keyText;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private Image icon;
        [SerializeField] private Image background;
        
        private Tween _hotbarTween;
        private readonly Color32 _inActiveColor = new Color32(0, 0, 0, 100);

        public int Index { get; private set; }

        private void Awake()
        {
            button.onClick.AddListener(OnPressed);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnPressed);
        }

        public void OnPressed()
        {
            _hotbarTween.Kill();
            
            transform.localScale = Vector3.one;
            _hotbarTween = transform.DOScale(0.9f, 0.07f)
                .SetLoops(2, LoopType.Yoyo);
            
            EventBus.Raise(new HotbarUseEvent(GetLocalIndex(Index)));
        }

        public void EnableFor(ItemSlot slot)
        {
            if(slot == null) return;

            if (slot.Item is EquipableItem equipableItem)
            {
                countText.gameObject.SetActive(true);
                icon.gameObject.SetActive(true);
                
                icon.sprite = equipableItem.ItemData.itemImage;
                countText.text = slot.Stack.ToString();
                
                Color targetColor = UIDefine.RarityColors[(int)slot.Item.ItemData.rarity];
                targetColor.a = 0.5f;
                background.color = targetColor;
            }
        }

        public void ClearUI()
        {
            icon.gameObject.SetActive(false);
            countText.gameObject.SetActive(false);
            background.color = _inActiveColor;
        }
        
        private void SetIndexText(int idx)
        {
            if (keyText != null)
            {
                keyText.text = (idx + 1).ToString();
            }
        }

        public void SetIndex(int idx)
        {
            SetIndexText(idx);
            Index = idx + (int)SlotType.Hotbar;
        }
    }
}