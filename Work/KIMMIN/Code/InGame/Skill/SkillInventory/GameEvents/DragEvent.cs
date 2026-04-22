using Chipmunk.GameEvents;
using Scripts.SkillSystem;
using UnityEngine;

namespace Work.Code.SkillInventory.GameEvents
{
    public struct DragEvent : IEvent
    {
        public Sprite Sprite { get; }
        public bool IsDragStart { get; }

        public DragEvent(Sprite sprite, bool isDragStart)
        {
            Sprite = sprite;
            IsDragStart = isDragStart;
        }
    }
}