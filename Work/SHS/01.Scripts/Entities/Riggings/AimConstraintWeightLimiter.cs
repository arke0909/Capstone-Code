using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace SHS.Scripts.Entities.Rigings
{
    public class AimConstraintWeightLimiter : MonoBehaviour
    {
        [Serializable]
        private class AimConstraintLimit
        {
            public MultiAimConstraint constraint;
            public float fadeStartRotation = 55f;
            public float maxRotation = 75f;
            public float maxWeight = 1f;
        }

        [SerializeField] private Transform characterRoot;
        [SerializeField] private Transform angleOrigin;
        [SerializeField] private Transform aimTargetTrm;
        [SerializeField] private List<AimConstraintLimit> aimConstraints = new List<AimConstraintLimit>();
        [SerializeField] private float minAimDistance = 0.8f;
        [SerializeField] private float distanceFadeRange = 0.8f;
        [SerializeField] private float smooth = 8f;

        private readonly List<float> _currentWeights = new List<float>();

        private void Awake()
        {
            SyncWeightCache();
        }

        private void LateUpdate()
        {
            SyncWeightCache();

            Vector3 up = characterRoot.up;
            Vector3 dir = aimTargetTrm.position - angleOrigin.position;
            dir = Vector3.ProjectOnPlane(dir, up);

            float distanceWeight = 0f;
            float absRotation = 0f;

            if (dir.sqrMagnitude >= 0.001f)
            {
                float distance = dir.magnitude;
                distanceWeight = distanceFadeRange <= 0f
                    ? (distance >= minAimDistance ? 1f : 0f)
                    : Mathf.InverseLerp(minAimDistance, minAimDistance + distanceFadeRange, distance);

                absRotation = Mathf.Abs(Vector3.SignedAngle(characterRoot.forward, dir / distance, up));
            }

            for (int i = 0; i < aimConstraints.Count; i++)
            {
                AimConstraintLimit limit = aimConstraints[i];
                float angleWeight = CalculateAngleWeight(absRotation, limit.fadeStartRotation, limit.maxRotation);
                float targetWeight = angleWeight * distanceWeight * limit.maxWeight;

                _currentWeights[i] = Mathf.Lerp(_currentWeights[i], targetWeight, Time.deltaTime * smooth);
                limit.constraint.weight = _currentWeights[i];
            }
        }

        private void SyncWeightCache()
        {
            while (_currentWeights.Count < aimConstraints.Count)
            {
                _currentWeights.Add(aimConstraints[_currentWeights.Count].constraint.weight);
            }

            while (_currentWeights.Count > aimConstraints.Count)
            {
                _currentWeights.RemoveAt(_currentWeights.Count - 1);
            }
        }

        private static float CalculateAngleWeight(float absRotation, float fadeStartRotation, float maxRotation)
        {
            if (absRotation >= maxRotation)
                return 0f;

            if (absRotation <= fadeStartRotation)
                return 1f;

            return Mathf.InverseLerp(maxRotation, fadeStartRotation, absRotation);
        }
    }
}
