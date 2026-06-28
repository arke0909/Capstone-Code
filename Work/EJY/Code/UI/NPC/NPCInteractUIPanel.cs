using System.Collections.Generic;
using System.Linq;
using Code.NPC;
using Code.UI.Core;
using DewmoLib.Dependencies;
using Scripts.Players;

namespace Code.UI.NPC
{
    public class NPCInteractUIPanel : UIPanel
    {
        private Dictionary<NPCDataSO, NPCInteractUIContent> _content;
        private NPCInteractUIContent _currentContent;

        [Inject] private Player _player;

        protected override void Awake()
        {
            base.Awake();

            _content = GetComponentsInChildren<NPCInteractUIContent>()
                .ToDictionary(content => content.NpcDataSO, content => content);
            
            _content.Values.ToList().ForEach(content => content.Init(_player));
        }

        public void ChangeContent(NPCDataSO npcDataSO)
        {
            _currentContent?.DisableUI();
            _currentContent = _content.GetValueOrDefault(npcDataSO);
            _currentContent.EnableUI();
        }
    }
}