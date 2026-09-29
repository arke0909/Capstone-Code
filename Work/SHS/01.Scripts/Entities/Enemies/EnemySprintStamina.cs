using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.SHS.Entities.Enemies.Events.Local;
using Scripts.Entities.Vitals;
using UnityEngine;

namespace Code.SHS.Entities.Enemies
{
    public class EnemySprintStamina : MonoBehaviour, IContainerComponent, ILocalEventSubscriber<EnemySpawnEvent>
    {
        [SerializeField, Min(0f)] private float staminaUsePerSecond = 20f;
        [SerializeField, Range(0f, 1f)] private float sprintRecoverRatio = 0.35f;

        private StaminaCompo _staminaCompo;
        private bool _isExhausted;

        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _staminaCompo = componentContainer.Get<StaminaCompo>();
            Debug.Assert(_staminaCompo != null,
                $"{nameof(EnemySprintStamina)} requires {nameof(StaminaCompo)}.", this);
        }

        public NavMoveType GetSprintMoveType()
        {
            if (_staminaCompo == null)
                return NavMoveType.Walk;

            if (_isExhausted)
            {
                if (_staminaCompo.CurrentValue >= GetRecoverThreshold())
                    _isExhausted = false;
                else
                    return NavMoveType.Walk;
            }

            if (_staminaCompo.CurrentValue <= 0f)
            {
                _isExhausted = true;
                return NavMoveType.Walk;
            }

            if (staminaUsePerSecond > 0f)
            {
                _staminaCompo.ChangeValueWithTimer(-(staminaUsePerSecond * Time.deltaTime), Time.deltaTime);
                if (_staminaCompo.CurrentValue <= 0f)
                {
                    _isExhausted = true;
                    return NavMoveType.Walk;
                }
            }

            return NavMoveType.Sprint;
        }

        public void OnLocalEvent(EnemySpawnEvent eventData)
        {
            _isExhausted = false;
        }

        private float GetRecoverThreshold() => _staminaCompo.MaxValue * sprintRecoverRatio;
    }
}
