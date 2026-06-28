using System;
using Ami.BroAudio;
using Chipmunk.ComponentContainers;
using Chipmunk.Modules.StatSystem;
using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;

namespace Scripts.Combat.Areas
{
    public class DealingArea : Area
    {
        [SerializeField] private float damage = 1f;
        [SerializeField] private DamageCaster overlapDamageCaster;
        [SerializeField] private StatSO damageModifyStat;
        [SerializeField] private SoundID areaSound;

        private DamageData _damageData;
        public override void Init(Entity owner, Vector3 position)
        {
            base.Init(owner, position);
            overlapDamageCaster.InitCaster(_owner);

            float damageModify = _owner.Get<StatOverrideBehavior>().GetStat(damageModifyStat).Value;
            _damageData = _owner.Get<DamageCalcCompo>().CalculateDamage(damage, damageModify, 1, DamageType.DOT);
            BroAudio.Play(areaSound, transform.position);
        }

        private void OnDisable()
        {
            BroAudio.Stop(areaSound);
        }

        protected override void TickElapsed()
        {
            overlapDamageCaster.CastDamage(_damageData, transform.position, -transform.up, null);
        }
        public override void ResetItem()
        {
            base.ResetItem();
            _damageData = default;
        }
    }
}