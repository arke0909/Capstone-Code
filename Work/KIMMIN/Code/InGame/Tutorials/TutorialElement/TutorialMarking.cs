using System;
using UnityEngine;
using Work.Code.Core.Extension;

namespace Work.Code.Tutorials
{
    public class TutorialMarking : MonoBehaviour
    {
        [SerializeField] private LayerMask whatIsTarget;
        [SerializeField] private Vector3 castSize;
        [SerializeField] private Vector3 castOffset;
        [SerializeField] private bool activeOnAwake;
        
        [SerializeField] private GameObject visual;

        private bool _isEnable;
        private Collider[] _buffer = new Collider[1];
        
        public event Action OnDetectTarget;

        private void Awake()
        {
            SetEnable(activeOnAwake);
        }

        private void Update()
        {
            if (_isEnable)
                CheckTarget();
        }

        public void CheckTarget()
        {
            int count = Physics.OverlapBoxNonAlloc(transform.position + castOffset, 
                castSize, _buffer, Quaternion.identity, whatIsTarget);
            
            if (count > 0)
            {
                OnDetectTarget?.Invoke();
                SetEnable(false);
            }
        }   

        public void SetEnable(bool isEnable)
        {
            _isEnable = isEnable;
            SetVisual(isEnable);
        }

        public void SetVisual(bool isEnable)
        {
            visual.SetActive(isEnable);
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + castOffset, castSize);
        }
#endif
    }
}