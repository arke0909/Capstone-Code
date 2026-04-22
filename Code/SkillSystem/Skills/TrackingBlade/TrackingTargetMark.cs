using System;
using UnityEngine;
using UnityEngine.UI;

namespace Code.SkillSystem.Skills.TrackingBlade
{
    public class TrackingTargetMark : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        
        private Transform _targetTrm;
        private float _currentTime;
        private float _chargeDuration;
        private bool _isTargeting;
        
        public void SetTarget(Transform targetTrm, float chargeDuration)
        {
            _targetTrm = targetTrm;
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
            transform.position = _targetTrm.position;
        }

        public void CancelCharge()
        {
            _isTargeting = false;
            
            gameObject.SetActive(false);
        }
    }
}