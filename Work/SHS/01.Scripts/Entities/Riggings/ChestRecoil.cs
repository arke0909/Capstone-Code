using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Scripts.Combat.Datas;
using SHS.Scripts.Combats.Events;
using SHS.Scripts.Entities.Players;
using UnityEngine;

namespace SHS.Scripts.Entities.Rigings
{
    [DefaultExecutionOrder(-50)]
    public class ChestRecoil : MonoBehaviour, IContainerComponent, ILocalEventSubscriber<GunAttackEvent>
    {
        [SerializeField] private Transform aimOrigin;
        [SerializeField] private float targetDistance = 10f;

        [Header("Recoil Scale")]
        [SerializeField] private float xRotationDegreesPerRecoilUnit = 0.025f;
        [SerializeField] private float yRotationDegreesPerRecoilUnit = 0.0125f;
        [SerializeField] private float maxXRotation = 5f;
        [SerializeField] private float maxYRotation = 2f;

        private AimTransform _aimTarget;
        private GunDataSO _recoilData;
        private float _targetXRotation;
        private float _targetYRotation;
        private float _xRotation;
        private float _yRotation;
        private float _xRotationRecoveryVelocity;
        private float _yRotationRecoveryVelocity;
        private float _lastShotTime = -999f;

        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            ComponentContainer = componentContainer;
            _aimTarget = componentContainer.Get<AimTransform>();
        }

        private void Awake()
        {
            Debug.Assert(aimOrigin != null, "AimOrigin is not assigned!");
            Debug.Assert(targetDistance > 0f, "TargetDistance must be greater than zero!");
        }

        public void OnLocalEvent(GunAttackEvent eventData)
        {
            _recoilData = eventData.GunData;
            _lastShotTime = Time.time;
            _xRotationRecoveryVelocity = 0f;
            _yRotationRecoveryVelocity = 0f;

            float xRotationKick = eventData.VerticalRecoil * xRotationDegreesPerRecoilUnit;
            float yRotationKick = eventData.HorizontalRecoil * yRotationDegreesPerRecoilUnit;

            _targetXRotation = Mathf.Clamp(_targetXRotation + xRotationKick, 0f, maxXRotation);
            _targetYRotation = Mathf.Clamp(_targetYRotation + yRotationKick, -maxYRotation, maxYRotation);
        }

        private void Update()
        {
            RecoverTargetRecoil();

            float followTime = Mathf.Max(0.001f, _recoilData != null ? _recoilData.recoilDuration : 0.05f);
            float followAlpha = 1f - Mathf.Exp(-Time.deltaTime / followTime);
            _xRotation = Mathf.Lerp(_xRotation, _targetXRotation, followAlpha);
            _yRotation = Mathf.Lerp(_yRotation, _targetYRotation, followAlpha);

            Vector3 aimDirection = _aimTarget.transform.position - aimOrigin.position;
            if (aimDirection.sqrMagnitude < 0.001f)
                aimDirection = aimOrigin.forward;

            Quaternion recoilRotation =
                Quaternion.AngleAxis(_yRotation, aimOrigin.up) *
                Quaternion.AngleAxis(-_xRotation, aimOrigin.right);

            transform.position = aimOrigin.position + recoilRotation * aimDirection.normalized * targetDistance;
        }

        private void RecoverTargetRecoil()
        {
            if (_recoilData == null || Time.time - _lastShotTime < _recoilData.recoilRecoveryStartTime)
                return;

            float recoveryTime = Mathf.Max(0.001f, _recoilData.recoilRecoveryTime);
            float xRotationRecoverySpeed = _recoilData.recoilRecovery * xRotationDegreesPerRecoilUnit;
            float yRotationRecoverySpeed = _recoilData.recoilRecovery * yRotationDegreesPerRecoilUnit;

            _targetXRotation = Mathf.SmoothDamp(
                _targetXRotation,
                0f,
                ref _xRotationRecoveryVelocity,
                recoveryTime,
                xRotationRecoverySpeed,
                Time.deltaTime);

            _targetYRotation = Mathf.SmoothDamp(
                _targetYRotation,
                0f,
                ref _yRotationRecoveryVelocity,
                recoveryTime,
                yRotationRecoverySpeed,
                Time.deltaTime);
        }
    }
}
