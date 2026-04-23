using Chipmunk.GameEvents;
using Code.UI.Core;
using UnityEngine;

namespace InGame.PlayerUI
{
    public struct ChangeCursorEvent : IEvent
    {
        public bool IsLocked { get; }

        public ChangeCursorEvent(bool isLocked = false)
        {
            IsLocked = isLocked;
        }
    }
}