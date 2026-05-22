using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.UI.Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Work.Code.UI.Misc;

namespace Work.Code.Tutorials
{
    public class TutorialProgressUI : UIBase
    {
        [SerializeField] private Transform root;
        [SerializeField] private Image linePrefab;
        [SerializeField] private Image fillImage;
        [SerializeField] private DynamicText progressText;

        public void InitProgressUI(int stateCount)
        {
            fillImage.transform.localScale = new Vector3(0f, 1f, 1f);
            progressText.SetText($"{0}/{stateCount} 완료", true);
            
            for (int i = 0; i < stateCount; i++)
            {
                var line = Instantiate(linePrefab, root);
                
                if (i == 0)
                {
                    line.gameObject.GetOrAddComponent<CanvasGroup>().alpha = 0;
                }
            }
        }

        public void SetProgress(int current, int total)
        {
            progressText.SetText($"{current}/{total} 완료");
            float progress = current / (float)total;
            fillImage.transform.DOScaleX(progress, 0.3f).SetUpdate(true);
        }
    }
}