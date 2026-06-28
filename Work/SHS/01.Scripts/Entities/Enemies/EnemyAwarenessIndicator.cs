using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.SHS.Entities.Enemies.Events.Local;
using Code.SHS.Entities.Enemies.Targetings.Events;
using Code.UI.Core;
using SHS.Scripts.Combats.Events;
using UnityEngine;
using Work.Code.Misc;

namespace Code.SHS.Entities.Enemies
{
    // 김민은 이 클래스 다 맘대로 가공해서 만들면 됨
    // 컴포넌트 경로 : Enemy -> EnemyAwarenessIndicator
    public class EnemyAwarenessIndicator : MonoBehaviour, IContainerComponent,
        ILocalEventSubscriber<TargetDetectedEvent>,
        ILocalEventSubscriber<TargetLostEvent>,
        ILocalEventSubscriber<EntityDeadEvent>,
        ILocalEventSubscriber<EnemySpawnEvent>
    {
        [SerializeField] private QuestMark questMark;

        private readonly float _recognize = 3f;
        private float _prevTime;
        private Enemy _enemy;

        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _enemy = componentContainer.Get<Enemy>(true);
        }

        // 타겟 발견 시 호출 (에너미 행동 : 바로 추적 및 전투 들어감)
        public void OnLocalEvent(TargetDetectedEvent eventData)
        {
            if (_enemy.IsDead) return;
            questMark.SetMark("!", UIDefine.RedColor, 1f);
        }

        // 타겟 추적 잃을 시, 또는 소리에 반응할 때 호출 (에너미 행동 : 마지막 타겟 위치를 탐색함)
        public void OnLocalEvent(TargetLostEvent eventData)
        {
            if (_enemy.IsDead) return;
            if (Time.time - _prevTime < _recognize) return;
            _prevTime = Time.time;
            questMark.SetMark("?", Color.white, 1f);
        }

        public void OnLocalEvent(EntityDeadEvent eventData)
        {
            questMark.ClearMark();
        }

        public void OnLocalEvent(EnemySpawnEvent eventData)
        {
            _prevTime = 0f;
            questMark.ClearMark();
        }
    }
}
