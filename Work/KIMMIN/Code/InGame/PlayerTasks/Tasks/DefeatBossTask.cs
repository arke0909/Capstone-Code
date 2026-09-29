using Chipmunk.GameEvents;
using Scripts.Players;
using UnityEngine;
using Work.Code.GameEvents;

namespace Work.Code.PlayerTasks
{
    public class DefeatBossTask : PlayerTask, IProgressTask
    {
        [SerializeField] private int bossCount = 2;
        private int _defeatedCount;
        
        public bool HasProgress { get; }
        public float Progress => _defeatedCount / (float)bossCount;
        
        public override void InitializeTask(Player player)
        {
            base.InitializeTask(player);
            EventBus.Subscribe<DefeatBossEvent>(HandleBossDefeated);
            _defeatedCount = 0;
        }

        private void HandleBossDefeated(DefeatBossEvent evt)
        {
            _defeatedCount++;
            UpdateTaskText();
            
            if (_defeatedCount == bossCount)
            {
                CompleteTask();
            }
        }

        protected override void StopTask()
        {
            EventBus.Subscribe<DefeatBossEvent>(HandleBossDefeated);
        }

        protected override string GetTaskText()
        {
            return $"보스를 처치하고 열쇠 재료를 모으세요 ({_defeatedCount}/{bossCount})\n(상세 위치는 맵에 표시됩니다.)";
        }
    }
}