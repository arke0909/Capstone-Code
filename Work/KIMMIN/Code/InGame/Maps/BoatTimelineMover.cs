using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Work.Code.Map
{
    public class BoatTimelineMover : MonoBehaviour
    {
        [SerializeField] private Transform boat;
        [SerializeField] private SplineContainer boatSpline;
        [SerializeField, Range(0f, 1f)] private float progress;

        private void Awake()
        {
            SetProgress(0f);
        }

        public void SetProgress(float value)
        {
            progress = Mathf.Clamp01(value);
            MoveBoat(progress);
        }

        private void MoveBoat(float progress)
        {
            boatSpline.Evaluate(progress, out float3 position, out float3 tangent, out float3 upVector);

            boat.position = position;

            Vector3 direction = ((Vector3)tangent).normalized;
            Vector3 up = ((Vector3)upVector).normalized;
            boat.rotation = Quaternion.LookRotation(direction, up);
        }
    }
}
