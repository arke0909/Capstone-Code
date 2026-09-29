using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Work.Code.Setting.KeySetting
{
    public class MappedKeyTextUI : MonoBehaviour
    {
        [SerializeField] private PlayerInputSO playerInput;
        [SerializeField] private TMP_Text keyText;
        [SerializeField] private Key targetKey;
        [SerializeField] private string prefix;
        [SerializeField] private string suffix;

        private void Awake()
        {
            RefreshKeyText();
            playerInput.OnKeyMapped += HandleKeyMapped;
        }

        private void OnEnable()
        {
            RefreshKeyText();
        }

        private void OnDestroy()
        {
            playerInput.OnKeyMapped -= HandleKeyMapped;
        }

        private void HandleKeyMapped(Key targetKey, Key newKey)
        {
            if (this.targetKey != targetKey)
                return;

            RefreshKeyText();
        }

        public void SetTargetKey(Key key)
        {
            targetKey = key;
            RefreshKeyText();
        }

        public void RefreshKeyText()
        {
            keyText.text = $"{prefix}{playerInput.GetKeyText(targetKey)}{suffix}";
        }
    }
}
