using System;
using System.Collections;
using Code.Players;
using Code.UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Work.Code.Craft
{
    public class TripleNodeTree : MonoBehaviour
    {
        private CraftNodeUI[] _craftNodes;
        private UILineRenderer[] _lines;
        private PlayerInventory _inventory;
        private Coroutine _treeRoutine;
        
        private readonly WaitForSeconds _showDelay = new(0.04f);

        [field: SerializeField] public RectTransform Rect { get; set; }
        public CraftNodeUI RootNode => _craftNodes[0];

        private void Awake()
        {
            _lines = GetComponentsInChildren<UILineRenderer>(true);
            _craftNodes = GetComponentsInChildren<CraftNodeUI>(true);
        }

        public void SetInventory(PlayerInventory inventory)
        {
            _inventory = inventory;
        }

        public void InitTree(CraftTreeSO tree, RectTransform rect, bool hasAnim, bool isRoot)
        {
            if (tree == null || tree.isBinary)
                return;
            
            Rect.transform.position = rect.transform.position;
            StopAnimation();
            
            if (hasAnim)
                _treeRoutine = StartCoroutine(RenderTreeRoutine(tree, isRoot));
            else
                RenderTreeImmediate(tree, isRoot);
        }

        private IEnumerator RenderTreeRoutine(CraftTreeSO treeData, bool isRoot)
        {
            for (int i = 0; i < _craftNodes.Length; i++)
            {
                RenderNode(treeData, i, isRoot, true);
                
                if (i < _lines.Length)
                {
                    _lines[i].gameObject.SetActive(true);
                }
                
                yield return _showDelay;
            }
            
            _treeRoutine = null;
        }
        
        private void RenderTreeImmediate(CraftTreeSO tree, bool isRoot)
        {
            for (int i = 0; i < _craftNodes.Length; i++)
            {
                RenderNode(tree, i, isRoot, false);
                
                if (i < _lines.Length)
                {
                    _lines[i].gameObject.SetActive(true);
                }
            }
        }
        
        private void RenderNode(CraftTreeSO tree, int index, bool isRoot, bool hasAnim)
        {
            NodeData nodeData = tree.nodeList[index];
            int ownedCount = _inventory.GetItemCount(nodeData.Item);
            bool isResult = isRoot ? index == 0 : index != 0;

            CraftNodeData craftData = new CraftNodeData(nodeData, ownedCount, isResult);
            _craftNodes[index].InitUI(craftData, hasAnim);
        }
        
        private void StopAnimation()
        {
            if (_treeRoutine == null) return;
            StopCoroutine(_treeRoutine);
            _treeRoutine = null;
        }

        public void Clear()
        {
            if(_treeRoutine != null)
                StopCoroutine(_treeRoutine);
            
            foreach (var node in _craftNodes)
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