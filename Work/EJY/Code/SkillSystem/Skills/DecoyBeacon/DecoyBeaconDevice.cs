using System.Collections.Generic;
using Code.SHS.Targetings.Enemies;
using Code.StatusEffectSystem;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Entities;
using SHS.Scripts.NoiseSystems;
using UnityEngine;

namespace Code.SkillSystem.Skills.DecoyBeacon
{
    public class DecoyBeaconDevice : Entity, IPoolable, ITauntTarget
    {
        [Header("References")]
        [SerializeField] private PoolItemSO poolItem;
        [SerializeField] private NoiseGenerator noiseGenerator;
        [SerializeField] private ParticleSystem pulseEffect;
        [SerializeField] private ParticleSystem finalBurstEffect;

        private readonly Collider[] _overlapResults = new Collider[32];
        private readonly HashSet<TargetProvider> _tauntedProviders = new HashSet<TargetProvider>();

        private Entity _owner;
        private float _activationDelay;
        private float _pulseInterval;
        private float _noiseRadius;
        private float _phantomBurstRadiusMultiplier;
        private int _pulseCount;
        private bool _phantomProtocol;
        private LayerMask _enemyLayerMask;
        private BuffSO _phantomProtocolBuff;
        private float _activationTimer;
        private float _pulseTimer;
        private int _emittedPulseCount;
        private bool _isActivated;
        private bool _isRunning;
        private Pool _myPool;

        public PoolItemSO PoolItem => poolItem;
        public GameObject GameObject => gameObject;
        public bool IsTauntTargetActive => isActiveAndEnabled && gameObject.activeInHierarchy;

        private void Awake()
        {
            if (noiseGenerator == null)
                noiseGenerator = GetComponent<NoiseGenerator>();
        }

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        private void Update()
        {
            if (_isRunning == false)
                return;

            if (_isActivated == false)
            {
                _activationTimer -= Time.deltaTime;
                if (_activationTimer > 0f)
                    return;

                _isActivated = true;
                EmitCurrentPulse();
                return;
            }

            _pulseTimer -= Time.deltaTime;
            if (_pulseTimer <= 0f)
            {
                EmitCurrentPulse();
            }
        }

        public void Initialize(
            Entity owner,
            float activationDelay,
            float pulseInterval,
            int pulseCount,
            float noiseRadius,
            LayerMask enemyLayerMask,
            bool phantomProtocol,
            float phantomBurstRadiusMultiplier,
            BuffSO phantomProtocolBuff)
        {
            ClearTauntedTargets();

            _owner = owner;
            _activationDelay = Mathf.Max(0f, activationDelay);
            _pulseInterval = Mathf.Max(0.05f, pulseInterval);
            _pulseCount = Mathf.Max(1, pulseCount);
            _noiseRadius = Mathf.Max(0.1f, noiseRadius);
            _enemyLayerMask = enemyLayerMask;
            _phantomProtocol = phantomProtocol;
            _phantomBurstRadiusMultiplier = Mathf.Max(1f, phantomBurstRadiusMultiplier);
            _phantomProtocolBuff = phantomProtocolBuff;
            _activationTimer = _activationDelay;
            _pulseTimer = 0f;
            _emittedPulseCount = 0;
            _isActivated = _activationDelay <= 0f;
            _isRunning = true;

            StopEffect(pulseEffect);
            StopEffect(finalBurstEffect);

            if (_isActivated)
            {
                EmitCurrentPulse();
            }
        }

        private void EmitCurrentPulse()
        {
            bool isFinalPulse = _emittedPulseCount == _pulseCount - 1;
            EmitPulse(isFinalPulse);
            _emittedPulseCount++;

            if (isFinalPulse)
            {
                _isRunning = false;
                ReturnToPool();
                return;
            }

            _pulseTimer = _pulseInterval;
        }

        private float EmitPulse(bool isFinalPulse)
        {
            if (_owner == null || noiseGenerator == null)
                return 0f;

            float pulseRadius = isFinalPulse && _phantomProtocol
                ? _noiseRadius * _phantomBurstRadiusMultiplier
                : _noiseRadius;

            noiseGenerator.GenerateNoise(_owner, pulseRadius);
            TauntEnemies(pulseRadius);
            float effectDuration = PlayEffect(pulseEffect);

            if (isFinalPulse == false || _phantomProtocol == false)
                return effectDuration;

            effectDuration = Mathf.Max(effectDuration, PlayEffect(finalBurstEffect));
            ApplyFinalBurstBuff(pulseRadius);
            return effectDuration;
        }

        private static float PlayEffect(ParticleSystem effect)
        {
            if (effect == null)
                return 0f;

            effect.Stop(true);
            effect.Play(true);
            return GetParticleLifetime(effect);
        }

        private static void StopEffect(ParticleSystem effect)
        {
            if (effect == null)
                return;

            effect.Stop(true);
        }

        private static float GetParticleLifetime(ParticleSystem particleSystem)
        {
            ParticleSystem.MainModule main = particleSystem.main;
            float duration = main.duration + main.startDelay.constantMax + main.startLifetime.constantMax;

            if (main.loop)
                duration = Mathf.Max(duration, 5f);

            return Mathf.Max(0.5f, duration);
        }

        private void TauntEnemies(float pulseRadius)
        {
            int count = Physics.OverlapSphereNonAlloc(
                transform.position,
                pulseRadius,
                _overlapResults,
                _enemyLayerMask);

            HashSet<TargetProvider> handledProviders = new HashSet<TargetProvider>();

            for (int i = 0; i < count; i++)
            {
                Collider targetCollider = _overlapResults[i];
                if (targetCollider == null)
                    continue;

                TargetProvider provider = targetCollider.GetComponentInParent<TargetProvider>();
                if (provider == null || handledProviders.Add(provider) == false)
                    continue;

                provider.SetTarget(this);
                _tauntedProviders.Add(provider);
            }
        }

        private void ApplyFinalBurstBuff(float pulseRadius)
        {
            if (_phantomProtocolBuff == null)
                return;

            int count = Physics.OverlapSphereNonAlloc(
                transform.position,
                pulseRadius,
                _overlapResults,
                _enemyLayerMask);

            HashSet<EntityStatusEffect> handledEffects = new HashSet<EntityStatusEffect>();

            for (int i = 0; i < count; i++)
            {
                Collider targetCollider = _overlapResults[i];
                if (targetCollider == null)
                    continue;

                EntityStatusEffect statusEffect = targetCollider.GetComponentInParent<EntityStatusEffect>();
                if (statusEffect == null || handledEffects.Add(statusEffect) == false)
                    continue;

                statusEffect.AddStatusEffect(_phantomProtocolBuff.GetStatusEffectInfo());
            }
        }

        public void ResetItem()
        {
            ClearTauntedTargets();

            _owner = null;
            _activationDelay = 0f;
            _pulseInterval = 0f;
            _noiseRadius = 0f;
            _phantomBurstRadiusMultiplier = 1f;
            _pulseCount = 0;
            _phantomProtocol = false;
            _enemyLayerMask = 0;
            _phantomProtocolBuff = null;
            _activationTimer = 0f;
            _pulseTimer = 0f;
            _emittedPulseCount = 0;
            _isActivated = false;
            _isRunning = false;

            StopEffect(pulseEffect);
            StopEffect(finalBurstEffect);
        }

        private void ReturnToPool()
        {
            ClearTauntedTargets();
            StopEffect(pulseEffect);
            StopEffect(finalBurstEffect);

            if (_myPool != null)
            {
                _myPool.Push(this);
                return;
            }

            Destroy(gameObject);
        }

        private void ClearTauntedTargets()
        {
            if (_tauntedProviders.Count == 0)
                return;

            Vector3 lastKnownPosition = transform.position;
            foreach (TargetProvider provider in _tauntedProviders)
            {
                if (provider == null)
                    continue;

                if (ReferenceEquals(provider.CurrentTarget, this) == false &&
                    ReferenceEquals(provider.Target, this) == false)
                    continue;

                provider.ResetTargetState(lastKnownPosition);
                provider.TargetLost(lastKnownPosition);
            }

            _tauntedProviders.Clear();
        }
    }
}
