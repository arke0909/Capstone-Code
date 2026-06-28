using Ami.BroAudio;
using DG.Tweening;
using Scripts.Entities;
using Scripts.GameSystem;
using UnityEngine;

namespace Code.Structures
{
    public class DoorStructure : InteractableStructure
    {
        [SerializeField] private SoundID openDoorSound;
        [SerializeField] private SoundID closeDoorSound;
        
        [Header("Door")]
        [SerializeField] private Transform doorPivot;
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float duration = 0.4f;

        private bool _isOpen;
        private Quaternion _closedRot;

        protected override void Awake()
        {
            base.Awake();
            _closedRot = doorPivot.localRotation;
        }

        public override void Interact(Entity interactor)
        {
            if (_isOpen)
            {
                Close();
                return;
            }

            Vector3 toInteractor = interactor.transform.position - transform.position;
            float side = Vector3.Dot(transform.forward, toInteractor);
            float angle = side >= 0f ? -openAngle : openAngle;

            Quaternion openRot = Quaternion.Euler(doorPivot.localEulerAngles + Vector3.up * angle);
            doorPivot.DOLocalRotateQuaternion(openRot, duration).SetEase(Ease.OutCubic);
            _isOpen = true;
            BroAudio.Play(openDoorSound);
            DeSelect();
        }

        private void Close()
        {
            doorPivot.DOLocalRotateQuaternion(_closedRot, duration).SetEase(Ease.OutCubic);
            _isOpen = false;
            BroAudio.Play(closeDoorSound);
            DeSelect();
        }
    }
}
