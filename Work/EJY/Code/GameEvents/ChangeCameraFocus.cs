using Chipmunk.GameEvents;
using UnityEngine;

namespace Code.GameEvents
{
    public struct ChangeCameraFocus : IEvent
    {
        public Transform TargetTrm;
    }
}