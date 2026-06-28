using Chipmunk.ComponentContainers;
using UnityEngine;

namespace SHS.Scripts.Skills.Miner
{
    public class PickaxeVisualSocket : MonoBehaviour, IContainerComponent
    {
        [SerializeField] private Transform socket;

        public ComponentContainer ComponentContainer { get; set; }
        public Transform Socket => socket;

        public void OnInitialize(ComponentContainer componentContainer)
        {
            ComponentContainer = componentContainer;
            Debug.Assert(socket != null, $"{nameof(PickaxeVisualSocket)} requires {nameof(socket)}.", this);
        }

        private void OnValidate()
        {
            Debug.Assert(socket != null, $"{nameof(PickaxeVisualSocket)} requires {nameof(socket)}.", this);
        }
    }
}
