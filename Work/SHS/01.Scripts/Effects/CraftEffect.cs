using Chipmunk.ComponentContainers;
using UnityEngine;

namespace SHS.Scripts.Effects
{
    public class CraftEffect : MonoBehaviour, IContainerComponent
    {
        public ComponentContainer ComponentContainer { get; set; }
        public void OnInitialize(ComponentContainer componentContainer)
        {
        }
    }
}