using Chipmunk.ComponentContainers;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace SHS.Scripts.Entities.Rigings
{
    [RequireComponent(typeof(RigBuilder))]
    public class RigBuilderController : MonoBehaviour, IContainerComponent
    {
        public RigBuilder RigBuilder => _rigBuilder;
        private RigBuilder _rigBuilder;
        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            ComponentContainer = componentContainer;
            _rigBuilder = GetComponent<RigBuilder>();
        }
    }
}