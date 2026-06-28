using System;
using Ami.BroAudio;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using Work.Code.PlayerTasks;

namespace Work.Code.Tutorials
{
    public class TutorialDoor : TaskCompleteInteraction
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
        
        
        public override void Interact()
        {
            gameObject.transform.DOKill();
            SetCameraPriority(100);
            OpenDoorAfterDelay();
        }
        
        private async void OpenDoorAfterDelay()
        {
            await UniTask.WaitForSeconds(1.5f);
            
            BroAudio.Play(doorSound);
            gameObject.transform.DOScaleX(targetScale, tweenDuration)
                .OnComplete(() =>
                {
                    SetCameraPriority(-1);
                });
        }

        public void CloseDoor()
        {
            transform.DOKill();
            transform.DOScaleX(_originalScale, tweenDuration);
        }

        private void SetCameraPriority(int priority)
        {
            if(doorCamera != null)
                doorCamera.Priority = priority;
        }
    }
}