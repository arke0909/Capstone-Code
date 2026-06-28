using System;
using System.Collections.Generic;
using System.Text;

namespace Work.Code.PlayerTasks
{
    public class EndTask : PlayerTask
    {
        protected override string GetTaskText()
        {
            return $"완성된 부분은 여기까지 입니다.\n플레이해주셔서 갑사합니다.";
        }

        protected override void StopTask()
        {
        }
    }
}
