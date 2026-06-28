using Chipmunk.ComponentContainers;
using Code.ETC;
using Code.StatusEffectSystem;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Entities;
using Scripts.FSM;
using Scripts.SkillSystem;
using Scripts.SkillSystem.Skills;
using UnityEngine;

namespace Code.SkillSystem.Skills.DecoyBeacon
{
    public class DecoyBeaconSkill : ActiveSkill, IAimSkill
    {
        public PlayerInputSO playerInput;
        [SerializeField] private DecalObject decalObject;

        [Header("Beacon")]
        [SerializeField] private PoolItemSO beaconPoolItem;
        [SerializeField] private LayerMask enemyLayerMask = ~0;
        [SerializeField] private float maxCastRange = 16f;
        [SerializeField] private float activationDelay = 0.35f;
        [SerializeField] private float pulseInterval = 0.8f;
        [SerializeField] private int pulseCount = 3;
        [SerializeField] private float noiseRadius = 10f;

        [SerializeField] private int amplifiedPulseBonus = 1;
        [SerializeField] private float amplifiedRadiusBonus = 2f;
        [SerializeField] private bool amplifiedSignal;

        [SerializeField] private float phantomBurstRadiusMultiplier = 1.5f;
        [SerializeField] private BuffSO phantomProtocolBuff;
        [SerializeField] private bool phantomProtocol;

        [Inject] private PoolManagerMono _poolManager;
        private IAimProvider _aimProvider;
        private Vector3 _cachedAimPosition;
        private Vector3 _castPosition;
        private bool _isAiming;

        public override bool CanUseSkill()
            => base.CanUseSkill() && beaconPoolItem != null && _poolManager != null && _aimProvider != null;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _aimProvider = container.GetSubclassComponent<IAimProvider>();
            _cachedAimPosition = _owner != null ? _owner.transform.position : transform.position;
            _castPosition = _cachedAimPosition;
        }

        public void StartAiming()
        {
            _isAiming = true;
            _cachedAimPosition = _aimProvider.GetWorldAimPosition();

            if (decalObject != null)
            {
                decalObject.SetParent(null);
                decalObject.SetActive(true);
                decalObject.SetPos(_cachedAimPosition);
            }
        }

        public void CancelSkill()
        {
            _isAiming = false;
            RestoreDecalParent();
        }

        public override void StartSkill()
        {
            _castPosition = _aimProvider.GetWorldAimPosition();
            _isAiming = false;
            RestoreDecalParent();
        }

        public override void OnSkillTrigger()
        {
            if (beaconPoolItem == null || _poolManager == null)
                return;

            DecoyBeaconDevice beacon = _poolManager.Pop<DecoyBeaconDevice>(beaconPoolItem);
            if (beacon == null)
                return;

            beacon.transform.SetPositionAndRotation(_castPosition, Quaternion.identity);

            beacon.Initialize(
                _owner,
                activationDelay,
                pulseInterval,
                GetPulseCount(),
                GetNoiseRadius(),
                enemyLayerMask,
                phantomProtocol,
                phantomBurstRadiusMultiplier,
                phantomProtocolBuff);
        }

        private void Update()
        {
            if (_isAiming == false)
                return;

            _cachedAimPosition = _aimProvider.GetWorldAimPosition();
            if (decalObject != null)
                decalObject.SetPos(_cachedAimPosition);
        }

        private int GetPulseCount()
        {
            int finalPulseCount = pulseCount;
            if (amplifiedSignal)
                finalPulseCount += amplifiedPulseBonus;

            return Mathf.Max(1, finalPulseCount);
        }

        private float GetNoiseRadius()
        {
            float finalNoiseRadius = noiseRadius;
            if (amplifiedSignal)
                finalNoiseRadius += amplifiedRadiusBonus;

            return Mathf.Max(0.1f, finalNoiseRadius);
        }

        private void RestoreDecalParent()
        {
            if (decalObject == null)
                return;

            decalObject.SetActive(false);
            decalObject.SetParent(transform);
        }

        private void UpgradeAmplifiedSignal() => amplifiedSignal = true;
        private void RollbackAmplifiedSignal() => amplifiedSignal = false;

        private void UpgradePhantomProtocol() => phantomProtocol = true;
        private void RollbackPhantomProtocol() => phantomProtocol = false;
    }
}
