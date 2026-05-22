using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Serialization;

namespace SHS.Scripts.Entities.Rigings
{
    public class HeadAimRiggingController : MonoBehaviour
    {
        [SerializeField] private Transform characterRoot;
        [SerializeField] private Transform headTrm;
        [SerializeField] private Transform aimTargetTrm;

        [SerializeField] float maxRotation = 75f;
        [SerializeField] float fadeStartRotation = 55f;
        [SerializeField] float smooth = 8f;

        private MultiAimConstraint _headAimConstraint;
        private float _currentWeight;

        private void Awake()
        {
            _headAimConstraint = GetComponent<MultiAimConstraint>();
        }

        void LateUpdate()
        {
            Vector3 up = characterRoot.up;
            Vector3 dir = aimTargetTrm.position - headTrm.position;
            dir = Vector3.ProjectOnPlane(dir, up);

            if (dir.sqrMagnitude < 0.001f)
                return;

            float Rotation = Vector3.SignedAngle(characterRoot.forward, dir.normalized, up);
            float absRotation = Mathf.Abs(Rotation);

            float targetWeight = 1f;

            if (absRotation > fadeStartRotation)
            {
                targetWeight = Mathf.InverseLerp(maxRotation, fadeStartRotation, absRotation);
            }

            if (absRotation > maxRotation)
            {
                targetWeight = 0f;
            }

            _currentWeight = Mathf.Lerp(_currentWeight, targetWeight, Time.deltaTime * smooth);
            _headAimConstraint.weight = _currentWeight;
        }
    }
}