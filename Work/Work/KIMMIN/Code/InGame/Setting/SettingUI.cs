using System.Collections.Generic;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.UI.Core;
using UnityEngine;
using Work.Code.GameEvents;
using Work.Code.SkillTree;

namespace Work.Code.Setting
{
    public class SettingUI : UIPanel
    {
        [SerializeField] private SettingOptionUI settingOptionUI;
        [SerializeField] private GameObject[] settings;
        [SerializeField] private GameObject canvas;
        [SerializeField] private PlayerInputSO playerInput;

        private Dictionary<SettingType, GameObject> _settingTypeDict = new();
        private SettingType _type = SettingType.Key;

        protected override void Awake()
        {
            base.Awake();
            
            for(int i = 0 ; i < settings.Length ; i++)
            {
                _settingTypeDict.Add((SettingType)i, settings[i]);
                settings[i].SetActive(false);
            }
            
            HandleSelectOptionSetting(SettingType.General);
            settingOptionUI.OnSelectOption += HandleSelectOptionSetting;
            EventBus.Subscribe<PressESCEvent>(HandleToggleUI);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            settingOptionUI.OnSelectOption -= HandleSelectOptionSetting;
            EventBus.Unsubscribe<PressESCEvent>(HandleToggleUI);
        }
        
        private void HandleToggleUI(PressESCEvent evt)
        {
            ToggleUI(true);
        }

        private void HandleSelectOptionSetting(SettingType type)
        {
            if (_type == type) return;
            _settingTypeDict[_type]?.SetActive(false);
            _type = type;
            _settingTypeDict[type]?.SetActive(true);
        }
    }
}