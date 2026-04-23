using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Work.Code.Crafting
{
    public class CraftTreeUI : MonoBehaviour, IUIElement<CraftTreeSO>
    { 
        [SerializeField] private RectTransform lineRoot;
        [SerializeField] private RectTransform nodeRoot;
        [SerializeField] private UILineRenderer linePrefab;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Button createButton;
        [SerializeField] private TripleNodeTree tripleTree;

        private List<CraftNodeUI> _nodes = new();
        private List<UILineRenderer> _lines = new();
        private List<TripleNodeTree> _tripleTrees = new();
        private CraftTreeSO _currentTree;
        private Coroutine _binaryRoutine;
        private CraftController _craftController;
        private WaitForSeconds _delay = new WaitForSeconds(0.05f);
        private WaitForSeconds _nonDelay = new WaitForSeconds(0f);

        private int _tripleCount;
        
        public CraftNodeUI Root => _nodes.FirstOrDefault();
        
        private void Start()
        {
            createButton.onClick.AddListener(() => CreateItem(_currentTree));
            EventBus<UpdateInventoryUIEvent>.OnEvent += HandleChangeUI;
        }

        private void OnDestroy()
        {
            createButton.onClick.RemoveListener(() => CreateItem(_currentTree));
            EventBus<UpdateInventoryUIEvent>.OnEvent -= HandleChangeUI;
        }

        public void Initialize(CraftController craftController)
        {
            _nodes = GetComponentsInChildren<CraftNodeUI>().ToList();
            _lines = GetComponentsInChildren<UILineRenderer>(true).ToList();
            _craftController = craftController;
            
            for (int i = 0; i < 3; i++)
            {
                var node = Instantiate(tripleTree, nodeRoot.transform);
                node.InitNode(craftController.Inventory);
                _tripleTrees.Add(node);
            }
            
            Clear();
        }
        
        private void HandleChangeUI(UpdateInventoryUIEvent evt)
        {
            if(_currentTree != null)
                UpdateTree(_currentTree, false);
        }

        public void EnableFor(CraftTreeSO tree)
        {
            UpdateTree(tree);
        }

        public void Clear()
        {
            if (_binaryRoutine != null)
                StopCoroutine(_binaryRoutine);
            
            foreach (var node in _nodes)
                node.Clear();
            
            foreach (var line in _lines)
                line.gameObject.SetActive(false);

            foreach (var tripleNode in _tripleTrees)
                tripleNode.Clear();
        }
        
        private void UpdateTree(CraftTreeSO tree, bool isNotificate = true)
        {
            Clear();
            title.text = tree == null ? string.Empty : tree.treeName;
            _tripleCount = 0;
            
            if (tree == null) return;
            
            if(tree.isBinary)
                BinaryTree(tree, isNotificate);
            else
                _tripleTrees[0].Init(tree, _nodes[0].Rect, isNotificate, true);
            
            _currentTree = tree;
        }

        private void BinaryTree(CraftTreeSO tree, bool isNotificate)
        {
            if (_binaryRoutine != null)
                StopCoroutine(_binaryRoutine);

            if (isNotificate)
                _binaryRoutine = StartCoroutine(BinaryTreeRoutine(tree));
            else
                BinaryTreeImmediate(tree);
        }
        
        private IEnumerator BinaryTreeRoutine(CraftTreeSO tree)
        {
            List<int> childList = new();
            CraftNodeData nodeData = new CraftNodeData(tree.Root, tree.Count, true);
            Root.InitUI(nodeData);

            for (int i = 1; i < tree.nodeList.Count; i++)
            {
                var node = tree.nodeList[i];
                if (childList.Contains(i) || node.Item == null) continue;

                InitializeNode(tree.nodeList[i], _nodes[i], i - 1);
                if (node.Tree != null && node.Tree.isBinary == false)
                {
                    InitTipleNode(tree, node, i);
                    childList.Add(i * 2 + 1);
                    childList.Add(i * 2 + 2);
                }

                yield return _delay;
            }

            _binaryRoutine = null;
        }
        
        private void BinaryTreeImmediate(CraftTreeSO tree)
        {
            List<int> childList = new();
            CraftNodeData nodeData = new CraftNodeData(tree.Root, tree.Count, true);
            Root.InitUI(nodeData, false);

            for (int i = 1; i < tree.nodeList.Count; i++)
            {
                var node = tree.nodeList[i];
                if (childList.Contains(i) || node.Item == null) continue;

                InitializeNode(tree.nodeList[i], _nodes[i], i - 1, false);

                if (node.Tree != null && node.Tree.isBinary == false)
                {
                    InitTipleNode(tree, node, i, false);
                    childList.Add(i * 2 + 1);
                    childList.Add(i * 2 + 2);
                }
            }
        }

        private void InitTipleNode(CraftTreeSO tree, NodeData data, int idx, bool isNotificate = true)
        {
            if (idx > 2) return;
            var node = _tripleTrees[_tripleCount++];
            node.Init(data.Tree, _nodes[idx].Rect, isNotificate);
            node.RootNode.SubscribeTooltip();
            node.RootNode.SubscribeClick(() => UpdateTree(tree.nodeList[idx].Tree));
            _nodes[idx].Clear();
        }

        private void InitializeNode(NodeData data, CraftNodeUI node, int index, bool isNotificate = true)
        {
            node.Clear();
            bool isNeedItem = index == 0 || index == 1;
            int count = _craftController.Inventory.GetItemCount(data.Item);
            CraftNodeData nodeData = new CraftNodeData(data, count, !isNeedItem);
            node.InitUI(nodeData, isNotificate);

            if (node.Data.Tree != null && index >= 0)
            {
                node.SubscribeTooltip();
                node.SubscribeClick(() => UpdateTree(node.Data.Tree));
            }

            if (index >= 0 && index < _lines.Count)
                _lines[index].gameObject.SetActive(true);
        }

        public void CreateItem(CraftTreeSO tree)
        {
            if(_craftController.Inventory == null || tree == null) return;

            if (!_craftController.Craft(tree))
                Debug.Log("아이템 부족");
        }
        
        #region ConnectLine
        #if UNITY_EDITOR
        [ContextMenu("Clear Line")]
        private void ClearLine()
        {
            foreach (var line in _lines)
            {
                if (line != null)
                    DestroyImmediate(line.gameObject);
            }
            
            _lines.Clear();
        }

        [ContextMenu("Connect Line")]
        private void ConnectThreeLines()
        {
            Clear();
            _nodes = GetComponentsInChildren<CraftNodeUI>().ToList();
            _lines = GetComponentsInChildren<UILineRenderer>(true).ToList();

            /*for(int i = 0; i < _nodes.Count; i++)
            {
                CreateBentLine(_nodes[i], _nodes[i * 2 + 1]);
                CreateBentLine(_nodes[i], _nodes[i * 2 + 2]);
            }*/

            CreateBentLine(_nodes[0], _nodes[1]);
            CreateBentLine(_nodes[0], _nodes[2]);
            CreateBentLine(_nodes[0], _nodes[3]);
        }

        private void CreateBentLine(CraftNodeUI parent, CraftNodeUI child)
        {
            var line = Instantiate(linePrefab, lineRoot);
            _lines.Add(line);

            Vector2 start = GetLocalPoint(parent.LineStartRect);
            Vector2 end   = GetLocalPoint(child.LineEndRect);

            float verticalOffset = 20f;
            float midY = start.y - verticalOffset;
            line.points = new[] { start, new(start.x, midY), new(end.x, midY), end };
            line.SetVerticesDirty();
        }
        
        private Vector2 GetLocalPoint(RectTransform target)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(lineRoot,
                RectTransformUtility.WorldToScreenPoint(null, target.position), 
                null, out Vector2 localPoint);

            return localPoint;
        }
        #endif
        #endregion
    }
}