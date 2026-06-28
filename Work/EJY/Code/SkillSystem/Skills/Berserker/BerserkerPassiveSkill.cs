using Assets.Work.AKH.Scripts.Entities.Vitals;
using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Chipmunk.Modules.StatSystem;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities.Vitals;
using Scripts.SkillSystem;
using UnityEngine;

namespace Code.SkillSystem.Skills.Berserker
{
    public class BerserkerPassiveSkill : PassiveSkill
    {
        [SerializeField] private StatSO damageModify ,damageDemodefy, defStat;
        [SerializeField, Range(0f, 0.9f)] private float calcHealthRatio = 0.7f;
        [SerializeField] private float damageDecrease = 0.7f;
        [SerializeField] private float damageIncrease = 0.8f;
        [SerializeField] private float immortalDuration = 2f;
        [SerializeField] private float immortalCooldown = 180f;

        private HealthCompo _healthCompo;
        private StatOverrideBehavior _statOverrideBehavior;
        private LocalEventBus _localEventBus;
        private ShieldCompo _shieldCompo;
        private StatSO _runtimeDamageDemodifyStat;
        private StatSO _runtimeDefStat;
        private bool _isDamageDecrease;
        private bool _isImmortality;
        private bool _isImmortalActive;
        private bool _isBeforeHitSubscribed;
        private float _immortalEndTime;
        private float _nextImmortalAvailableTime;
        
        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _healthCompo = container.Get<HealthCompo>();
            _statOverrideBehavior = container.Get<StatOverrideBehavior>();
            _localEventBus = container.Get<LocalEventBus>();
            _shieldCompo = container.Get<ShieldCompo>();

            _runtimeDamageDemodifyStat = _statOverrideBehavior.GetStat(damageDemodefy);
            _runtimeDefStat = _statOverrideBehavior.GetStat(defStat);
            _nextImmortalAvailableTime = 0f;
        }

        private void UpgradeDamageDecrease()
        {
            _isDamageDecrease = true;
        }

        private void RollbackDamageDecrease()
        {
            _isDamageDecrease = false;
            _statOverrideBehavior.RemoveModifier(damageDemodefy, this);
        }
        
        private void UpgradeImmortality()
        {
            _isImmortality = true;
            RefreshBeforeHitSubscription();
        }

        private void RollbackImmortality()
        {
            _isImmortality = false;
            _isImmortalActive = false;
            RefreshBeforeHitSubscription();
        }

        private bool BeforeHit(DamageContext context)
        {
            if (!_isImmortality || !Enabled || _runtimeDamageDemodifyStat == null || _runtimeDefStat == null)
                return false;

            if (_isImmortalActive && Time.time >= _immortalEndTime)
            {
                _isImmortalActive = false;
            }

            float remainHealth = _healthCompo.CurrentValue - CalculateFinalDamage(context.DamageData);

            // While immortality is active, clamp any hit that would drop HP below 1.
            if (_isImmortalActive)
            {
                if (remainHealth > 1f)
                    return false;

                _healthCompo.CurrentValue = 1f;
                return true;
            }

            bool cooldownReady = Time.time >= _nextImmortalAvailableTime;
            bool lethalHit = remainHealth <= 0f;

            if (!cooldownReady || !lethalHit || _healthCompo.CurrentValue <= 1f)
                return false;

            _isImmortalActive = true;
            _immortalEndTime = Time.time + immortalDuration;
            _nextImmortalAvailableTime = Time.time + immortalCooldown;
            _healthCompo.CurrentValue = 1f;
            return true;
        }

        private void HandleHealthChange(HealthChangeEvent evt)
        {
            // 잃은 체력 비 구하기
            float healthRatio = 1 - (evt.CurrentHealth / evt.MaxHealth);

            healthRatio = Mathf.Clamp(healthRatio, 0f, calcHealthRatio);

            float applyBuffRatio = healthRatio / calcHealthRatio;

            _statOverrideBehavior.RemoveModifier(damageModify, this);
            _statOverrideBehavior.AddModifier(damageModify, this, applyBuffRatio * damageIncrease);

            if (_isDamageDecrease)
            {
                _statOverrideBehavior.RemoveModifier(damageDemodefy, this);
                _statOverrideBehavior.AddModifier(damageDemodefy, this, applyBuffRatio * damageDecrease);
            }
        }

        private float CalculateFinalDamage(DamageData damageData)
        {
            float finalDefModify = 2f / (Mathf.Max(_runtimeDefStat.Value - damageData.defPierceLevel, 0f) + 2f);
            float finalDamage = damageData.damage * finalDefModify * _runtimeDamageDemodifyStat.Value;

            if (_shieldCompo != null)
            {
                finalDamage = Mathf.Max(finalDamage - _shieldCompo.CurrentShieldAmount, 0f);
            }

            return finalDamage;
        }

        private void RefreshBeforeHitSubscription()
        {
            bool shouldSubscribe = Enabled && _isImmortality;

            if (shouldSubscribe && !_isBeforeHitSubscribed)
            {
                _healthCompo.OnBeforeHit += BeforeHit;
                _isBeforeHitSubscribed = true;
            }
            else if (!shouldSubscribe && _isBeforeHitSubscribed)
            {
                _healthCompo.OnBeforeHit -= BeforeHit;
                _isBeforeHitSubscribed = false;
            }
        }

        public override void EnableSkill()
        {
            base.EnableSkill();
            _localEventBus.Subscribe<HealthChangeEvent>(HandleHealthChange);
            RefreshBeforeHitSubscription();
        }

        public override void DisableSkill()
        {
            _localEventBus.Unsubscribe<HealthChangeEvent>(HandleHealthChange);
            _isImmortalActive = false;
            RefreshBeforeHitSubscription();
            _statOverrideBehavior.RemoveModifier(damageModify, this);
            _statOverrideBehavior.RemoveModifier(damageDemodefy, this);
            base.DisableSkill();
        }
    }
}
