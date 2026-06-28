using System.Collections.Generic;
using System.Linq;
using EPOOutline;
using Scripts.GameSystem.Structures;
using UnityEngine;

namespace Code.NPC
{
    public class NPC : InvokeCallbackStructure
    {
        [SerializeField] private Transform visualRoot;

        private Dictionary<NPCDataSO, NPCVisual> _npcVisualDict = new();
        private NPCVisual _currentVisual;

        protected override void Awake()
        {
            base.Awake();
            _npcVisualDict = visualRoot.GetComponentsInChildren<NPCVisual>()
                .ToDictionary(npcVisual => npcVisual.NPCData);
            foreach (var npcVisual in _npcVisualDict.Values)
            {
                npcVisual.SetVisual(false);
            }
        }

        public void SetData(NPCDataSO npcData)
        {
            _currentVisual?.SetVisual(false);
            
            if (npcData == null || !_npcVisualDict.ContainsKey(npcData))
            {
                Debug.Log("data is not valid");
                return;
            }

            _currentVisual = _npcVisualDict[npcData];
            _currentVisual.SetVisual(true);
            
            // 생성된 NPC 외형 아웃라인 동기화 해주기
            var renderers = _currentVisual.transform.GetComponentsInChildren<Renderer>();

            foreach (var renderer in renderers)
            {
                Outlinable.AddTarget(new OutlineTarget(renderer));
            }
        }

        public override void Despawn()
        {
            // despawn effect play
            base.Despawn();
            _currentVisual.SetVisual(false);
        }
    }
}