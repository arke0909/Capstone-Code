using Code.GameEvents;

namespace Work.Code.PlayerTasks
{
    public class ReloadTask : PlayerTask
    {
        private const string ReloadTaskText = "[R]키를 눌러 총을 재장전하세요.";

        public override void StartTask()
        {
            base.StartTask();
            _player.LocalEventBus.Subscribe<AmmoUpdateEvent>(HandleChangeAmmo);
        }
        
        private void HandleChangeAmmo(AmmoUpdateEvent evt)
        {
            if (evt.CurrentAmmo == evt.TotalAmmo)
            {
                CompleteTask();
            }
        }

        protected override void StopTask()
        {
            _player.LocalEventBus.Unsubscribe<AmmoUpdateEvent>(HandleChangeAmmo);
        }

        protected override string GetTaskText()
        {
            return ReloadTaskText;
        }
    }
}