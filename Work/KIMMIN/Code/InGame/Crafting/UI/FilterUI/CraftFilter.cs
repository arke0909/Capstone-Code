using System;
using TMPro;
using UnityEngine;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Craft
{
    public class CraftFilter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI typeText;
        [SerializeField] private FavoriteSelectUI notFavoriteButton;
        [SerializeField] private FavoriteSelectUI favoriteButton;

        private CraftTypeUI[] _craftTypes;
        private ItemType _type = ItemType.None;
        private bool _isFavorite;
        
        public event Action<ItemType, bool> OnRefreshCraftUI;

        private void Awake()
        {
            _craftTypes = GetComponentsInChildren<CraftTypeUI>();

            foreach (CraftTypeUI craftType in _craftTypes)
            {
                craftType.OnItemSelected += HandleSelectType;
            }

            favoriteButton.SelectButton.onClick.AddListener(HandleFavorite);
            notFavoriteButton.SelectButton.onClick.AddListener(HandleUnFavorite);

            HandleUnFavorite();
        }

        private void HandleUnFavorite()
        {
            SetFavoriteState(false);
        }

        private void HandleFavorite()
        {
            SetFavoriteState(true);
        }
        
        private void SetFavoriteState(bool state)
        {
            _isFavorite = state;
            favoriteButton.OnSelect(state);
            notFavoriteButton.OnSelect(!state);
            
            OnRefreshCraftUI?.Invoke(_type, state);
        }

        private void HandleSelectType(ItemType type)
        {
            bool isMatchType = type == _type;
            
            _type = isMatchType ? ItemType.None : type;
            typeText.text = isMatchType ? "전체 아이템" : type.ToString();
            OnRefreshCraftUI?.Invoke(_type, _isFavorite);
        }

        private void OnDestroy()
        {
            foreach (CraftTypeUI craftType in _craftTypes)
            {
                craftType.OnItemSelected -= HandleSelectType;
            }
            
            favoriteButton.SelectButton.onClick.RemoveListener(HandleFavorite);
            notFavoriteButton.SelectButton.onClick.RemoveListener(HandleUnFavorite);
        }
    }
}