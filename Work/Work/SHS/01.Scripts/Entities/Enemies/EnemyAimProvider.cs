using Chipmunk.ComponentContainers;
using Code.ETC;
using Code.SHS.Entities.Enemies.FSM;
using Code.SHS.Targetings.Enemies;
using UnityEngine;

namespace Code.SHS.Entities.Enemies
{
    [DisallowMultipleComponent]
    public class EnemyAimProvider : MonoBehaviour, IContainerComponent, IAimProvider
    {
        private TargetProvider _targetProvider;
        public ComponentContainer ComponentContainer { get; set; }

        private Enemy _enemy;
        public void OnInitialize(ComponentContainer componentContainer)
        {
            ComponentContainer = componentContainer;
            _targetProvider = this.Get<TargetProvider>();
        }

        public Vector3 GetAimPosition()
        {
            if (_targetProvider.CurrentTarget == null)
                return _targetProvider.LastTargetPosition;
            return _targetProvider.CurrentTarget.transform.position;
        }
    }
}
