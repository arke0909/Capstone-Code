using Chipmunk.ComponentContainers;

using Code.StatusEffectSystem;

using Entities;
using Scripts.Combat;
using Scripts.Entities;
using Scripts.FSM;
using UnityEngine;

namespace Scripts.SkillSystem.Skills
{
    public class AdrenalineSkill : ActiveSkill
    {
        [SerializeField] private BuffSO adrenalineData;
        [SerializeField] private float additionalTime = 0.3f;
        [SerializeField] private bool addReloadSpeed;
        [SerializeField] private bool getAdditionalTime;
        [SerializeField] private BuffSO reloadSpeedData;

        private EntityStatusEffect _buffCompo;
        private StatusEffectLayer _adrenalineLayer;
        private bool _isBuffActive;
        private int _buffLevel;
        private int _reloadLevel;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _buffCompo = container.Get<EntityStatusEffect>();
        }


        private void StartBuff()
        {
            _buffCompo.AddStatusEffect(adrenalineData, this, _buffLevel);

            if (!_buffCompo.TryGetLayer(adrenalineData, out _adrenalineLayer))
            {
                Debug.LogWarning($"{nameof(AdrenalineSkill)} failed to apply adrenaline buff.", this);
                return;
            }

            _isBuffActive = true;
            _buffCompo.OnStatusEffectLayerReleased += HandleAdrenalineReleased;

            if (getAdditionalTime)
                _owner.OnAttack += OnHitTarget;
        }

        private void AddReloadSpeed()
        {
            _buffCompo.AddStatusEffect(reloadSpeedData, this, _reloadLevel);
        }

        private void HandleAdrenalineReleased(StatusEffectLayer layer)
        {
            if (_adrenalineLayer != layer)
                return;

            _adrenalineLayer = null;
            _isBuffActive = false;
            _owner.OnAttack -= OnHitTarget;
            _buffCompo.OnStatusEffectLayerReleased -= HandleAdrenalineReleased;
        }

         private void OnHitTarget(Entity dealer, IDamageable target)
         {
             if (!_isBuffActive) return;

             _adrenalineLayer?.ExtendDuration(additionalTime);
         }

        private void OnDestroy()
        {
            if (_buffCompo != null)
            {
                _buffCompo.OnStatusEffectLayerReleased -= HandleAdrenalineReleased;
                _buffCompo.RemoveStatusEffect(adrenalineData, this);
                _buffCompo.RemoveStatusEffect(reloadSpeedData, this);
            }

            if (_owner != null)
                _owner.OnAttack -= OnHitTarget;
        }

        public override void OnSkillTrigger()
        {
            if (_isBuffActive)
                return;

            StartBuff();

            if (_isBuffActive && addReloadSpeed)
                AddReloadSpeed();
        }
    }
}
