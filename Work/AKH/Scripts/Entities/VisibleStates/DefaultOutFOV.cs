namespace Scripts.Entities.VisibleStates
{
    public class DefaultOutFOV : IVisibleModule
    {
        public bool IsVisible => false;

        public void Apply(FindableRenderer renderer)
        {
            renderer.RestorePreviousMaterials();
            renderer.SetVisible(false);
        }
    }

}
