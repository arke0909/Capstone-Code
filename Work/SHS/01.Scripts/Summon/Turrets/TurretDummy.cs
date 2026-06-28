using System;
using Ami.BroAudio;
using Scripts.Entities;
using SHS.Scripts.Summon;
using UnityEngine;

namespace SHS.Scripts.Summon.Turrets
{
    public class TurretDummy : Entity, ISummonable
    {
        [SerializeField] private SoundID turretInstallSound;
        
        [Header("Throw Settings")]
        [SerializeField] private GameObject turretPrefab;
        [SerializeField] private float throwHeight = 3f;
        [SerializeField] private float throwDuration = 1.2f;
        [SerializeField] private Animator animator;
        [SerializeField] private EntityAnimatorTrigger animatorTrigger;

        [Header("Rotation Settings")]
        [SerializeField] private float spinSpeed = 360f;
        [SerializeField] private Vector3 spinAxis = Vector3.right;

        private Vector3 _startPoint;
        private Vector3 _targetPoint;
        private float _elapsedTime;
        private bool _isFlying;
        private EngineerTurretTracker _turretTracker;

        private void Start()
            => animatorTrigger.OnAnimationEndTrigger += SpawnTurret;

        private void OnDestroy()
            => animatorTrigger.OnAnimationEndTrigger -= SpawnTurret;

        public void Throw(Vector3 targetPoint)
        {
            _startPoint = transform.position;
            _targetPoint = targetPoint;
            _elapsedTime = 0f;
            _isFlying = true;
        }

        public void SetTracker(EngineerTurretTracker turretTracker)
            => _turretTracker = turretTracker;

        private void Update()
        {
            if (!_isFlying) return;

            _elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsedTime / throwDuration);

            Vector3 currentPos = EvaluateParabola(t);
            Vector3 nextPos = EvaluateParabola(Mathf.Clamp01(t + 0.01f));
            Vector3 moveDirection = (nextPos - currentPos).normalized;

            if (moveDirection != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(moveDirection)
                                   * Quaternion.Euler(spinAxis * (spinSpeed * _elapsedTime));

            transform.position = currentPos;

            if (t >= 1f) OnImpact();
        }

        private Vector3 EvaluateParabola(float t)
        {
            Vector3 horizontalPos = Vector3.Lerp(_startPoint, _targetPoint, t);
            float heightOffset = Mathf.Sin(t * Mathf.PI) * throwHeight;
            return horizontalPos + Vector3.up * heightOffset;
        }

        private void OnImpact()
        {
            _isFlying = false;
            transform.position = _targetPoint;
            animator.enabled = true;
        }

        private void SpawnTurret()
        {
            GameObject turret = Instantiate(turretPrefab, transform.position, transform.rotation);
            BroAudio.Play(turretInstallSound, transform.position);
            if (_turretTracker != null)
            {
                _turretTracker.Unregister(gameObject);
                _turretTracker.Register(turret);
            }

            Destroy(gameObject);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;

            Gizmos.color = Color.yellow;
            int segments = 30;
            Vector3 prev = _startPoint;

            for (int i = 1; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector3 next = EvaluateParabola(t);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_targetPoint, 0.2f);
        }
#endif
    }
}
