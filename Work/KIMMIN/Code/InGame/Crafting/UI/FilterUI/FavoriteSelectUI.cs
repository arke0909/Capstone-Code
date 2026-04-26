using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Work.Code.Craft
{
    public class FavoriteSelectUI : MonoBehaviour
    {
        [field: SerializeField] public Button SelectButton { get; private set; }
        [SerializeField] private RectTransform rect;
        [SerializeField] private Image background;
        
        [SerializeField] private Color selectedColor;
        [SerializeField] private Color deselectedColor;

        private readonly float _selectHeight = 40f;
        private readonly float _deselectHeight = 35f;

        public void OnSelect(bool isSelected)
        {
            float ySize = isSelected ? _selectHeight : _deselectHeight;
            Vector3 size = new(rect.localScale.x, ySize, rect.localScale.z);
            
            background.color = isSelected ? selectedColor : deselectedColor;
            rect.DOSizeDelta(size, 0.15f);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (background != null)
            {
                background.color = deselectedColor;
            }
        }
#endif
    }
}