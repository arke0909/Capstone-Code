using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.UI.Bar;
using Scripts.Combat.Fovs;
using UnityEngine;

namespace Scripts.Entities
{
    public enum VisibleState
    {
        InFOV,
        OutFOV
    }

    public class EntityVisibleManagement : MonoBehaviour, IContainerComponent, IFindable, IAfterInitialze
    {
        public int SightCount { get; set; }
        public delegate void OnVisibleStateChange(VisibleState prev, VisibleState next, bool onFound);
        public event OnVisibleStateChange OnVisibleStateChanged;

        private LocalEventBus _localEventBus;
        private VisibleState _visibleState;

        public ComponentContainer ComponentContainer { get; set; }
        public VisibleState VisibleState
        {
            get => _visibleState;
            set => SetVisibleState(value);
        }
        public bool IsFound => SightCount > 0;

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _localEventBus = componentContainer.Get<LocalEventBus>();
        }

        public void SetDefault()
        {
            SetVisibleState(IsFound ? VisibleState.InFOV : VisibleState.OutFOV, true);
        }

        public void Founded()
            =>VisibleState = VisibleState.InFOV;

        public void Escape()
            => VisibleState = VisibleState.OutFOV;

        public void AfterInitialize()
        {
            SetDefault();
        }

        private void SetVisibleState(VisibleState value, bool force = false)
        {
            if (!force && _visibleState == value)
                return;

            VisibleState prev = _visibleState;
            _visibleState = value;
            OnVisibleStateChanged?.Invoke(prev, value, IsFound);
        }
    }
}
