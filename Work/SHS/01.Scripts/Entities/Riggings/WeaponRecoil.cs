using Chipmunk.Library.Utility.GameEvents.Local;
using Scripts.Combat.Datas;
using SHS.Scripts.Combats.Events;
using UnityEngine;

namespace SHS.Scripts.Entities.Rigings
{
    [DefaultExecutionOrder(-100)]
    public class WeaponRecoil : MonoBehaviour, ILocalEventSubscriber<GunAttackEvent>
    {
        [Header("Recoil Scale")]
        [SerializeField] private float backwardDistancePerRecoilUnit = 0.0008f;
        [SerializeField] private float xRotationDegreesPerRecoilUnit = 0.04f;
        [SerializeField] private float yRotationDegreesPerRecoilUnit = 0.015f;
        [SerializeField] private float maxBackwardDistance = 0.08f;
        [SerializeField] private float maxXRotation = 8f;
        [SerializeField] private float maxYRotation = 3f;

        private GunDataSO _recoilData;
        private Vector3 _initialLocalPosition;
        private Quaternion _initialLocalRotation;
        private float _targetBackwardDistance;
        private float _targetXRotation;
        private float _targetYRotation;
        private float _backwardDistance;
        private float _xRotation;
        private float _yRotation;
        private float _backwardRecoveryVelocity;
        private float _xRotationRecoveryVelocity;
        private float _yRotationRecoveryVelocity;
        private float _lastShotTime = -999f;

        private void Awake()
        {
            _initialLocalPosition = transform.localPosition;
            _initialLocalRotation = transform.localRotation;
        }

        public void OnLocalEvent(GunAttackEvent eventData)
        {
            _recoilData = eventData.GunData;
            _lastShotTime = Time.time;
            _backwardRecoveryVelocity = 0f;
            _xRotationRecoveryVelocity = 0f;
            _yRotationRecoveryVelocity = 0f;

            float verticalKick = eventData.VerticalRecoil;
            float horizontalKick = eventData.HorizontalRecoil;

            _targetBackwardDistance = Mathf.Min(
                _targetBackwardDistance + verticalKick * backwardDistancePerRecoilUnit,
                maxBackwardDistance);
            _targetXRotation = Mathf.Min(
                _targetXRotation + verticalKick * xRotationDegreesPerRecoilUnit,
                maxXRotation);
            _targetYRotation = Mathf.Clamp(
                _targetYRotation + horizontalKick * yRotationDegreesPerRecoilUnit,
                -maxYRotation,
                maxYRotation);
        }

        private void Update()
        {
            RecoverTargetRecoil();

            float followTime = Mathf.Max(0.001f, _recoilData != null ? _recoilData.recoilDuration : 0.05f);
            float followAlpha = 1f - Mathf.Exp(-Time.deltaTime / followTime);
            _backwardDistance = Mathf.Lerp(_backwardDistance, _targetBackwardDistance, followAlpha);
            _xRotation = Mathf.Lerp(_xRotation, _targetXRotation, followAlpha);
            _yRotation = Mathf.Lerp(_yRotation, _targetYRotation, followAlpha);

            transform.localPosition = _initialLocalPosition + Vector3.back * _backwardDistance;
            transform.localRotation = _initialLocalRotation * Quaternion.Euler(-_xRotation, _yRotation, 0f);
        }

        private void RecoverTargetRecoil()
        {
            if (_recoilData == null || Time.time - _lastShotTime < _recoilData.recoilRecoveryStartTime)
                return;

            float recoveryTime = Mathf.Max(0.001f, _recoilData.recoilRecoveryTime);

            _targetBackwardDistance = Mathf.SmoothDamp(
                _targetBackwardDistance,
                0f,
                ref _backwardRecoveryVelocity,
                recoveryTime,
                _recoilData.recoilRecovery * backwardDistancePerRecoilUnit,
                Time.deltaTime);

            _targetXRotation = Mathf.SmoothDamp(
                _targetXRotation,
                0f,
                ref _xRotationRecoveryVelocity,
                recoveryTime,
                _recoilData.recoilRecovery * xRotationDegreesPerRecoilUnit,
                Time.deltaTime);

            _targetYRotation = Mathf.SmoothDamp(
                _targetYRotation,
                0f,
                ref _yRotationRecoveryVelocity,
                recoveryTime,
                _recoilData.recoilRecovery * yRotationDegreesPerRecoilUnit,
                Time.deltaTime);
        }
    }
}
