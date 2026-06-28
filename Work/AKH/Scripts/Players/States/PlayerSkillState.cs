using Scripts.SkillSystem.Manage;
using Chipmunk.ComponentContainers;
using UnityEngine;
using Scripts.SkillSystem;
using Scripts.SkillSystem.Skills;

namespace Scripts.Players.States
{
    public class PlayerSkillState : PlayerMoveState
    {
        private ActiveSkillComponent _skillCompo;
        private ActiveSkill _currentSkill;
        private MovingSkill _movingSkill;
        private float _endTime;
        private bool _waitingForBurrowEmergeEnd;

        private static readonly int _skillHash = Animator.StringToHash("SkillIndex");
        private static readonly int _walkableHash = Animator.StringToHash("IsWalkable");

        public PlayerSkillState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _skillCompo = container.Get<ActiveSkillComponent>(true);
        }

        public override void Enter()
        {
            Debug.Assert(_skillCompo != null && _skillCompo.CurrentSkill != null,
                "CurrentSkill is null but you are in skill state");

            _currentSkill = _skillCompo.CurrentSkill;
            _movingSkill = _currentSkill as MovingSkill;
            _waitingForBurrowEmergeEnd = false;
            if (_movingSkill != null)
                _endTime = Time.time + _movingSkill.Duration;

            _myMoveType = _currentSkill.MoveType == SkillMoveType.Move
                ? MoveType.Walk
                : MoveType.Idle;

            base.Enter();

            if (_currentSkill.MoveType != SkillMoveType.Move)
                StopManualMovement();

            _animator.SetParam(_walkableHash, _currentSkill.isWalkable);
            _animator.SetParam(_skillHash, (int)_currentSkill.AnimType);
            _animatorTrigger.OnCastSkillTrigger += HandleSkillCast;
            _currentSkill.StartSkill();
        }

        protected override bool ShouldProcessManualMovement()
        {
            return !_waitingForBurrowEmergeEnd && _currentSkill != null && _currentSkill.MoveType == SkillMoveType.Move;
        }

        public override void Update()
        {
            if (_waitingForBurrowEmergeEnd)
            {
                StopManualMovement();

                if (_isTriggerCall)
                    _player.ChangeState(PlayerStateEnum.Idle);

                return;
            }

            if (_movingSkill != null && _currentSkill.AnimType == SkillAnimType.Burrow && Time.time >= _endTime)
            {
                _waitingForBurrowEmergeEnd = true;
                _isTriggerCall = false;
                _animator.SetParam(_animationHash, false);
                _animator.SetParam(_walkableHash, false);
                StopManualMovement();
                return;
            }

            base.Update();

            if (_currentSkill.MoveType == SkillMoveType.Stop)
                _movement.SetMovementDirection(Vector3.zero);

            if (_movingSkill != null)
            {
                if (Time.time >= _endTime)
                    _player.ChangeState(PlayerStateEnum.Idle);

                return;
            }

            if (_isTriggerCall)
                _player.ChangeState(PlayerStateEnum.Idle);
        }

        private void HandleSkillCast()
        {
            _currentSkill.OnSkillTrigger();
        }

        public override void Exit()
        {
            base.Exit();
            _animator.SetParam(_walkableHash, false);
            _currentSkill.EndSkill();
            _animatorTrigger.OnCastSkillTrigger -= HandleSkillCast;
            _movingSkill = null;
            _waitingForBurrowEmergeEnd = false;
        }
    }
}
