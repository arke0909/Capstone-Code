using System;
using Ami.BroAudio;
using TMPro;
using UnityEngine;
using Work.Code.UI.ScrollBar;

namespace Work.Code.Setting.SoundSettings
{
    public class SoundSetting : MonoBehaviour
    {
        [SerializeField] private BaseScrollbar masterVolumeSlider;
        [SerializeField] private BaseScrollbar sfxVolumeSlider;
        [SerializeField] private BaseScrollbar bgmVolumeSlider;

        private void Awake()
        {
            masterVolumeSlider.OnValueChanged += (HandleMasterVolumeChanged);
            sfxVolumeSlider.OnValueChanged += (HandleSFXVolumeChanged);
            bgmVolumeSlider.OnValueChanged += (HandleBGMVolumeChanged);
        }

        private void OnDestroy()
        {
            masterVolumeSlider.OnValueChanged -= (HandleMasterVolumeChanged);
            sfxVolumeSlider.OnValueChanged -= (HandleSFXVolumeChanged);
            bgmVolumeSlider.OnValueChanged -= (HandleBGMVolumeChanged);
        }

        private void HandleMasterVolumeChanged(float value)
        {
            BroAudio.SetVolume(BroAudioType.All, value);
        }

        private void HandleSFXVolumeChanged(float value)
        {
            BroAudio.SetVolume(BroAudioType.SFX, value);
        }

        private void HandleBGMVolumeChanged(float value)
        {
            BroAudio.SetVolume(BroAudioType.Music, value);
        }
    }
}