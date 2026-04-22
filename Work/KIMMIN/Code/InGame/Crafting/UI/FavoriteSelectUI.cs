using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Work.Code.Crafting
{
    public class FavoriteSelectUI : MonoBehaviour
    {
        [field: SerializeField] public Button SelectButton { get; private set; }
        [SerializeField] private RectTransform rect;
        [SerializeField] private Image background;
        
        [SerializeField] private Color selectedColor;
        [SerializeField] private Color deselectedColor;

        public void OnSelect(bool isSelected)
        {
            float ySize = isSelected ? 40 : 35;
            background.color = isSelected ? selectedColor : deselectedColor;
            var size = new Vector3(rect.localScale.x, ySize, rect.localScale.z);
            rect.DOSizeDelta(size, 0.15f);
        }

        private void OnValidate()
        {
            if(background != null)
                background.color = deselectedColor;
        }
    }
}