using UnityEngine;

namespace Work.Code.MapEvents
{
    [CreateAssetMenu(fileName = "NewMapEvent", menuName = "SO/MapEvent", order = 0)]
    public class MapEventSO : ScriptableObject
    {
        public string eventName;
        public Sprite eventIcon;
        public Color eventColor;
        public float interval;
        public float duration;
        public bool isRepeat;
    }
}