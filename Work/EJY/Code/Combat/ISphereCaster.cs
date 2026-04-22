namespace Scripts.Combat
{
    public interface ISphereCaster
    {
        public float CastRadius { get; }
        
        public void SetRadius(float radius);
    }
}