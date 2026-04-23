using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Code.InGame.Crafting.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Work.Code.Crafting;
using Work.LKW.Code.Items.ItemInfo;
using Object = UnityEngine.Object;

public class CraftingTreeEditor : EditorWindow
{
    [SerializeField] private VisualTreeAsset visualTreeAsset;
    [SerializeField] private VisualTreeAsset nodeAsset;
    [SerializeField] private VisualTreeAsset treeAsset;
    [SerializeField] private CraftTreeListSO treeList;

    private List<CraftNode> _nodeList = new();
    private CraftNode _currentNode;
    private CraftTreeSO _currentTree;
    private bool _isBinaryMode = true;

    private Label _title;
    private TextField _nameField;
    private ScrollView _itemView;
    private IntegerField _countField;
    private Button _generateButton;
    private Button _binaryTreeButton;
    private Button _tripleTreeButton;
    private ObjectField _itemField;
    private ObjectField _treeField;
    
    private string _rootFolderPath;

    [MenuItem("Tools/CraftingTreeGenerator")]
    public static void ShowWindow()
    {
        CraftingTreeEditor wnd = GetWindow<CraftingTreeEditor>();
        wnd.titleContent = new GUIContent("CraftingTreeEditor");
        wnd.Show();
    }

    public void CreateGUI()
    {
        InitializeWindow();
        VisualElement root = rootVisualElement;
        visualTreeAsset.CloneTree(root);
        CreateTree(root);
        FindElements(root);
        UpdateTreeItems();
    }

    private void FindElements(VisualElement root)
    {
        _title = root.Q<Label>("Title");
        _countField = root.Q<IntegerField>("CountField");
        _nameField = root.Q<TextField>("NameField");
        _itemField = root.Q<ObjectField>("ItemField");
        _treeField = root.Q<ObjectField>("TreeField");
        _itemView = root.Q<ScrollView>("ItemView");
        _generateButton = root.Q<Button>("GenerateButton");
        _binaryTreeButton = root.Q<Button>("BinaryTreeButton");
        _tripleTreeButton = root.Q<Button>("TripleTreeButton");

        _countField.RegisterValueChangedCallback(_ =>  
            ChangeNodeData(m => m.Count = _countField.value));
        _itemField.RegisterValueChangedCallback(_ => 
            ChangeNodeData(m => m.Item = _itemField.value as ItemDataSO));
        _treeField.RegisterValueChangedCallback(_ => 
            ChangeNodeData(m => m.Tree = _treeField.value as CraftTreeSO));
        
        _generateButton.RegisterCallback<ClickEvent>(HandleGenerate);
        _binaryTreeButton.RegisterCallback<ClickEvent>(_ =>
        {
            _title.text = string.Empty;
            CreateTree(root);
        });
        _tripleTreeButton.RegisterCallback<ClickEvent>(_ =>
        {
            _title.text = string.Empty;
            CreateTree(root, false);
        });
    }

    private void HandleGenerate(ClickEvent evt)
    {
        CraftTreeSO newTree = CreateInstance<CraftTreeSO>();
        newTree.nodeList = _nodeList.Select(node => node.NodeData).ToList();
        newTree.treeName = _nameField.value;
        newTree.isBinary = _isBinaryMode;
        
        if (Directory.Exists($"{_rootFolderPath}/Trees") == false)
        {
            Directory.CreateDirectory($"{_rootFolderPath}/Trees");
        }
        
        AssetDatabase.CreateAsset(newTree, $"{_rootFolderPath}/Trees/{newTree.treeName}.asset");
        treeList.list.Add(newTree);
        SaveAssets(treeList);
        UpdateTreeItems();
    }

    private void SetUpTree(CraftTreeSO tree)
    {
        if (tree == null) return;

        _countField.value = 1;
        _itemField.value = null;
        _treeField.value = null;
        
        for (int i = 0; i < _nodeList.Count; i++)
        {
            if (i < tree.nodeList.Count)
                _nodeList[i].NodeData = tree.nodeList[i].Clone();
            else
                _nodeList[i].NodeData = new NodeData();
            
            _nodeList[i].NodeButton.SetEnabled(!_nodeList[i].NodeData.IsChild);
            UpdateNode(_nodeList[i]);
        }
    }

    private void UpdateTreeItems()
    {
        _itemView.Clear();
        
        foreach (var tree in treeList.list)
        {
            TemplateContainer itemTemplate = treeAsset.Instantiate();
            CraftTreeItem treeItem = new CraftTreeItem(itemTemplate, tree);
            _itemView.Add(itemTemplate);
            treeItem.Name = tree.treeName;
            treeItem.OnSelectEvent += HandleTreeSelect;
            treeItem.OnDeleteEvent += HandleTreeDelete;
        }
    }

    private void HandleTreeSelect(CraftTreeSO tree)
    {
        _currentNode = null;
        _currentTree = tree;

        _countField.SetValueWithoutNotify(1);
        _itemField.SetValueWithoutNotify(null);
        _treeField.SetValueWithoutNotify(null);
        _nameField.SetValueWithoutNotify(tree.treeName);
        
        CreateTree(rootVisualElement, tree.isBinary);
        SetUpTree(tree);

        _title.text = tree.treeName;
    }

    private void HandleSelectNode(CraftNode node)
    {
        if (_currentNode != null) _currentNode.BGColor = Color.grey;
        _currentNode = node;
        _currentNode.BGColor = Color.white;

        _itemField.SetValueWithoutNotify(node.NodeData.Item);
        _treeField.SetValueWithoutNotify(node.NodeData.Tree);
        _countField.SetValueWithoutNotify(node.NodeData.Count);
    }
    
    private void ChangeNodeData(Action<NodeData> modify, CraftTreeSO tree = null)
    {
        if (_currentNode == null) return;
        modify(_currentNode.NodeData);

        if (_currentTree != null)
        {
            _currentTree.nodeList[_currentNode.Index] = _currentNode.NodeData.Clone();
            SaveAssets(_currentTree);
        }

        UpdateNode(_currentNode);
        
        if (_currentNode.NodeData.Tree != null)
            SetTreeData();
        else if (_currentNode.NodeData.Tree == null)
            ResetTreeData();
    }

    private void ResetTreeData()
    {
        int idx = _currentNode.Index;
        for (int i = 1; i <= 2; i++)
        {
            if(idx * 2 + 1 >= treeList.list.Count) continue;
            _nodeList[idx * 2 + i].NodeData.IsChild = false;
        }
    }

    private void SetTreeData()
    {
        CraftTreeSO tree = _currentNode.NodeData.Tree;
        int count = _currentNode.NodeData.Count;
        int idx = _currentNode.Index;

        for (int i = 1; i <= 2; i++)
        {
            int target = idx * 2 + i;
        
            if (target < 0 || target >= _nodeList.Count) continue;

            if (i < tree.nodeList.Count)
            {
                NodeData cloned = tree.nodeList[i].Clone();
                cloned.Count = tree.nodeList[i].Count * count;
                cloned.IsChild = true;

                _nodeList[target].NodeData = cloned;
                _nodeList[target].Index = target;
                _nodeList[target].NodeButton.SetEnabled(false);
        
                UpdateNode(_nodeList[target]);
        
                if (_currentTree != null)
                {
                    _currentTree.nodeList[target] = cloned;
                }
            }
        }

        if (tree != null && tree.nodeList.Count > 0)
        {
            _currentNode.NodeData.Item = tree.nodeList[0].Item;
            UpdateNode(_currentNode);
        }

        _itemField.SetValueWithoutNotify(_currentNode.NodeData.Item);
        SaveAssets(_currentTree);
    }

    private void SaveAssets(Object target)
    {
        if(target == null) return;
        EditorUtility.SetDirty(target);
        AssetDatabase.SaveAssets();
    }


    private void UpdateNode(CraftNode node)
    {
        if (node.NodeData.Item == null)
        {
            node.Icon = null;
            node.Name = "[None]";
            return;
        }
        
        node.Icon = node.NodeData.Item.itemImage;
        node.Name = $"[{node.NodeData.Item.itemName}] {node.NodeData.Count}개";
    }
    
    private void InitializeWindow()
    {
        MonoScript monoScript = MonoScript.FromScriptableObject(this);
        string scriptPath = AssetDatabase.GetAssetPath(monoScript);
        _rootFolderPath = Directory.GetParent(Path.GetDirectoryName(scriptPath)).FullName.Replace('\\', '/');
        _rootFolderPath = "Assets" + _rootFolderPath.Substring(Application.dataPath.Length);

        if (treeList == null)
        {
            string filePath = $"{_rootFolderPath}/TreeListSO.asset";
            treeList = AssetDatabase.LoadAssetAtPath<CraftTreeListSO>(filePath);
            if (treeList == null)
            {
                Debug.LogWarning("CraftTreeListSO is not found. Create a new one");
                treeList = CreateInstance<CraftTreeListSO>();
                AssetDatabase.CreateAsset(treeList, filePath);
            }
        }
        
        visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{_rootFolderPath}/Editor/CraftingTreeEditor.uxml");
        Debug.Assert(visualTreeAsset != null, "visualTreeAsset is null");
        nodeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{_rootFolderPath}/Editor/CraftingNode.uxml");
        Debug.Assert(nodeAsset != null, "node is null");
        treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{_rootFolderPath}/Editor/CraftingTreeItem.uxml");
        Debug.Assert(treeAsset != null, "treeItem is null");
    }
    
    private void CreateTree(VisualElement root, bool isBinary = true)
    {
        _nodeList.Clear();
        _isBinaryMode = isBinary;
        
        int count = 1;
        int index = 0;

        for (int i = 0; i < 3; i++)
        {
            var level = root.Q<VisualElement>($"Level{i + 1}");
            level?.Clear();
            if (i >= 2 && isBinary == false) continue;
            
            for (int j = 0; j < count; j++)
                AddNode(index++, level);

            count *= isBinary ? 2 : 3;
        }
    }

    private void AddNode(int index, VisualElement root)
    {
        TemplateContainer nodeTemplate = nodeAsset.Instantiate();
        var node = new CraftNode(nodeTemplate, new NodeData());
        node.Index = index++;
        node.NodeData.Count = 1;
        _nodeList.Add(node);
        root.Add(nodeTemplate);
        node.OnSelectEvent += HandleSelectNode;
    }
    
    private void HandleTreeDelete(CraftTreeSO tree)
    {
        if (!EditorUtility.DisplayDialog("DeletePoolItem", $"Are you sure you want to delete{tree.treeName}?", "OK", "Cancel"))
            return;
        
        string path = AssetDatabase.GetAssetPath(tree);
        treeList.list.Remove(tree);
        AssetDatabase.DeleteAsset(path);
        SaveAssets(treeList);
        UpdateTreeItems();
    }
}