using UnityEngine;
using Work.Code.MapEvents;

namespace Scripts.GameSystem.GameEvents
{
    public class BossRoomEnableEvent : MapEvent
    {
        protected override void StartEvent()
        {
            EventName = "보스방에 입장 가능합니다";
        }
    }
}
