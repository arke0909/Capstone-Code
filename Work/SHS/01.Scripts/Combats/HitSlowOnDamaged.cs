using System.Collections.Generic;
using Assets.Work.AKH.Scripts.Entities.Vitals;
using Chipmunk.ComponentContainers;
using Chipmunk.Modules.StatSystem;
using Code.StatusEffectSystem;
using Scripts.Combat;
using Scripts.Combat.Datas;
using UnityEngine;

namespace SHS.Scripts.Combats
{
    public class HitSlowOnDamaged : MonoBehaviour, IContainerComponent, IAfterInitialze
    {
        [SerializeField] private BuffSO hitSlowBuff;
        [SerializeField] private AbstractStatusEffectDataSO slowStatusEffectData;
        [SerializeField] private StatSO slowResistanceStat;

        private HealthCompo _healthCompo;
        private EntityStatusEffect _statusEffect;
        private StatOverrideBehavior _statOverrideBehavior;
        private StatSO _slowResistance;

        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _healthCompo = componentContainer.Get<HealthCompo>();
            _statusEffect = componentContainer.Get<EntityStatusEffect>();
            _statOverrideBehavior = componentContainer.Get<StatOverrideBehavior>();

            Debug.Assert(_healthCompo != null, "HitSlowOnDamaged requires HealthCompo.", this);
            Debug.Assert(_statusEffect != null, "HitSlowOnDamaged requires EntityStatusEffect.", this);
            Debug.Assert(_statOverrideBehavior != null, "HitSlowOnDamaged requires StatOverrideBehavior.", this);
            Debug.Assert(hitSlowBuff != null, "HitSlowOnDamaged requires hitSlowBuff.", this);
            Debug.Assert(slowStatusEffectData != null,
                "HitSlowOnDamaged requires slowStatusEffectData.", this);
            Debug.Assert(slowResistanceStat != null, "HitSlowOnDamaged requires slowResistanceStat.", this);
        }

        public void AfterInitialize()
        {
            _slowResistance = _statOverrideBehavior.GetStat(slowResistanceStat);
            Debug.Assert(_slowResistance != null, "HitSlowOnDamaged requires registered slowResistanceStat.", this);

            _healthCompo.OnHit += HandleHit;
        }

        private void OnDestroy()
        {
            if (_healthCompo != null)
                _healthCompo.OnHit -= HandleHit;
        }

        private void HandleHit(DamageContext context)
        {
            if ((context.DamageData.damageType & DamageType.DOT) != 0)
                return;

            float slowScale = 1f - Mathf.Clamp01(_slowResistance.Value);
            List<StatusEffectInfo> infos = hitSlowBuff.GetStatusEffectInfo();
            Debug.Assert(infos.Count > 0,
                "HitSlowOnDamaged requires at least one status effect in hitSlowBuff.", this);
            if (infos.Count == 0)
                return;

            bool foundSlowStatusEffect = false;

            for (int i = 0; i < infos.Count; i++)
            {
                StatusEffectInfo info = infos[i];
                if (info.StatusEffectData == slowStatusEffectData)
                {
                    info.Value *= slowScale;
                    foundSlowStatusEffect = true;
                }

                infos[i] = info;
            }

            if (!foundSlowStatusEffect)
            {
                Debug.LogError(
                    $"{hitSlowBuff.name} does not contain {slowStatusEffectData.name}.",
                    this);
                return;
            }

            _statusEffect.AddStatusEffect(infos, this);
        }
    }
}
