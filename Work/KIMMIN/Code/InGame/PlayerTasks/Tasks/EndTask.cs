using System;
using System.Collections.Generic;
using System.Text;

namespace Work.Code.PlayerTasks
{
    public class EndTask : PlayerTask
    {
        protected override string GetTaskText()
        {
            return $"열쇠를 제작하고 섬을 탈출하세요.";
        }

        protected override void StopTask()
        {
        }
    }
}
