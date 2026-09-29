using System;
using Code.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Work.Code.Setting.KeySetting
{
    public class KeyMappingUI : UIBase
    {
        public delegate void KeyMappingChangeHandler(Key targetKey, Key newKey);
        public delegate void KeyMappingEditHandler(KeyMappingUI mappingUI);
        public delegate void KeyMappingEditEndHandler(KeyMappingUI mappingUI, string text);

        [SerializeField] private TMP_InputField keyInputField;
        [SerializeField] private Key targetKey;

        public Key TargetKey => targetKey;
        public Key CurrentKey { get; private set; }

        public event KeyMappingChangeHandler OnKeyChanged;
        public event KeyMappingEditHandler OnEditStarted;
        public event KeyMappingEditEndHandler OnTextChanged;
        public event KeyMappingEditEndHandler OnEditEnded;

        protected override void Awake()
        {
            base.Awake();
            SetCurrentKey(targetKey);
            keyInputField.placeholder.gameObject.SetActive(false);
            keyInputField.onSelect.AddListener(HandleEditStarted);
            keyInputField.onValueChanged.AddListener(HandleTextChanged);
            keyInputField.onEndEdit.AddListener(HandleEditEnded);
        }

        private void Start()
        {
            SetCurrentKey(CurrentKey);
        }

        private void HandleEditStarted(string text)
        {
            keyInputField.SetTextWithoutNotify(string.Empty);
            keyInputField.ForceLabelUpdate();
            OnEditStarted?.Invoke(this);
        }

        private void HandleEditEnded(string text)
        {
            OnEditEnded?.Invoke(this, text);
        }

        private void HandleTextChanged(string text)
        {
            string upperText = text.ToUpper();

            if (text != upperText)
            {
                keyInputField.SetTextWithoutNotify(upperText);
                keyInputField.ForceLabelUpdate();
            }

            OnTextChanged?.Invoke(this, upperText);
        }

        public void ApplyKey(Key key)
        {
            SetCurrentKey(key);
            OnKeyChanged?.Invoke(targetKey, key);
        }

        public void SetCurrentKey(Key key)
        {
            CurrentKey = key;
            SetDuplicate(false);
            keyInputField.SetTextWithoutNotify(GetKeyText(key));
            keyInputField.ForceLabelUpdate();
        }

        public void ResetKeyText()
        {
            SetDuplicate(false);
            keyInputField.SetTextWithoutNotify(GetKeyText(CurrentKey));
            keyInputField.ForceLabelUpdate();
        }

        public void SetDuplicate(bool isDuplicated)
        {
            keyInputField.textComponent.color = isDuplicated ? UIDefine.RedColor : Color.black;
        }

        public bool TryParseKey(string text, out Key key)
        {
            string keyName = text.Trim().ToUpper();

            if (keyName.Length != 1)
            {
                key = Key.None;
                return false;
            }

            if (char.IsDigit(keyName[0]))
                keyName = $"Digit{keyName}";

            return Enum.TryParse(keyName, true, out key);
        }

        private string GetKeyText(Key key)
        {
            string keyName = key.ToString();

            if (keyName.StartsWith("Digit"))
                return keyName.Replace("Digit", string.Empty);

            return keyName;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            keyInputField.onSelect.RemoveListener(HandleEditStarted);
            keyInputField.onValueChanged.RemoveListener(HandleTextChanged);
            keyInputField.onEndEdit.RemoveListener(HandleEditEnded);
        }
    }
}
