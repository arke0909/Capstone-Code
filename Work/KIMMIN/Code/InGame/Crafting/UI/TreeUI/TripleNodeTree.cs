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
        private Func<CraftTreeSO, bool> _canCraftTree;
        private Action<CraftTreeSO> _onNodeSelected;
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

        public void SetCanCraftChecker(Func<CraftTreeSO, bool> canCraftTree)
        {
            _canCraftTree = canCraftTree;
        }

        public void SetNodeSelectAction(Action<CraftTreeSO> onNodeSelected)
        {
            _onNodeSelected = onNodeSelected;
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
            CraftTreeSO selectTree = !isRoot && index == 0 ? tree : nodeData.Tree;
            bool isCraftableBySubItems = !isResult && selectTree != null
                                                   && (ownedCount >= nodeData.Count || CanCraftTree(selectTree));

            CraftNodeData craftData = new CraftNodeData(nodeData, ownedCount, isResult, isCraftableBySubItems);
            _craftNodes[index].InitUI(craftData, hasAnim);

            SubscribeNode(_craftNodes[index], selectTree);
        }

        private void SubscribeNode(CraftNodeUI node, CraftTreeSO tree)
        {
            if (tree == null)
                return;
            
            node.SubscribeTooltip();
            node.SubscribeClick(() => _onNodeSelected?.Invoke(tree));
        }

        private bool CanCraftNode(CraftTreeSO tree, NodeData nodeData, int index)
        {
            if (_canCraftTree == null)
                return false;

            CraftTreeSO targetTree = index == 0 ? tree : nodeData.Tree;
            return CanCraftTree(targetTree);
        }

        private bool CanCraftTree(CraftTreeSO tree)
        {
            return tree != null && _canCraftTree != null && _canCraftTree(tree);
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
