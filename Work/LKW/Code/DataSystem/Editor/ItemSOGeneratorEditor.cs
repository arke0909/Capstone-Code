using Work.LKW.Code.Items.ItemInfo;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Codice.CM.Common.Partial;
using ExcelDataReader;
using Scripts.Combat.Datas;
using Unity.Plastic.Antlr3.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Work.LKW.Code.Items;

namespace Code.DataSystem.Editor
{
    public class ItemSOGeneratorEditor : EditorWindow
    {
        [SerializeField] private VisualTreeAsset visualTreeAsset;
        [SerializeField] private ItemDataBaseSO itemDataBase;
        [SerializeField] private VisualTreeAsset itemAsset;
        [SerializeField] private VisualTreeAsset selectBtnAsset;

        [SerializeField] private string savePath = "Assets/DataSystem/SO/ItemData";
        [SerializeField] private string filePath = "Assets/DataSystem/Item_Table.xlsx";

        private static readonly Dictionary<ItemType, Type> ItemTypeToSOType
            = new()
            {
                { ItemType.Material, typeof(MaterialDataSO) },
                { ItemType.Food, typeof(FoodDataSO) },
                { ItemType.Medicine, typeof(MedicineDataSO) },
                { ItemType.Gun,   typeof(GunDataSO) },
                { ItemType.MeleeWeapon,   typeof(MeleeWeaponDataSO) },
                { ItemType.Armor,    typeof(ArmorDataSO) },
                { ItemType.Bullet,    typeof(BulletDataSO) },
            };
                                                                                                     
        private Button _createButton;
        private ScrollView _itemView;
        private VisualElement _infoView;
        private ScrollView _buttonView;
        private ItemSOPreview _selectedItemPreview;
                
        private UnityEditor.Editor _cachedEditor;

        [MenuItem("Tools/ItemSOGenerator")]
        private static void ShowWindow()
        {
            var window = GetWindow<ItemSOGeneratorEditor>();
            window.titleContent = new UnityEngine.GUIContent("ItemSOGenerator");
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            visualTreeAsset.CloneTree(root);

            SetElement(root);
            CreateTypeButtons();
            EnsureTypeFoldersExist();
            GenerateItemSOPreview(ItemType.Material);
        }

        private void EnsureTypeFoldersExist()
        {
            foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
            {
                string parent = savePath;
                string folderName = type.ToString();

                if (!AssetDatabase.IsValidFolder($"{parent}/{folderName}"))
                {
                    AssetDatabase.CreateFolder(parent, folderName);
                }
            }
        }

        private DataSet ReadExcelData()
        {
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    return reader.AsDataSet(new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true
                        }
                    });
                }
            }
        }

        private void ProcessItemTypeSheet(ItemType type, DataTable table)
        {
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];

                string assetPath = $"{savePath}/{type.ToString()}/{row[0]}_{row[1]}.asset";

                ItemDataSO so = AssetDatabase.LoadAssetAtPath<ItemDataSO>(assetPath);

                if (so == null)
                {
                    so = CreateInstance(ItemTypeToSOType[type]) as ItemDataSO;
                    AutoExcelParser.ParseRow(row, i, so);
                    AssetDatabase.CreateAsset(so, assetPath);
                    EditorUtility.SetDirty(so);
                }
                else
                {
                    AutoExcelParser.ParseRow(row, i, so);
                    EditorUtility.SetDirty(so);
                }
                itemDataBase.allItems.Add(so);
            }
        }
        
        private void SetElement(VisualElement root)
        {
            _createButton = root.Q<Button>("GeneratorBtn");
            _createButton.clicked += HandleGeneratorButton;
                
            _itemView = root.Q<ScrollView>("ItemView");
                
            _buttonView = root.Q<ScrollView>("ButtonView");
                
            _infoView = root.Q<VisualElement>("InfoView");
        }
        
        private void CreateTypeButtons()
        {
            foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
            {
                TemplateContainer buttonTemplate = selectBtnAsset.Instantiate();
                TypeSelectButton typeSelectButton = new TypeSelectButton(buttonTemplate, type.ToString() ,type);

                typeSelectButton.OnButtonClicked += HandleTypeSelect;
                
                _buttonView.Add(buttonTemplate);
            }
        }
        
        private void HandleGeneratorButton()
        {
            try
            {
                _selectedItemPreview = null;
                _infoView.Clear();
                itemDataBase.allItems.Clear();

                var result = ReadExcelData();

                foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
                {
                    string sheetName = type.ToString();

                    if (!result.Tables.Contains(sheetName)) continue;

                    DataTable table = result.Tables[sheetName];
                    ProcessItemTypeSheet(type, table);
                }

                EditorUtility.SetDirty(itemDataBase);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Success", "Item SOs Generated Successfully!", "OK");
                EditorApplication.delayCall += () =>
                {
                    GenerateItemSOPreview(ItemType.Material);
                };
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to generate Item SOs: {ex.Message}", "OK");
                Debug.LogError($"ItemSOGenerator Error: {ex}");
            }
        }
        
        private void GenerateItemSOPreview(ItemType type)
        { 
            List<ItemDataSO> targetItems = itemDataBase.GetItemByType(type);
            if (targetItems == null || targetItems.Count <= 0)
            {
                _itemView.Clear();
                return;
            }
            
            _itemView.Clear();
            
            foreach (var itemData in targetItems)
            {
                if (itemData == null)
                    continue;

                if (!AssetDatabase.Contains(itemData))
                    continue;
                
                TemplateContainer itemTemplate = itemAsset.Instantiate();
                ItemSOPreview itemUI = new ItemSOPreview(itemTemplate, itemData);
                _itemView.Add(itemTemplate);
                itemUI.Name = itemData.itemName;
                itemUI.Id = itemData.itemId;
                if (_selectedItemPreview !=null&&_selectedItemPreview.itemData == itemData)
                {
                    HandleItemSelect(itemUI);
                }
                itemUI.OnSelectEvent += HandleItemSelect;
            }
        }
        
        
        private void HandleItemSelect(ItemSOPreview item)
        {
            if (item?.itemData == null) return;

            if (_cachedEditor != null)
            {
                DestroyImmediate(_cachedEditor);
                _cachedEditor = null;
            }

            if (_selectedItemPreview != null)
                _selectedItemPreview.IsActive = false;

            _selectedItemPreview = item;
            _selectedItemPreview.IsActive = true;

            _infoView.Clear();

            UnityEditor.Editor.CreateCachedEditor(item.itemData, null, ref _cachedEditor);

            var inspector = new IMGUIContainer(() =>
            {
                if (_cachedEditor != null)
                    _cachedEditor.OnInspectorGUI();
            });

            _infoView.Add(inspector);
        }


        private void HandleTypeSelect(ItemType type)
        {
            _infoView.Clear();
            _itemView.Clear();
            GenerateItemSOPreview(type);
        }
    }
}
