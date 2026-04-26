using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.Players;
using Code.UI.Core;
using TMPro;
using UnityEngine;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Craft.View
{
    public class CraftTreeView : MonoBehaviour
    {
        [SerializeField] private RectTransform lineRoot;
        [SerializeField] private RectTransform nodeRoot;
        [SerializeField] private UILineRenderer linePrefab;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TripleNodeTree tripleTreePrefab;
        
        private List<CraftNodeUI> _nodeList = new();
        private List<UILineRenderer> _lineList = new();
        private List<TripleNodeTree> _tripleTreeList = new();
        
        private readonly WaitForSeconds _showDelay = new(0.05f);
        private Coroutine _treeAnimRoutine;
        
        private CraftNodeUI Root => _nodeList.FirstOrDefault();

        public event Action<CraftTreeSO> OnNodeSelected;
        public event Func<ItemDataSO, int> RequestItemCount;
        
        public void InitTreeView(PlayerInventory inventory)
        {
            _nodeList = GetComponentsInChildren<CraftNodeUI>(true).ToList();
            _lineList = GetComponentsInChildren<UILineRenderer>(true).ToList();

            for (int i = 0; i < 3; i++)
            {
                TripleNodeTree tripleNodeTree = Instantiate(tripleTreePrefab, nodeRoot);
                tripleNodeTree.SetInventory(inventory);
                _tripleTreeList.Add(tripleNodeTree);
            }

            ClearTree();
        }
        
        public void RenderTree(CraftTreeSO tree, bool hasAnim)
        {
            ClearTree();
            titleText.text = tree.treeName;

            if (tree.isBinary)
                RenderBinaryTree(tree, hasAnim);
            else
                _tripleTreeList[0].InitTree(tree, Root.Rect, hasAnim, true);
        }

        private void RenderBinaryTree(CraftTreeSO tree, bool hasAnim)
        {
            if (hasAnim)
            {
                StopAnimation();
                _treeAnimRoutine = StartCoroutine(RenderBinaryTreeAnimated(tree));
            }
            else
            {
                RenderBinaryTreeImmediate(tree);
            }
        }
        
        private void RenderBinaryTreeImmediate(CraftTreeSO tree)
        {
            HashSet<int> indices = new();

            RenderRootNode(tree.Root, false);

            for (int i = 1; i < tree.nodeList.Count; i++)
            {
                NodeData node = tree.nodeList[i];

                if (indices.Contains(i) || node.Item == null)
                    continue;

                RenderNode(node, _nodeList[i], lineIndex: i - 1, false);

                if (IsTripleTree(node))
                {
                    RenderTripleTree(tree, node, i, false);
                    indices.Add(i * 2 + 1);
                    indices.Add(i * 2 + 2);
                }
            }
        }
        
        private IEnumerator RenderBinaryTreeAnimated(CraftTreeSO tree)
        {
            HashSet<int> indices = new();

            RenderRootNode(tree.Root, true);

            for (int i = 1; i < tree.nodeList.Count; i++)
            {
                NodeData node = tree.nodeList[i];

                if (indices.Contains(i) || node.Item == null)
                    continue;

                RenderNode(node, _nodeList[i], lineIndex: i - 1, true);

                if (IsTripleTree(node))
                {
                    RenderTripleTree(tree, node, i, true);
                    indices.Add(i * 2 + 1);
                    indices.Add(i * 2 + 2);
                }

                yield return _showDelay;
            }

            _treeAnimRoutine = null;
        }
        
        private void RenderRootNode(NodeData node, bool hasAnim)
        {
            int itemCount = RequestItemCount(node.Item);
            CraftNodeData craftData = new(node, itemCount, true);
            
            Root.InitUI(craftData, hasAnim);
        }
        
        private void RenderNode(NodeData node, CraftNodeUI nodeUI, int lineIndex, bool hasAnim)
        {
            nodeUI.Clear();

            bool isRootNode = lineIndex is 0 or 1;
            int itemCount = RequestItemCount(node.Item);
            CraftNodeData craftData = new(node, itemCount, isRootNode);

            nodeUI.InitUI(craftData, hasAnim);

            if (node.Tree != null)
            {
                nodeUI.SubscribeTooltip();
                nodeUI.SubscribeClick(() => OnNodeSelected?.Invoke(node.Tree));
            }

            if (lineIndex >= 0 && lineIndex < _lineList.Count)
            {
                _lineList[lineIndex].gameObject.SetActive(true);
            }
        }
        
        private void RenderTripleTree(CraftTreeSO tree, NodeData node, int idx, bool hasAnim)
        {
            if (idx > 2 || _tripleTreeList.Count == 0)
                return;

            int tripleIndex = Mathf.Min(idx - 1, _tripleTreeList.Count - 1);
            TripleNodeTree tripleTree  = _tripleTreeList[tripleIndex];

            tripleTree.InitTree(node.Tree, _nodeList[idx].Rect, hasAnim, false);
            tripleTree.RootNode.SubscribeTooltip();
            tripleTree.RootNode.SubscribeClick(() => OnNodeSelected?.Invoke(tree.nodeList[idx].Tree));

            _nodeList[idx].Clear();
        }
        
        private bool IsTripleTree(NodeData nodeData)
            => nodeData.Tree != null && !nodeData.Tree.isBinary;
        
        private void StopAnimation()
        {
            if (_treeAnimRoutine != null)
            {
                StopCoroutine(_treeAnimRoutine);
                _treeAnimRoutine = null;
            }
        }
        
        private void ClearTree()
        {
            StopAnimation();
            
            foreach (var node in _nodeList) 
            {
                node.Clear();
            }

            foreach (var line in _lineList) 
            {
                line.gameObject.SetActive(false);
            }
            
            foreach (var tree in _tripleTreeList) 
            {
                tree.Clear();
            }
        }
    }
}