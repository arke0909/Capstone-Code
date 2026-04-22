    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    namespace Code.StatusEffectSystem.Editor
    {
        [CustomEditor(typeof(StatusEffectListSO))]
        public class StatusEffectListSOEditor : UnityEditor.Editor
        {
            [SerializeField] private VisualTreeAsset view = default;
            
            public override VisualElement CreateInspectorGUI()
            {
                VisualElement root = new VisualElement();
                InspectorElement.FillDefaultInspector(root, serializedObject, this);

                if (view != null)
                {
                    view.CloneTree(root);

                    root.Q<Button>("GenerateBtn").clicked += HandleGenerateEnum;
                }
                return root;
            }

            private void HandleGenerateEnum()
            {
                StatusEffectListSO list = target as StatusEffectListSO;

                int index = 0;
                string enumString = string.Join(", ", list.statusEffectData.Select(so =>
                {
                    so.idx = index;
                    EditorUtility.SetDirty(so);
                    return $"{so.StatusEffectName.ToUpper().Replace(' ', '_')} = {1 * Mathf.Pow(2,index++)}";
                }));
                
                string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
                string dirName = Path.GetDirectoryName(scriptPath);
                DirectoryInfo parentDirectory = Directory.GetParent(dirName);
                string path = parentDirectory.FullName;
                string code = string.Format(CodeFormat.EnumFormat, list.enumName, enumString);
                
                File.WriteAllText($"{path}/{list.enumName}.cs", code);
                
                string methodString = string.Join("\n ", list.statusEffectData.Select(so => 
                    $"\t\t\t\tcase {list.enumName}.{so.StatusEffectName.ToUpper().Replace(' ', '_')}:\n\t\t\t\t\treturn statusEffectData.FirstOrDefault(status => status.StatusEffectName == \"{so.StatusEffectName}\");"));
                
                code = string.Format(CodeFormat.MethodFormat, list.enumName, methodString);
                
                string originPath = $"{path}/{list.GetType().Name}.cs";
                Debug.Log(originPath);
                string originalCode = File.ReadAllText(originPath);
                
                int startIdx = originalCode.IndexOf("//" + CodeFormat.StartMark);
                int endIdx = originalCode.IndexOf("//" + CodeFormat.EndMark)+ CodeFormat.EndMark.Length;

                if (startIdx != -1 && endIdx != -1)
                {
                    string preText = originalCode.Substring(0, startIdx);
                    string postText = originalCode.Substring(endIdx + 2);
                    string finalCode = preText + code + postText;

                    File.WriteAllText(originPath, finalCode);
                }
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }