using UnityEngine;

namespace Work.Code.UI.ContextMenu
{
    [CreateAssetMenu(fileName = "ContextActionSO", menuName = "SO/ContextAction", order = 0)]
    public class ContextActionSO : ScriptableObject
    {
        public Sprite actionIcon;
        public int sortOrder;
        
        [Header("Action Text Settings")]
        public string actionText;
        public string inactiveText;
        [TextArea] public string activeDescription;
        [TextArea] public string inactiveDescription;
        
        public BaseContextAction contextAction;
    }
}