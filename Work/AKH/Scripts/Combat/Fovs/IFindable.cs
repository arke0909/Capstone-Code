using UnityEngine.Events;

namespace Scripts.Combat.Fovs
{
    public interface IFindable
    {
        int SightCount { get; set; }
        bool IsFounded => SightCount > 0;
        void Founded();
        void Escape();
    }
}
