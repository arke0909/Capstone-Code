using UnityEngine;

namespace Code.ETC
{
    public class FixedRotationObject : MonoBehaviour
    {
        private Quaternion _originRotation;

        private void Awake()
        {
            _originRotation = transform.rotation;
        }

        private void LateUpdate()
        {
            transform.rotation = _originRotation;
        }
    }
}