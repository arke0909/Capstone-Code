using Ami.BroAudio;
using Code.UI.Core;
using UnityEngine;

namespace Code.UI
{
    public class UIToggleSoundPlayer : MonoBehaviour
    {
        [SerializeField] private UIPanel panel;
        [SerializeField] private SoundID openSoundID;
        [SerializeField] private SoundID closeSoundID;

        private void Start()
        {
            panel.OnToggleUI += SoundPlayOnToggle;
        }

        private void OnDestroy()
        {
            panel.OnToggleUI -= SoundPlayOnToggle;
        }

        private void SoundPlayOnToggle(UIBase uiBase, bool isActive)
        {
            BroAudio.Play(uiBase.IsActive ? openSoundID : closeSoundID);
        }
    }
}