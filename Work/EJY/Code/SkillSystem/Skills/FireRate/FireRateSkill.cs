using Ami.BroAudio;
using Chipmunk.ComponentContainers;
using Chipmunk.Modules.StatSystem;
using Code.StatusEffectSystem;
using Code.StatusEffectSystem.StatusEffects;
using Entities;
using Scripts.Combat;
using Scripts.Entities;
using Scripts.SkillSystem;
using UnityEngine;

namespace Code.SkillSystem.Skills.FireRate
{
    public class FireRateSkill : ActiveSkill
    {
        [SerializeField] private BuffSO fireRateBuffSO;
        [SerializeField] private AbstractStatusEffectDataSO fireRateStatusEffectData;
        [SerializeField] private BuffSO bulletReduceRateBuff;
        [SerializeField] private StatSO fireRateStatSO;
        [SerializeField] private FireRateSkillVFX fireRateSkillVFX;
        [SerializeField] private SoundID soundID;
        [SerializeField] private bool isOnHitAddFireRate;
        [SerializeField] private bool isBulletReduceRateDecrease;
        [SerializeField] private float onHitFireRateAmount = 0.025f, maxFireRate = 0.5f;

        private EntityStatusEffect _entityStatusEffect;
        private StatOverrideBehavior _stat;
        private StatusEffectLayer _statusEffectLayer;
        private float _totalFireRate;
        private bool _applied;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _entityStatusEffect = container.Get<EntityStatusEffect>();
            _stat = container.Get<StatOverrideBehavior>();

            fireRateSkillVFX.InitVFXCompo(_owner);
        }

        private void OnDestroy()
        {
            if (_entityStatusEffect != null)
            {
                _entityStatusEffect.OnStatusEffectLayerReleased -= HandleFireRateReleased;
                _entityStatusEffect.RemoveStatusEffect(fireRateBuffSO, this);
                _entityStatusEffect.RemoveStatusEffect(bulletReduceRateBuff, this);
            }

            if (_owner != null)
                _owner.OnAttack -= OnHitAddFireRate;

            _stat?.GetStat(fireRateStatSO)?.RemoveModifier(this);
            _applied = false;
            _statusEffectLayer = null;
            _totalFireRate = 0f;

            if (fireRateSkillVFX != null)
            {
                fireRateSkillVFX.ResetHeatRatio();
                fireRateSkillVFX.StopMuzzleSmog();
            }
        }

        private void UpgradeOnHitAddFireRate() => isOnHitAddFireRate = true;
        private void RollbackOnHitAddFireRate() => isOnHitAddFireRate = false;
        private void UpgradeBulletReduceRateDecrease() => isBulletReduceRateDecrease = true;
        private void RollbackBulletReduceRateDecrease() => isBulletReduceRateDecrease = false;

        public override void OnSkillTrigger()
        {
            if (_applied) return;

            Debug.Assert(fireRateBuffSO != null,
                $"{nameof(FireRateSkill)} requires {nameof(fireRateBuffSO)}.", this);
            Debug.Assert(fireRateStatusEffectData != null,
                $"{nameof(FireRateSkill)} requires {nameof(fireRateStatusEffectData)}.", this);
            if (fireRateBuffSO == null || fireRateStatusEffectData == null)
                return;

            _entityStatusEffect.AddStatusEffect(fireRateBuffSO, this);

            if (!_entityStatusEffect.TryGetLayer(fireRateBuffSO, out StatusEffectLayer layer) ||
                !layer.TryGetStatusEffect(fireRateStatusEffectData, out _))
            {
                Debug.LogWarning($"{nameof(FireRateSkill)} failed to apply fire rate buff.", this);
                _entityStatusEffect.RemoveStatusEffect(fireRateBuffSO, this);
                return;
            }

            _statusEffectLayer = layer;

            if (isBulletReduceRateDecrease)
            {
                Debug.Assert(bulletReduceRateBuff != null,
                    $"{nameof(FireRateSkill)} requires {nameof(bulletReduceRateBuff)} when {nameof(isBulletReduceRateDecrease)} is enabled.", this);

                if (bulletReduceRateBuff != null)
                    _entityStatusEffect.AddStatusEffect(bulletReduceRateBuff, this);
            }

            BroAudio.Play(soundID, _owner.transform.position);

            _applied = true;
            fireRateSkillVFX.PlayMuzzleSmog();

            if (isOnHitAddFireRate)
                _owner.OnAttack += OnHitAddFireRate;

            _entityStatusEffect.OnStatusEffectLayerReleased += HandleFireRateReleased;
        }

        private void HandleFireRateReleased(StatusEffectLayer layer)
        {
            if (_statusEffectLayer != layer)
                return;

            _applied = false;
            _statusEffectLayer = null;

            if (bulletReduceRateBuff != null)
                _entityStatusEffect.RemoveStatusEffect(bulletReduceRateBuff, this);

            _owner.OnAttack -= OnHitAddFireRate;
            _totalFireRate = 0f;
            _stat.GetStat(fireRateStatSO).RemoveModifier(this);

            fireRateSkillVFX.ResetHeatRatio();
            fireRateSkillVFX.StopMuzzleSmog();
            _entityStatusEffect.OnStatusEffectLayerReleased -= HandleFireRateReleased;
        }

        private void OnHitAddFireRate(Entity dealer, IDamageable target)
        {
            if (!_applied)
                return;

            if (Mathf.Approximately(_totalFireRate, maxFireRate))
                return;

            _totalFireRate = Mathf.Min(_totalFireRate + onHitFireRateAmount, maxFireRate);
            fireRateSkillVFX.SetHeatRatio(_totalFireRate / maxFireRate);

            var targetStat = _stat.GetStat(fireRateStatSO);
            targetStat.RemoveModifier(this);
            targetStat.AddValueModifier(this, -_totalFireRate);
        }
    }
}
