using Ami.BroAudio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Work.Code.UI.Interaction
{
    [RequireComponent(typeof(Button))]
    public class ButtonSFXCompo : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] private SoundID clickSoundID;
        [SerializeField] private SoundID hoveringSoundID;
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
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            BroAudio.Play(hoveringSoundID);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(HandleClick);
        }
    }
}