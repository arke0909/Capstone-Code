using UnityEngine;

namespace Scripts.Entities.VisibleStates
{
    public interface IVisibleModule
    {
        public bool IsVisible { get; }
        public void Apply(FindableRenderer renderer);
    }
}
