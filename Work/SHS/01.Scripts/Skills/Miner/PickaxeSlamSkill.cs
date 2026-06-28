using Chipmunk.ComponentContainers;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.SkillSystem;
using Scripts.SkillSystem.Skills;
using UnityEngine;

namespace SHS.Scripts.Skills.Miner
{
    public class PickaxeSlamSkill : ActiveSkill
    {
        [Header("Required")]
        [SerializeField] private OverlapDamageCaster damageCaster;

        [Header("Visual")]
        [SerializeField] private GameObject pickaxeVisualPrefab;
        [SerializeField] private Vector3 visualLocalPosition;
        [SerializeField] private Vector3 visualLocalEulerAngles;
        [SerializeField] private Vector3 visualLocalScale = Vector3.one;

        [Header("Damage")]
        [SerializeField] private float defaultDamage = 18f;
        [SerializeField] private float damageMultiplier = 1f;
        [SerializeField] private int defPierceLevel = 1;

        private DamageCalcCompo _damageCalcCompo;
        private GameObject _pickaxeVisualInstance;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _damageCalcCompo = container.GetSubclassComponent<DamageCalcCompo>();
            PickaxeVisualSocket visualSocket = container.GetSubclassComponent<PickaxeVisualSocket>();
            AssertRequiredSettings();
            Debug.Assert(visualSocket != null, $"{nameof(PickaxeSlamSkill)} requires owner {nameof(PickaxeVisualSocket)}.", this);

            damageCaster.InitCaster(_owner);
            CreatePickaxeVisual(visualSocket.Socket);
        }

        public override void StartSkill()
        {
            base.StartSkill();
            _pickaxeVisualInstance.SetActive(true);
        }

        public override void EndSkill()
        {
            base.EndSkill();
            _pickaxeVisualInstance.SetActive(false);
        }

        public override void OnSkillTrigger()
        {
            DamageData damageData = _damageCalcCompo.CalculateDamage(
                defaultDamage,
                damageMultiplier,
                defPierceLevel,
                DamageType.MELEE);

            damageCaster.CastDamage(
                damageData,
                damageCaster.transform.position,
                damageCaster.transform.forward,
                null);
        }

        private void Reset()
        {
            AnimType = SkillAnimType.PickaxeSlam;
            visualLocalScale = Vector3.one;
        }

        private void OnValidate()
        {
            AnimType = SkillAnimType.PickaxeSlam;
            AssertRequiredSettings();
        }

        private void CreatePickaxeVisual(Transform socket)
        {
            Debug.Assert(socket != null, $"{nameof(PickaxeVisualSocket)} requires valid socket.", this);

            _pickaxeVisualInstance = Instantiate(pickaxeVisualPrefab, socket, false);
            _pickaxeVisualInstance.transform.localPosition = visualLocalPosition;
            _pickaxeVisualInstance.transform.localRotation = Quaternion.Euler(visualLocalEulerAngles);
            _pickaxeVisualInstance.transform.localScale = visualLocalScale;
            _pickaxeVisualInstance.SetActive(false);
        }

        private void AssertRequiredSettings()
        {
            Debug.Assert(damageCaster != null, $"{nameof(PickaxeSlamSkill)} requires {nameof(damageCaster)}.", this);
            Debug.Assert(pickaxeVisualPrefab != null, $"{nameof(PickaxeSlamSkill)} requires {nameof(pickaxeVisualPrefab)}.", this);
            Debug.Assert(_damageCalcCompo != null || !Application.isPlaying,
                $"{nameof(PickaxeSlamSkill)} requires {nameof(DamageCalcCompo)}.", this);
        }

        private void OnDestroy()
        {
            if (_pickaxeVisualInstance != null)
                Destroy(_pickaxeVisualInstance);
        }

        private void OnDrawGizmosSelected()
        {
            if (damageCaster == null)
                return;

            Gizmos.color = new Color(0.9f, 0.15f, 0.05f, 0.25f);
            Gizmos.DrawWireSphere(damageCaster.transform.position, damageCaster.CastRadius);
        }
    }
}
