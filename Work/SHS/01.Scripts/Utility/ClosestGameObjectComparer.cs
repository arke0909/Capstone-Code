using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Utility
{
    public struct ClosestGameObjectComparer<T> : IComparer<T> where T : MonoBehaviour
    {
        private Vector3 _targetPosition;

        public ClosestGameObjectComparer(Vector3 targetPosition)
        {
            _targetPosition = targetPosition;
        }

        public int Compare(T x, T y)
        {
            return (x.transform.position - _targetPosition).sqrMagnitude
                .CompareTo((y.transform.position - _targetPosition).sqrMagnitude);
        }
    }

    public struct ClosestGameObjectComparer : IComparer<GameObject>
    {
        private Vector3 _targetPosition;

        public ClosestGameObjectComparer(Vector3 targetPosition)
        {
            _targetPosition = targetPosition;
        }

        public int Compare(GameObject x, GameObject y)
        {
            return (x.transform.position - _targetPosition).sqrMagnitude
                .CompareTo((y.transform.position - _targetPosition).sqrMagnitude);
        }
    }
}