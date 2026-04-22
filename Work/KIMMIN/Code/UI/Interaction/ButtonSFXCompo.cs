using Ami.BroAudio;
using UnityEngine;
using UnityEngine.UI;

namespace Work.Code.UI.Interaction
{
    [RequireComponent(typeof(Button))]
    public class ButtonSFXCompo : MonoBehaviour
    {
        [SerializeField] private SoundID clickSoundID;
        private Button _button;
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleClick);
        }

        private void HandleClick()
        {
            BroAudio.Play(clickSoundID);
        }

        private void OnDestroy()
        {
            _button.onClick.AddListener(HandleClick);
        }
    }
}