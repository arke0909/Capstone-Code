using Chipmunk.ComponentContainers;
using Chipmunk.Modules.StatSystem;
using Cysharp.Threading.Tasks;
using Entities;
using Scripts.Entities;
using UnityEngine;

namespace Scripts.SkillSystem.PassiveSkills
{
    public class AssaultPassiveSkill : PassiveSkill,IAfterInitialze
    {
        [SerializeField] private StatSO damageDemodifyStat;
        [SerializeField] private float shieldCooldown;
        [SerializeField] private float shieldDuration;
        [SerializeField] private float damageDemodifyValue;
        private StatOverrideBehavior _statOverrideBehavior;
        private VFXComponent _vfxCompo;
        private float _cooldownTimer;


        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _statOverrideBehavior = _owner.Get<StatOverrideBehavior>();
            _vfxCompo = _owner.Get<VFXComponent>();
        }
        public void AfterInitialize()
        {
            damageDemodifyStat = _statOverrideBehavior.GetStat(damageDemodifyStat);
        }
        public override void EnableSkill()
        {
            base.EnableSkill();
            EnableShield();
            _owner.OnHitEvent.AddListener(HandleOwnerHit);
        }
        private void EnableShield()
        {
            damageDemodifyStat.AddValueModifier("AssaultPassive", -damageDemodifyValue);
            _vfxCompo.PlayVFX("HolyShield", transform.position, Quaternion.identity);
        }
        private void Update()
        {
            if (_cooldownTimer <= 0) return;

            _cooldownTimer -= Time.deltaTime;
            if (_cooldownTimer < 0)
            {
                _cooldownTimer = 0;
                EnableShield();
            }
        }
        private async void HandleOwnerHit()
        {
            if (_cooldownTimer > 0f || !Enabled)
                return;
            await UniTask.WaitForSeconds(shieldDuration);
            damageDemodifyStat.RemoveModifier("AssaultPassive");
            _vfxCompo.StopVFX("HolyShield");
            _cooldownTimer = shieldCooldown;
        }

        public override void DisableSkill()
        {
            HandleOwnerHit();
            _owner.OnHitEvent.RemoveListener(HandleOwnerHit);
            base.DisableSkill();
        }


    }
}

