using Chipmunk.ComponentContainers;
using System;
using UnityEngine;

namespace Scripts.Entities
{
    public class EntityAnimatorTrigger : MonoBehaviour, IContainerComponent
    {
        public Action OnAnimationEndTrigger;
        public Action OnAttackVFXTrigger;
        public Action<bool> OnManualRotationTrigger;
        public Action OnDamageCastTrigger;
        public Action<bool> OnDamageToggleTrigger;
        public Action OnCastSkillTrigger;
        private Entity _entity;

        public ComponentContainer ComponentContainer { get; set; }

        public void Initialize(Entity entity)
        {
            _entity = entity;
        }

        private void AnimationEnd() //매서드 명 오타나면 안된다. (이벤트 이름과 동일하게 만들어야 해.)
        {
            OnAnimationEndTrigger?.Invoke();
        }
        private void CastSkill() => OnCastSkillTrigger?.Invoke();
        private void PlayAttackVFX() => OnAttackVFXTrigger?.Invoke();
        private void StartManualRotation() => OnManualRotationTrigger?.Invoke(true);
        private void StopManualRotation() => OnManualRotationTrigger?.Invoke(false);
        private void DamageCast() => OnDamageCastTrigger?.Invoke();
        private void StartDamageCast() => OnDamageToggleTrigger?.Invoke(true);
        private void StopDamageCast() => OnDamageToggleTrigger?.Invoke(false);

        public void OnInitialize(ComponentContainer componentContainer)
        {
        }
    }
}