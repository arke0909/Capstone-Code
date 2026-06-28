using System;
using UnityEngine;
using UnityEngine.UI;

namespace Code.SkillSystem.Skills.TrackingBlade
{
    public class TrackingTargetMark : MonoBehaviour
    {
        [SerializeField] private Image fillImage;

        private Camera _cam;
        private Transform _targetTrm;
        private Vector3 _offset;
        private float _currentTime;
        private float _chargeDuration;
        private bool _isTargeting;

        private void Awake()
        {
            _cam = Camera.main;
            
            CancelCharge();
        }

        public void SetTarget(Transform targetTrm, float chargeDuration)
        {
            _targetTrm = targetTrm; 
            _offset = -_cam.transform.forward * 1.5f;
            _offset.y += 1f;
            
            _isTargeting = true;
            _chargeDuration = chargeDuration;
            _currentTime = 0;
            
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (_isTargeting)
            {
                _currentTime += Time.deltaTime;

                if (_chargeDuration >= _currentTime)
                {
                    fillImage.fillAmount = _currentTime / _chargeDuration;
                }
            }
        }

        private void LateUpdate()
        {
            transform.position = _targetTrm.position + _offset;
        }

        public void CancelCharge()
        {
            _isTargeting = false;
            
            gameObject.SetActive(false);
        }
    }
}