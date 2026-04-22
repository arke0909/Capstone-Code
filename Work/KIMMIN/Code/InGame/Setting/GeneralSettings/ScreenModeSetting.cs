using System;
using UnityEngine;
using Work.Code.UI.Dropdown;

namespace Work.Code.Setting.GeneralSetting
{
    public class ScreenModeSetting : MonoBehaviour
    {
        [SerializeField] private DropdownUI dropdown;
        [SerializeField] private string[] options;

        private void Awake()
        {
            dropdown.UpdateDropdown(options, HandleSelect);
        }

        private void HandleSelect(int index, string option)
        {
            switch (index)
            {
                case 0:
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                    break;
                case 1:
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                    break;
            }
        }
    }
}