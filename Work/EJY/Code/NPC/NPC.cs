using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using EPOOutline;
using Scripts.GameSystem.Structures;
using UnityEngine;

namespace Code.NPC
{
    public class NPC : InvokeCallbackStructure
    {
        [SerializeField] private Transform visualRoot;

        private Dictionary<NPCDataSO, NPCVisual> _npcVisualDict = new();
        private readonly List<OutlineTarget> _outlineTargets = new();
        private NPCVisual _currentVisual;
        private bool _isDespawning;

        protected override void Awake()
        {
            base.Awake();
            _npcVisualDict = visualRoot.GetComponentsInChildren<NPCVisual>()
                .ToDictionary(npcVisual => npcVisual.NPCData);
            foreach (var npcVisual in _npcVisualDict.Values)
            {
                npcVisual.SetVisual(false);
            }

            DeSelect();
            gameObject.SetActive(false);
        }

        public void SetData(NPCDataSO npcData)
        {
            ClearOutlineTargets();
            _currentVisual?.SetVisual(false);
            _currentVisual = null;

            if (npcData == null || !_npcVisualDict.TryGetValue(npcData, out NPCVisual npcVisual))
            {
                Debug.LogWarning("NPC data is not valid.", this);
                return;
            }

            _currentVisual = npcVisual;
            _currentVisual.SetVisual(true);

            Renderer[] renderers = _currentVisual.transform.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                OutlineTarget target = new OutlineTarget(renderer);
                Outlinable.AddTarget(target);
                _outlineTargets.Add(target);
            }
        }

        public new void Spawn(Vector3 targetPos)
        {
            transform.DOKill();
            _isDespawning = false;
            base.Spawn(targetPos);
        }

        public override void Despawn()
        {
            if (_isDespawning || !gameObject.activeSelf)
                return;

            _isDespawning = true;
            transform.DOKill();
            base.Despawn();
            _currentVisual?.SetVisual(false);
            ClearOutlineTargets();
        }

        private void ClearOutlineTargets()
        {
            foreach (OutlineTarget target in _outlineTargets)
            {
                Outlinable.RemoveTarget(target);
            }

            _outlineTargets.Clear();
        }
    }
}