using Chipmunk.Modules.StatSystem;
using Scripts.Entities;
using UnityEngine;

namespace Code.StatusEffectSystem.StatusEffects
{
    public class StatStatusEffect : AbstractStatusEffect
    {
        protected StatOverrideBehavior _targetStat;
        protected StatSO _targetStatSO;
        private float _configuredValue;
        private bool _isPercent;
        private bool _useCurrentValuePercent;

        public StatStatusEffect(
            Entity target,
            StatusEffectInfo statusEffectInfo,
            StatSO statSO,
            bool useCurrentValuePercent = false) : base(target, statusEffectInfo)
        {
            _targetStat = target.ComponentContainer.Get<StatOverrideBehavior>();
            _targetStatSO = statSO;
            _configuredValue = statusEffectInfo.Value;
            _isPercent = statusEffectInfo.IsPercent;
            _useCurrentValuePercent = useCurrentValuePercent;

            Debug.Assert(_targetStat != null, "Target has not stat component");
        }

        public override void ApplyStatusEffect(Entity entity)
        {
            if (_targetStatSO == null)
            {
                Debug.Log("Target stat is null");
                _isApplying = false;
                return;
            }

            StatSO targetStat = _targetStat.GetStat(_targetStatSO);
            if (targetStat == null)
            {
                Debug.LogError($"{entity.name} does not have {_targetStatSO.name} registered.");
                _isApplying = false;
                return;
            }

            SetValue(CalculateAppliedValue(targetStat));
            base.ApplyStatusEffect(entity);
            targetStat.AddValueModifier(this, _value);
        }

        public override void ReleaseStatusEffect(Entity entity)
        {
            _isApplying = false;

            if (_targetStatSO == null)
            {
                Debug.Log("Target stat is null");
                _isApplying = false;
                return;
            }

            _targetStat.GetStat(_targetStatSO).RemoveModifier(this);
        }

        public override void SetStrongerValue(StatusEffectInfo info)
        {
            if (!_useCurrentValuePercent)
            {
                base.SetStrongerValue(info);
                return;
            }

            StatSO targetStat = _targetStat.GetStat(_targetStatSO);
            targetStat.RemoveModifier(this);

            Priority = info.Priority;
            _configuredValue = info.Value;
            _isPercent = info.IsPercent;
            _baseValue = CalculateAppliedValue(targetStat);
            _value = CalculateStackedValue();
            SetRemainingTime(info.ApplyTime);

            targetStat.AddValueModifier(this, _value);
        }

        protected override void OnValueChanged()
        {
            if (!_isApplying || _targetStatSO == null || _targetStat == null)
                return;

            var targetStat = _targetStat.GetStat(_targetStatSO);
            targetStat.RemoveModifier(this);
            targetStat.AddValueModifier(this, _value);
        }

        private float CalculateAppliedValue(StatSO targetStat)
        {
            if (_useCurrentValuePercent && _isPercent)
                return _configuredValue * targetStat.Value;

            return _configuredValue;
        }
    }
}
