using Ami.BroAudio;
using Chipmunk.ComponentContainers;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities;
using Scripts.Entities.Vitals;
using Scripts.FSM;
using Scripts.Players;
using UnityEngine;

namespace Scripts.SkillSystem.Skills
{
    public class RollingSkill : ActiveSkill, IUseStateSkill
    {
        [SerializeField] private MovementDataSO movementData;
        [SerializeField] private float staminaUsage = -10f;
        [SerializeField] private float staminaStopTimer = 1.2f;
        [SerializeField] private SoundID rollingID;
        private ISkillMovement _movement;
        private int _defaultLayer;
        private int _avoidLayer;
        private StaminaCompo _staminaCompo;
        private EntityAnimator _animator;

        [field: SerializeField] public StateDataSO TargetState { get; set; }
        public SkillAnimType AnimType => SkillAnimType.Rolling;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _defaultLayer = _owner.gameObject.layer;
            _staminaCompo = container.Get<StaminaCompo>();
            _avoidLayer = LayerMask.NameToLayer("AvoidEntity");
            _movement = container.GetSubclassComponent<ISkillMovement>();
            _animator = container.Get<EntityAnimator>();
        }

        public override bool CanUseSkill()
        {
            return base.CanUseSkill() && (_staminaCompo?.CurrentValue ?? 0f) >= -staminaUsage;
        }

        public override void StartAndUseSkill()
        {
            base.StartAndUseSkill();
            BroAudio.Play(rollingID, transform.position);
            
            _movement.CanMove = false;
            Vector3 velocity = _movement.Velocity;
            velocity.y = 0;
            Vector3 direction = Mathf.Approximately(velocity.magnitude, 0f)
                ? _owner.transform.forward
                : _movement.Velocity.normalized;
            direction.y = 0;
            _movement.SetRotation(direction);
            _movement.ApplyMovementData(direction, movementData);
            _staminaCompo?.ChangeValueWithTimer(staminaUsage, staminaStopTimer);
            _owner.gameObject.layer = _avoidLayer;
            _animator.OnAnimatorMoveEvent.AddListener(HandleAnimatorMove);
        }

        public override void EndSkill()
        {
            _movement.CanMove = true;
            _owner.gameObject.layer = _defaultLayer;
            _animator.OnAnimatorMoveEvent.RemoveListener(HandleAnimatorMove);
        }

        private void HandleAnimatorMove(Vector3 arg0, Quaternion arg1)
        {
            _movement.SetPosition(transform.position + arg0);
        }

        public void OnSkillTrigger()
        {
        }
    }
}