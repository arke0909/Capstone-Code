using UnityEngine;

namespace Code.UI.Core
{
    public class UIToggleUtil : MonoBehaviour
    {
        [ContextMenu("Show UI")]
        public void ShowUI()
        {
            var canvas  = UIUtility.GetOrAddComponent<CanvasGroup>(gameObject);
            canvas.alpha = 1;
            canvas.interactable = true;
            canvas.blocksRaycasts = true;
        }

        [ContextMenu("Hide UI")]
        public void HideUI()
        {
            var canvas  = UIUtility.GetOrAddComponent<CanvasGroup>(gameObject);
            canvas.alpha = 0;
            canvas.interactable = false;
            canvas.blocksRaycasts = false;
        }
    }
}