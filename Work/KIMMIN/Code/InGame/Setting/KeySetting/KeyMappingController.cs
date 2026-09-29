using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Work.Code.Setting.KeySetting
{
    public class KeyMappingController : MonoBehaviour
    {
        public delegate void KeyMappingChangeHandler(Key targetKey, Key newKey);

        [SerializeField] private PlayerInputSO playerInput;

        private readonly Dictionary<Key, Key> _keyMap = new();
        private readonly Dictionary<KeyMappingUI, KeyMappingUI.KeyMappingChangeHandler> _changeHandlers = new();
        private readonly Dictionary<KeyMappingUI, KeyMappingUI.KeyMappingEditHandler> _editStartHandlers = new();
        private readonly Dictionary<KeyMappingUI, KeyMappingUI.KeyMappingEditEndHandler> _textChangeHandlers = new();
        private readonly Dictionary<KeyMappingUI, KeyMappingUI.KeyMappingEditEndHandler> _editEndHandlers = new();

        private bool _prevInputActive;

        public event KeyMappingChangeHandler OnKeyMapped;

        private void Awake()
        {
            foreach (KeyMappingUI mappingUI in GetComponentsInChildren<KeyMappingUI>(true))
            {
                AddMappingUI(mappingUI);
            }
        }

        private void AddMappingUI(KeyMappingUI mappingUI)
        {
            mappingUI.SetCurrentKey(playerInput.GetMappedKey(mappingUI.TargetKey));
            _keyMap[mappingUI.TargetKey] = mappingUI.CurrentKey;

            KeyMappingUI.KeyMappingChangeHandler changeHandler = (targetKey, newKey) =>
            {
                HandleKeyChanged(mappingUI, targetKey, newKey);
            };

            KeyMappingUI.KeyMappingEditHandler editStartHandler = HandleEditStarted;
            KeyMappingUI.KeyMappingEditEndHandler textChangeHandler = HandleTextChanged;
            KeyMappingUI.KeyMappingEditEndHandler editEndHandler = HandleEditEnded;

            _changeHandlers.Add(mappingUI, changeHandler);
            _editStartHandlers.Add(mappingUI, editStartHandler);
            _textChangeHandlers.Add(mappingUI, textChangeHandler);
            _editEndHandlers.Add(mappingUI, editEndHandler);

            mappingUI.OnKeyChanged += changeHandler;
            mappingUI.OnEditStarted += editStartHandler;
            mappingUI.OnTextChanged += textChangeHandler;
            mappingUI.OnEditEnded += editEndHandler;
        }

        private void HandleEditStarted(KeyMappingUI mappingUI)
        {
            _prevInputActive = playerInput.IsPlayerInputActive;
            playerInput.SetActive(false);
        }

        private void HandleTextChanged(KeyMappingUI mappingUI, string text)
        {
            mappingUI.SetDuplicate(!mappingUI.TryParseKey(text, out Key newKey) || IsDuplicatedKey(mappingUI.TargetKey, newKey));
        }

        private void HandleEditEnded(KeyMappingUI mappingUI, string text)
        {
            playerInput.SetPlayerInput(_prevInputActive);

            if (!mappingUI.TryParseKey(text, out Key newKey) || IsDuplicatedKey(mappingUI.TargetKey, newKey))
            {
                mappingUI.ResetKeyText();
                return;
            }

            mappingUI.ApplyKey(newKey);
        }

        private void HandleKeyChanged(KeyMappingUI mappingUI, Key targetKey, Key newKey)
        {
            _keyMap[targetKey] = newKey;
            playerInput.ApplyKeyMapping(targetKey, newKey);
            OnKeyMapped?.Invoke(targetKey, newKey);
        }

        private bool IsDuplicatedKey(Key targetKey, Key newKey)
        {
            foreach (var pair in _keyMap)
            {
                if (pair.Key != targetKey && pair.Value == newKey)
                    return true;
            }

            return playerInput.IsKeyUsed(targetKey, newKey);
        }

        private void OnDestroy()
        {
            foreach (var pair in _changeHandlers)
            {
                pair.Key.OnKeyChanged -= pair.Value;
            }

            foreach (var pair in _editStartHandlers)
            {
                pair.Key.OnEditStarted -= pair.Value;
            }

            foreach (var pair in _textChangeHandlers)
            {
                pair.Key.OnTextChanged -= pair.Value;
            }

            foreach (var pair in _editEndHandlers)
            {
                pair.Key.OnEditEnded -= pair.Value;
            }
        }
    }
}
