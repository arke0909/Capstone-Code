using Chipmunk.GameEvents;
using Code.ETC.CameraZone;
using UnityEngine;

namespace Code.Events
{
    public struct CameraSwitchEvent : IEvent
    {
        public CameraSwitchData Data { get;}
        
        public CameraSwitchEvent(CameraSwitchData data)
        {
            Data = data;
        }
    }
}