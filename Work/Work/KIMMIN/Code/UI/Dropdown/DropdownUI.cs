using System;
using System.Collections.Generic;
using Code.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Work.Code.UI.Dropdown
{
    public class DropdownUI : MonoBehaviour
    {
        [SerializeField] private Button toggleButton;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private DropdownTemplate template;
        [SerializeField] private GameObject content;
        [SerializeField] private Canvas canvas;
        
        [SerializeField] private Image arrowImage;
        [SerializeField] private Sprite upArrow;
        [SerializeField] private Sprite downArrow;

        
        private string[] _options;
        private Action<int, string> _onValueChanged;
        private List<DropdownTemplate> _templates = new();

        private void Awake()
        {
            toggleButton.onClick.AddListener(HandleToggleDropdown);
        }

        private void OnDestroy()
        {
            toggleButton.onClick.RemoveListener(HandleToggleDropdown);
        }

        private void HandleToggleDropdown()
        {
            if (content == null) return;
            
            content.SetActive(!content.activeSelf);
            
            if (content.activeSelf)
            {
                canvas.sortingOrder = 50;
            }
        }
    
        public void UpdateDropdown(string[] options, Action<int, string> onValueChanged = null)
        {
            _options = options;
            _onValueChanged = onValueChanged;
            DisableDropdown();

            for (int i = 0; i < _templates.Count || i < options.Length; i++)
            {
                if (i >= _templates.Count)
                {
                    var newTemplate = Instantiate(template, content.transform);
                    _templates.Add(newTemplate);
                }

                DropdownTemplate currentTemplate = _templates[i];

                if (i < options.Length)
                {
                    int index = i;
                    string option = _options[index];

                    currentTemplate.EnableFor(option);
                    currentTemplate.Button.onClick.RemoveAllListeners();
                    currentTemplate.Button.onClick.AddListener(() => Select(index, option));
                }
                else
                {
                    currentTemplate.Disable();
                }
            }
        }

        private void Select(int index, string value)
        {
            title.text = value;
            canvas.sortingOrder = 0;
            _onValueChanged?.Invoke(index, value);
            DisableDropdown();
        }

        public void DisableDropdown()
        {
            content?.SetActive(false);
        }
    }
}