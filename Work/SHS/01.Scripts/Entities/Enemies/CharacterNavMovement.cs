using AYellowpaper.SerializedCollections;
using Chipmunk.ComponentContainers;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities;
using System;
using System.Collections.Generic;
using Chipmunk.Modules.StatSystem;
using UnityEngine;

namespace Code.SHS.Entities.Enemies
{
    public enum NavMoveType
    {
        Idle,
        Walk,
        Sprint,
        Aim
    }

    /// <summary>
    /// NavMovement를 상속하여 이동 타입에 따라 속도가 달라지는 네비게이션 이동 컴포넌트
    /// </summary>
    public class CharacterNavMovement : NavMovement, ISkillMovement
    {
        [SerializeField] private SerializedDictionary<NavMoveType, StatSO> speedMultipliers;

        private NavMoveType _moveType = NavMoveType.Idle;
        private StatOverrideBehavior _StatOverrideBehavior;

        private bool _canMove = true;

        public NavMoveType MoveType
        {
            get => _moveType;
            set
            {
                if (_moveType != value)
                {
                    _moveType = value;
                    UpdateAgentSpeed();
                }
            }
        }

        private StatSO CurrentTypeMultiplier => speedMultipliers.GetValueOrDefault(MoveType);

        // ISkillMovement 구현
        bool ISkillMovement.CanMove
        {
            get => _canMove;
            set
            {
                _canMove = value;
                SetStop(!value);
            }
        }

        void ISkillMovement.SetRotation(Vector3 direction)
        {
            if (direction.sqrMagnitude > 0.01f)
            {
                LookAtTarget(transform.position + direction, false);
            }
        }

        void ISkillMovement.ApplyMovementData(Vector3 direction, MovementDataSO movementData)
        {
            KnockBack(direction, movementData);
        }

        public override void OnInitialize(ComponentContainer componentContainer)
        {
            base.OnInitialize(componentContainer);
            _StatOverrideBehavior = this.GetContainerComponent<StatOverrideBehavior>();
        }

        public override void AfterInitialize()
        {
            // 기본 스탯 초기화
            moveSpeedStat = _StatOverrideBehavior.GetStat(moveSpeedStat);

            // 각 MoveType별 속도 배율 스탯 초기화
            foreach (NavMoveType type in Enum.GetValues(typeof(NavMoveType)))
            {
                if (speedMultipliers.TryGetValue(type, out StatSO stat))
                {
                    speedMultipliers[type] = _StatOverrideBehavior.GetStat(stat);
                }
            }

            UpdateAgentSpeed();
        }

        protected override void UpdateAgentSpeed()
        {
            if (moveSpeedStat == null) return;

            float typeMultiplier = CurrentTypeMultiplier != null ? CurrentTypeMultiplier.Value : 1f;
            agent.speed = moveSpeedStat.Value * _speedMultiplier * typeMultiplier;
        }

        /// <summary>
        /// 특정 MoveType의 속도 배율을 가져옵니다.
        /// </summary>
        public float GetTypeSpeedMultiplier(NavMoveType type)
        {
            if (speedMultipliers.TryGetValue(type, out StatSO stat))
            {
                return stat.Value;
            }
            return 1f;
        }

        /// <summary>
        /// 현재 MoveType의 실제 이동 속도를 반환합니다.
        /// </summary>
        public float GetCurrentSpeed()
        {
            float typeMultiplier = CurrentTypeMultiplier != null ? CurrentTypeMultiplier.Value : 1f;
            return moveSpeedStat.Value * _speedMultiplier * typeMultiplier;
        }

        public void SetPosition(Vector3 position)
        {
        }

        public override void ResetMovementState(Vector3 position, Quaternion rotation)
        {
            _canMove = true;
            MoveType = NavMoveType.Idle;
            SpeedMultiplier = 1f;
            base.ResetMovementState(position, rotation);
        }
    }
}
