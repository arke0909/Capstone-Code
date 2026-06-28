using DewmoLib.Dependencies;
using Scripts.Players;
using Work.Code.GameEvents;
using Work.Code.PlayerTasks.TaskTrigger;

namespace Work.Code.PlayerTasks
{
    public class TeleportTask : PlayerTask
    {
        public override void StartTask()
        {
            base.StartTask();
            _player.LocalEventBus.Subscribe<PlayerTeleportEvent>(HandlePlayerTeleport);
        }

        private void HandlePlayerTeleport(PlayerTeleportEvent obj)
        {
            CompleteTask();
        }

        protected override void StopTask()
        {
            _player.LocalEventBus.Unsubscribe<PlayerTeleportEvent>(HandlePlayerTeleport);
        }

        protected override string GetTaskText()
        {
            return "텔레포트 장치에서 다른 구역으로 이동하세요.\n(상세 위치는 맵에 표시됩니다.)";
        }
    }
}