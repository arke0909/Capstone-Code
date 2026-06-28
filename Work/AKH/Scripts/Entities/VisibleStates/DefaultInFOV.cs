namespace Scripts.Entities.VisibleStates
{
    public class DefaultInFOV : IVisibleModule
    {
        public bool IsVisible => true;

        public void Apply(FindableRenderer renderer)
        {
            renderer.RestorePreviousMaterials();
            renderer.SetVisible(true);
        }
    }
}
