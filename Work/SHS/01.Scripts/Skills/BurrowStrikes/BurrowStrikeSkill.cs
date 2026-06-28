using Chipmunk.ComponentContainers;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities;
using Scripts.Players;
using Scripts.SkillSystem.Skills;
using UnityEngine;

namespace SHS.Scripts.Skills.BurrowStrikes
{
    public class BurrowStrikeSkill : MovingSkill
    {
        [SerializeField] private ParticleSystem burrowEffect;
        [SerializeField] private ParticleSystem digEffect;
        [SerializeField] private int invincibilityLayer;
        [SerializeField] private DamageCaster damageCaster;
        [SerializeField] private float damagePerSecond = 10f;
        [SerializeField] private int defPierceLevel = 1;
        private EntityAnimator entityAnimator;
        private EntityAnimatorTrigger animatorTrigger;
        private DamageCalcCompo damageCalcCompo;
        private CharacterMovement playerMovement;
        private Renderer[] modelRenderers;
        private bool[] modelRendererEnabledStates;
        private Vector3 animatorLocalPosition;
        private Quaternion animatorLocalRotation;
        private int previousLayer;
        private SkillMoveType previousMoveType;
        private bool previousApplyRootMotion;
        private bool isBurrowed;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            entityAnimator = container.Get<EntityAnimator>();
            animatorTrigger = container.Get<EntityAnimatorTrigger>();
            damageCalcCompo = container.GetSubclassComponent<DamageCalcCompo>();
            playerMovement = container.Get<CharacterMovement>();
            modelRenderers = entityAnimator.GetComponentsInChildren<Renderer>(true);
            modelRendererEnabledStates = new bool[modelRenderers.Length];

            damageCaster.InitCaster(_owner);
        }

        public override void StartSkill()
        {
            base.StartSkill();
            previousLayer = _container.gameObject.layer;
            _container.gameObject.layer = invincibilityLayer;
            isBurrowed = false;
            SaveModelRendererStates();

            if (playerMovement == null)
                return;

            previousMoveType = MoveType;
            MoveType = SkillMoveType.Stop;
            playerMovement.SetMovementDirection(Vector3.zero);
            playerMovement.StopImmediately();

            animatorLocalPosition = entityAnimator.transform.localPosition;
            animatorLocalRotation = entityAnimator.transform.localRotation;
            previousApplyRootMotion = entityAnimator.ApplyRootMotion;
            entityAnimator.ApplyRootMotion = true;
            entityAnimator.OnAnimatorMoveEvent.AddListener(HandleAnimatorMove);
            animatorTrigger.OnAttackVFXTrigger += PlayDigEffect;
            animatorTrigger.OnBurrowEmergeTrigger += HandleBurrowEmerge;
        }

        public override void OnSkillTrigger()
        {
            base.OnSkillTrigger();
            if (playerMovement != null)
            {
                MoveType = SkillMoveType.Move;
                digEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            PlayBurrowEffect();
            SetModelVisible(false);
            isBurrowed = true;
        }

        private void Update()
        {
            if (!isBurrowed)
                return;

            DamageData damageData = damageCalcCompo.CalculateDamage(
                damagePerSecond * Time.deltaTime,
                1f,
                defPierceLevel,
                DamageType.MELEE);
            damageCaster.CastDamage(damageData, _owner.transform.position, _owner.transform.forward, null);
        }

        public override void EndSkill()
        {
            base.EndSkill();
            isBurrowed = false;
            if (playerMovement != null)
            {
                MoveType = previousMoveType;
                animatorTrigger.OnAttackVFXTrigger -= PlayDigEffect;
                animatorTrigger.OnBurrowEmergeTrigger -= HandleBurrowEmerge;
                entityAnimator.OnAnimatorMoveEvent.RemoveListener(HandleAnimatorMove);
                entityAnimator.ApplyRootMotion = previousApplyRootMotion;
                entityAnimator.transform.localPosition = animatorLocalPosition;
                entityAnimator.transform.localRotation = animatorLocalRotation;
                digEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            burrowEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            SetModelVisible(true);
            _container.gameObject.layer = previousLayer;
        }

        private void PlayBurrowEffect()
        {
            burrowEffect.Play(true);
        }

        private void PlayDigEffect()
        {
            digEffect.Play(true);
        }

        private void HandleAnimatorMove(Vector3 positionDelta, Quaternion rotationDelta)
        {
            entityAnimator.transform.position += positionDelta;
            entityAnimator.transform.rotation *= rotationDelta;
        }

        private void HandleBurrowEmerge()
        {
            isBurrowed = false;
            burrowEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            SetModelVisible(true);
            PlayDigEffect();
        }

        private void SetModelVisible(bool isVisible)
        {
            for (int i = 0; i < modelRenderers.Length; i++)
            {
                Renderer modelRenderer = modelRenderers[i];
                if (modelRenderer is ParticleSystemRenderer)
                    continue;

                modelRenderer.enabled = isVisible && modelRendererEnabledStates[i];
            }
        }

        private void SaveModelRendererStates()
        {
            for (int i = 0; i < modelRenderers.Length; i++)
                modelRendererEnabledStates[i] = modelRenderers[i].enabled;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            if (damageCaster is SphereDamageCaster sphereCaster)
                Gizmos.DrawWireSphere(transform.position, sphereCaster.CastRadius);
        }
    }
}
