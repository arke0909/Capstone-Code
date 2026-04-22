/*using System.Collections.Generic;
using System.Linq;
using Code.SkillSystem.Upgrade;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Code.UI.SkillTree;

namespace Code.UI.Editor
{
    [CustomEditor(typeof(SkillTreePanel))]
    public class SkillTreePanelEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset view = default;
        [SerializeField] private GameObject linePrefab = default;
        
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);
            view.CloneTree(root);

            root.Q<Button>("DrawButton").clicked += HandleDrawClick;
            
            return root;
        }

        private void HandleDrawClick()
        {
            SkillTreePanel panel = target as SkillTreePanel;

            Dictionary<SkillUpgradeSO, SkillUpgradeUI> uiDict 
                = panel.GetComponentsInChildren<SkillUpgradeUI>().ToDictionary(ui => ui.UpgradeData);
            
            //기존에 그려뒀던 라인은 모두 제거한다.
            List<UILineRenderer> existsLines = panel.LineParentTrm.GetComponentsInChildren<UILineRenderer>().ToList();
            foreach (UILineRenderer line in existsLines)
            {
                DestroyImmediate(line.gameObject);
            }

            foreach (SkillUpgradeUI upgradeUI in uiDict.Values)
            {
                ////필요한 업그레이드로 라인을 이을꺼다. 
                //foreach (SkillUpgradeSO upgradeData in upgradeUI.UpgradeData.needUpgradeList)
                //{
                //    GameObject lineObject = Instantiate(linePrefab, panel.LineParentTrm);
                //    UILineRenderer line = lineObject.GetComponent<UILineRenderer>();
                //    RectTransform lineRectTrm = line.GetComponent<RectTransform>();
                //    lineRectTrm.anchoredPosition = panel.LineParentTrm.InverseTransformPoint(upgradeUI.RectTrm.position);

                //    // 필요한 업그레이드 데이터를 가진 UI
                //    if (uiDict.TryGetValue(upgradeData, out SkillUpgradeUI parentUI))
                //    {
                //        DrawLineToParent(parentUI, upgradeUI.RectTrm, line,upgradeUI.UpgradeData);
                //    }
                //    else
                //    {
                //        Debug.Log($"There is no parent UI for {upgradeData.UpgradeTitle}");
                //    }
                //}
            }
        }

        private void DrawLineToParent(SkillUpgradeUI parentUI, RectTransform selfRect, UILineRenderer line,SkillUpgradeSO data)
        {
            line.data = data;
            Vector3 endPos = selfRect.InverseTransformPoint(parentUI.transform.position);

            Vector3 middlePos = endPos * 0.5f;

            line.points = new Vector2[4]
            {
                Vector2.zero,
                new Vector2(0, middlePos.y),
                new Vector2(endPos.x, middlePos.y),
                new Vector2(endPos.x, endPos.y)
            };
            
            //float yDelta = (parentUI.RectTrm.position.y - selfRect.position.y) / 2;
            //line.points = new Vector2[4];
            //line.points[0] = line.rectTransform.InverseTransformPoint(selfRect.position);
            //line.points[1] = line.rectTransform.InverseTransformPoint(selfRect.position + new Vector3(0, yDelta));
            //line.points[2] = line.rectTransform.InverseTransformPoint(parentUI.RectTrm.position + new Vector3(0, -yDelta));
            //line.points[3] = line.rectTransform.InverseTransformPoint(parentUI.RectTrm.position);
        }
    }
}*/