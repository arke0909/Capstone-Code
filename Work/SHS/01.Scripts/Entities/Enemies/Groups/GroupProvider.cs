using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.SHS.Entities.Enemies.Events.Local;
using Scripts.Entities;
using SHS.Scripts.Combats.Events;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.Groups
{
    public struct JoinGroupEvent : ILocalEvent
    {
        public Group group;
    }
    public struct LeaveGroupEvent : ILocalEvent
    {
        public Group group;
    }
    public class GroupProvider : MonoBehaviour, IContainerComponent, ILocalEventSubscriber<EntityDeadEvent>,
        ILocalEventSubscriber<EnemySpawnEvent>
    {
        private Enemy _owner;
        public Group CurrentGroup => _currentGroup;
        private LocalEventBus _localEventBus;
        private Group _currentGroup;
        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _localEventBus = componentContainer.Get<LocalEventBus>();
            _owner = componentContainer.Get<Enemy>(true);
        }

        public void SetGroup(Group group)
        {
            if (_currentGroup != null)
            {
                _currentGroup.Leave(_owner);
                _localEventBus.Raise(new LeaveGroupEvent() { group = _currentGroup});
            }
            _currentGroup = group;
            if (_currentGroup != null)
            {
                _currentGroup.Join(_owner);
                _localEventBus.Raise(new JoinGroupEvent() { group = _currentGroup});
            }
        }
        public void OnLocalEvent(EntityDeadEvent eventData)
        {
            SetGroup(null);
        }

        public void OnLocalEvent(EnemySpawnEvent eventData)
        {
            SetGroup(null);
        }
    }
}
