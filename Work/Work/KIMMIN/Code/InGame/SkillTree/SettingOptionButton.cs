using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Work.Code.SkillTree
{
    public enum CharacterType
    {
        Rusher,
        Scout,
        Assassin,
        Controller,
        Machinist
    }

    public enum SettingType
    {
        General,
        Sound,
        Key,
        Misc,
        None
    }
    
    public class SettingOptionButton : MonoBehaviour
    {
        [field: SerializeField] public SettingType Type { get; set; } 
        [SerializeField] private TextMeshProUGUI optionText;
        [SerializeField] private Image icon;
        [SerializeField] private Image outline;
        [SerializeField] private Image background;
        [SerializeField] private Sprite activeOutline;
        [SerializeField] private Sprite inActiveOutline;
        
        private RectTransform Rect => transform as RectTransform;
        
        private Button _button;
        private Tween _sizeTween;
        private Color32 _activeColor = new Color32(40, 50, 80, 255);
        private Color32 _inActiveColor = new Color32(30, 40, 60, 255);

        private string[] _settingName =
        {
            "일반 설정",
            "소리 설정",
            "키 설정",
            "기타 설정"
        };
        
        public event Action<SettingOptionButton> OnSelect;
        public int Index => transform.GetSiblingIndex();
        

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() => OnSelect?.Invoke(this));
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }

        public void EnableOption()
        {
            background.color = _activeColor;
            outline.sprite = activeOutline;
            icon.DOColor(Color.white, 0.1f).SetUpdate(true);
            optionText.DOColor(Color.white, 0.1f).SetUpdate(true);
            ChangeHeight(50f);
        }

        public void DisableOption()
        {
            background.color = _inActiveColor;
            outline.sprite = inActiveOutline;
            icon.DOColor(Color.grey, 0.1f).SetUpdate(true);
            optionText.DOColor(Color.grey, 0.1f).SetUpdate(true);
            ChangeHeight(45f);
        }
        
        private void ChangeHeight(float targetHeight)
        {
            _sizeTween?.Kill();

            Vector2 targetSize = new Vector2(Rect.sizeDelta.x, targetHeight);
            _sizeTween = Rect.DOSizeDelta(targetSize, 0.2f)
                .SetEase(Ease.OutCubic).SetUpdate(true);
        }

        private void OnValidate()
        {
            name = $"{_settingName[(int)Type]}버튼";
            if(optionText != null)
                optionText.text = _settingName[(int)Type];
        }
    }
}