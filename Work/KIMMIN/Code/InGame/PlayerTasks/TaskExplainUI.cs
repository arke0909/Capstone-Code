using System;
using Code.UI.Core;
using UnityEngine;

namespace Work.Code.PlayerTasks
{
    public class TaskExplainUI : TaskCompleteInteraction
    {
        [SerializeField] private UIBase targetUI;
        
        public override void Interact()
        {
            targetUI.ToggleUI(true);
        }
    }
}