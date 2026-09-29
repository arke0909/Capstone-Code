using System;
using TMPro;
using UnityEngine;

namespace Code.UI.Popup
{
    public struct RoomEnterFailedData { }
    
    public class RoomEnterFailedPopup : BasePopup<RoomEnterFailedData, EmptyCallback>
    {
        [SerializeField] private TextMeshProUGUI failText;

        protected override void ShowPopup(RoomEnterFailedData data, EmptyCallback callback)
        {
            EnableUI();
            DisableWithDelay();
        }

        private async void DisableWithDelay()
        {
            await Awaitable.WaitForSecondsAsync(3f);
            DisableUI(true);
        }
    }
}