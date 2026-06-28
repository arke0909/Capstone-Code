namespace Work.Code.PlayerTasks
{
    public interface IProgressTask
    {
        bool HasProgress { get; }
        float Progress { get; }
    }
}
