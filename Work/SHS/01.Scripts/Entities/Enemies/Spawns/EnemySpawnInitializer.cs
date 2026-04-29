using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.SHS.Entities.Enemies.Events.Local;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.Spawns
{
    public class EnemySpawnInitializer : MonoBehaviour, IContainerComponent, ILocalEventSubscriber<EnemySpawnEvent>
    {
        public ComponentContainer ComponentContainer { get; set; }
        public virtual void OnInitialize(ComponentContainer componentContainer)
        {
        }

        public virtual void OnLocalEvent(EnemySpawnEvent eventData)
        {
        }
    }
}