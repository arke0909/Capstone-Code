using Chipmunk.ComponentContainers;
using UnityEngine;

namespace Work.Code.Skills
{
    public class HitInfoReader : MonoBehaviour, IContainerComponent
    {
        public ComponentContainer ComponentContainer { get; set; }
        
        public void OnInitialize(ComponentContainer componentContainer)
        {
            
        }
    }
}