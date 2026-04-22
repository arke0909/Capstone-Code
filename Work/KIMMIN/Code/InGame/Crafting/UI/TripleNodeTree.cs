using System;
using System.Collections;
using Code.Players;
using Code.UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Work.Code.Crafting
{
    public class TripleNodeTree : MonoBehaviour
    {
        private CraftNodeUI[] _nodes = new CraftNodeUI[4];
        private UILineRenderer[] _lines;
        private Coroutine _treeRoutine;
        private PlayerInventory _inventory;
        private WaitForSeconds _delay = new WaitForSeconds(0.04f);
        private WaitForSeconds _nonDelay = new WaitForSeconds(0f);

        [field: SerializeField] public RectTransform Rect { get; set; }
        
        public CraftNodeUI RootNode => _nodes[0];

        private void Awake()
        {
            _lines = GetComponentsInChildren<UILineRenderer>(true);
            _nodes = GetComponentsInChildren<CraftNodeUI>(true);
        }

        public void InitNode(PlayerInventory inventory)
        {
            _inventory = inventory;
        }

        public void Init(CraftTreeSO tree, RectTransform rect, bool isNotificate, bool isRoot = false)
        {
            if (tree == null || tree.isBinary) return;
            
            Rect.transform.position = rect.transform.position;
            if(_treeRoutine != null)
                StopCoroutine(_treeRoutine);
            
            if (isNotificate)
                _treeRoutine = StartCoroutine(TreeRoutine(tree, isRoot));
            else
                TreeImmediate(tree, isRoot);
        }

        private IEnumerator TreeRoutine(CraftTreeSO tree, bool isRoot)
        {
            for(int i = 0; i < _nodes.Length; i++)
            {
                int count = _inventory.GetItemCount(tree.nodeList[i].Item);
                bool isRootNode = isRoot ? i == 0 : i != 0;
                CraftNodeData data = new(tree.nodeList[i], count, isRootNode);
                _nodes[i].InitUI(data, true);
                if (i < _nodes.Length - 1)
                    _lines[i].gameObject.SetActive(true);

                yield return _delay;
            }
            
            _treeRoutine = null;
        }
        
        private void TreeImmediate(CraftTreeSO tree, bool isRoot)
        {
            for (int i = 0; i < _nodes.Length; i++)
            {
                int count = _inventory.GetItemCount(tree.nodeList[i].Item);
                bool isRootNode = isRoot ? i == 0 : i != 0;
                CraftNodeData data = new(tree.nodeList[i], count, isRootNode);

                _nodes[i].InitUI(data, false);

                if (i < _nodes.Length - 1)
                    _lines[i].gameObject.SetActive(true);
            }
        }

        public void Clear()
        {
            if(_treeRoutine != null)
                StopCoroutine(_treeRoutine);
            
            foreach (var node in _nodes)
            {
                node.Clear();
            }
            
            foreach (var line in _lines)
            {
                line.gameObject.SetActive(false);
            }
        }
    }
}