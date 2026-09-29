using System;
using UnityEngine;

namespace SHS.Scripts
{
    public class StackingCamera : MonoBehaviour
    {
        [SerializeField] private Camera parentCamera;
        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Reset()
        {
            parentCamera = gameObject.transform.parent.GetComponent<Camera>();
        }

        private void Update()
        {
            _camera.fieldOfView = parentCamera.fieldOfView;
        }
    }
}