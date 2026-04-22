namespace Scripts.Combat.Datas
{
    public interface IReloadable
    {
        public bool CanReload { get; }
        public void Reload();
    }
}
