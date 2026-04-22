using System;
using Chipmunk.GameEvents;

namespace Work.Code.GameEvents
{
    public struct PlayerGageEvent : IEvent
    {
        public string GageText { get; }
        public float Duration { get; }
        public Action OnComplete { get; }

        public PlayerGageEvent(string gageText, float duration, Action onComplete)
        {
            GageText = gageText;
            Duration = duration;
            OnComplete = onComplete;
        }
    }
    
    public struct StopPlayerGageEvent : IEvent { }
}