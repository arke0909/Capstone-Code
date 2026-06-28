using Chipmunk.GameEvents;
using UnityEngine;

namespace Code.GameEvents
{
    public struct ChangeCameraFocus : IEvent
    {
        public Transform TargetTrm;
    }

    public struct ChangeCameraZoom : IEvent
    {
        public float FieldOfViewReduction { get; }

        public ChangeCameraZoom(float fieldOfViewReduction)
        {
            FieldOfViewReduction = fieldOfViewReduction;
        }
    }
}
