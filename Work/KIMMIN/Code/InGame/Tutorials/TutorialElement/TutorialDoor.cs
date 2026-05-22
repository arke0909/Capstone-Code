using System;
using Ami.BroAudio;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

namespace Work.Code.Tutorials
{
    public class TutorialDoor : MonoBehaviour
    {
        [SerializeField] private float tweenDuration;
        [SerializeField] private float targetScale;
        [SerializeField] private CinemachineCamera doorCamera;
        [SerializeField] private SoundID doorSound;
        
        private float _originalScale;

        private void Awake()
        {
            _originalScale = transform.localScale.x;
        }

        public void OpenDoor()
        {
            doorCamera.Priority = 100;
            gameObject.transform.DOKill();
            BroAudio.Play(doorSound);
            
            OpenDoorAfterDelay();
        }

        private async void OpenDoorAfterDelay()
        {
            await UniTask.WaitForSeconds(1.5f);
            gameObject.transform.DOScaleX(targetScale, tweenDuration)
                .OnComplete(() =>
                {
                    doorCamera.Priority = -1;
                });
        }

        public void CloseDoor()
        {
            transform.DOKill();
            transform.DOScaleX(_originalScale, tweenDuration);
        }
    }
}