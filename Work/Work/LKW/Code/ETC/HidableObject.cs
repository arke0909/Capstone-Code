using Scripts.Combat.Fovs;
using UnityEngine;
using UnityEngine.Events;

namespace Code.ETC
{
    public class HidableObject : MonoBehaviour, IFindable
    {
        [SerializeField] private MeshRenderer[] meshRenderer;
        
        public UnityEvent<bool> OnFound { get; }
        public int SightCount { get; set; }
        
        public void Founded()
        {
            foreach (var render in meshRenderer)
            {
                render.enabled = false;
            }
        }

        public void Escape()
        {
            foreach (var render in meshRenderer)
            {
                render.enabled = true;
            }
        }
    }
}