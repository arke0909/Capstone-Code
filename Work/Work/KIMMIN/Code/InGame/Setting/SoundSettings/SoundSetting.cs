using System;
using Ami.BroAudio;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Work.Code.Setting.SoundSettings
{
    public class SoundSetting : MonoBehaviour
    {
        [SerializeField] private Scrollbar masterVolumeSlider;
        [SerializeField] private Scrollbar sfxVolumeSlider;
        [SerializeField] private Scrollbar bgmVolumeSlider;

        [SerializeField] private TextMeshProUGUI masterText;
        [SerializeField] private TextMeshProUGUI sfxText;
        [SerializeField] private TextMeshProUGUI bgmText;

        private void Awake()
        {
            masterVolumeSlider.onValueChanged.AddListener(HandleMasterVolumeChanged);
            sfxVolumeSlider.onValueChanged.AddListener(HandleSFXVolumeChanged);
            bgmVolumeSlider.onValueChanged.AddListener(HandleBGMVolumeChanged);

            masterVolumeSlider.value = 1f;
            sfxVolumeSlider.value = 1f;
            bgmVolumeSlider.value = 1f;
        }

        private void HandleMasterVolumeChanged(float value)
        {
            Debug.Log(Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f);
            BroAudio.SetVolume(BroAudioType.All, value);
            masterText.text = ((int)(value * 100)).ToString();
        }

        private void HandleSFXVolumeChanged(float value)
        {
            BroAudio.SetVolume(BroAudioType.SFX, value);
            sfxText.text = ((int)(value * 100)).ToString();
        }

        private void HandleBGMVolumeChanged(float value)
        {
            BroAudio.SetVolume(BroAudioType.Music, value);
            bgmText.text = ((int)(value * 100)).ToString();
        }
    }
}