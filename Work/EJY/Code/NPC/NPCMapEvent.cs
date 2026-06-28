using System;
using System.Collections.Generic;
using Chipmunk.GameEvents;
using Code.UI.Core;
using Code.UI.Minimap.Core;
using Code.UI.NPC;
using UnityEngine;
using Work.Code.MapEvents;
using Random = UnityEngine.Random;

namespace Code.NPC
{
    [Serializable]
    public struct NPCSpawnEntry
    {
        public NPCDataSO data;
        public int weight;
    }
    
    public class NPCMapEvent : DropStructureEvent
    {
        [SerializeField] private NPC npc;
        [SerializeField] private List<NPCSpawnEntry> npcEntries;
        [SerializeField] private Sprite mapMarker;
        
        private NPCInteractUIPanel _interactUI;

        protected void Start()
        {
            _interactUI = UIManager.Instance.GetPanel<NPCInteractUIPanel>("NPC_Panel");
        }

        private NPCDataSO GetNPCData()
        {
            if (npcEntries == null || npcEntries.Count == 0)
            {
                Debug.LogWarning("NPC entry list is empty.", this);
                return null;
            }

            int totalWeight = 0;
            foreach (var entry in npcEntries)
            {
                if (entry.data == null || entry.weight <= 0)
                    continue;

                totalWeight += entry.weight;
            }

            if (totalWeight <= 0)
            {
                Debug.LogWarning("NPC entry list has no positive weight.", this);
                foreach (var entry in npcEntries)
                {
                    if (entry.data != null)
                        return entry.data;
                }

                return null;
            }

            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (var entry in npcEntries)
            {
                if (entry.data == null || entry.weight <= 0)
                    continue;

                currentWeight += entry.weight;
                if (randomValue < currentWeight)
                    return entry.data;
            }

            return npcEntries[npcEntries.Count - 1].data;
        }

        protected override void StartDropStructureEvent()
        {
            if (!TryGetRandomAreaPoint(out AreaPoint spawnPoint))
                return;

            NPCDataSO npcData = GetNPCData();
            if (npcData == null)
                return;
            
            npc.SetData(npcData);

            RegisterDropStructure(npc);
            string iconId = MinimapUtil.AddToMinimap(npc, ElementType.Marker, mapMarker, true, spawnPoint.Position);
            

            if (npcData.isOneTime)
            {
                void EndInteract(UIBase ui, bool isOn)
                {
                    if (!isOn)
                    {
                        _interactUI.OnToggleUI -= EndInteract;
                        npc.Despawn();
                    }
                }

                _interactUI.OnToggleUI += EndInteract;
            }
            
            npc.Init((entity) =>
            {
                _interactUI.ChangeContent(npcData);
                _interactUI.EnableUI(true);
            },
                ()=>
                {
                    _interactUI.DisableUI();
                    MinimapUtil.RemoveFromMinimap(iconId);
                });
            npc.Spawn(spawnPoint.Position);

            EventName = $"{spawnPoint.AreaIndex + 1} 지역 {npcData.npcName} 출현";
        }
    }
}