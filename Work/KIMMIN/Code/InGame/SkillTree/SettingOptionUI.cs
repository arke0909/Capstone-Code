using System;
using UnityEngine;
using UnityEngine.UI;

namespace Work.Code.SkillTree
{
    public class SettingOptionUI : MonoBehaviour
    {
        private SettingOptionButton[] _settingOptions;
        private SettingOptionButton _option;
        
        public event Action<SettingType> OnSelectOption;

        private void Awake()
        {
            _settingOptions = GetComponentsInChildren<SettingOptionButton>();

            foreach (var option in _settingOptions)
            {
                option.OnSelect += HandleSelectOption;
                option.DisableOption();
            }
            
            HandleSelectOption(_settingOptions[0]);
        }

        private void HandleSelectOption(SettingOptionButton option)
        {
            if(_option == option) return;
            if(_option != null) _option.DisableOption();
            
            OnSelectOption?.Invoke(option.Type);
            _option = option;
            _option.EnableOption();
        }
    }
}