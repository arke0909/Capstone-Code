using Code.NPC;
using Code.UI.Core;
using Code.UI.Inventory;
using Scripts.Players;
using UnityEngine;

namespace Code.UI.NPC
{
    public abstract class NPCInteractUIContent : UIBase
    {
        [field: SerializeField] public NPCDataSO NpcDataSO { get; private set; }
        
        private Player _player;

        public virtual void Init(Player player)
        {
            _player = player;
        }
    }
}