using UnityEngine;

namespace Scripts.Entities.VisibleStates
{
    public class ChangeMaterial : IVisibleModule
    {
        private readonly Material _material;

        public ChangeMaterial(Material material, bool isVisible)
        {
            IsVisible = isVisible;
            _material = material;
        }

        public bool IsVisible { get; set; }

        public void Apply(FindableRenderer renderer)
        {
            renderer.ApplyMaterial(_material);
            renderer.SetVisible(true);
        }
    }
}
