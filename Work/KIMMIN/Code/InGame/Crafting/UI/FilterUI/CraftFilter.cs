using System;
using TMPro;
using UnityEngine;
using Code.Items.ItemInfo;

namespace Work.Code.Craft
{
    public class CraftFilter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI typeText;
        [SerializeField] private FavoriteSelectUI notFavoriteButton;
        [SerializeField] private FavoriteSelectUI favoriteButton;

        private CraftTypeUI[] _craftTypes;
        private CraftTypeUI _typeUI;
        private bool _isFavorite;

        public event Action<ItemType[], bool> OnRefreshCraftUI;

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

            OnRefreshCraftUI?.Invoke(_typeUI != null ? _typeUI.ItemTypes : null, state);
        }

        private void HandleSelectType(CraftTypeUI typeUI)
        {
            bool isMatchType = typeUI == _typeUI;

            _typeUI = isMatchType ? null : typeUI;
            typeText.text = isMatchType ? "전체 아이템" : typeUI.ItemName;
            OnRefreshCraftUI?.Invoke(_typeUI != null ? _typeUI.ItemTypes : null, _isFavorite);
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
