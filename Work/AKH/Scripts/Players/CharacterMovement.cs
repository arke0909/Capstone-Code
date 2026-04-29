using AYellowpaper.SerializedCollections;
using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies.Combat;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities;
using Scripts.FSM;
using System;
using System.Collections.Generic;
using Chipmunk.Modules.StatSystem;
using UnityEngine;

namespace Scripts.Players
{
    public enum MoveType
    {
        Idle,
        Walk,
        Sprint,
        Aim
    }
    public struct RotationInfo
    {
        public Quaternion targetRot;
        public float rotationSpeed;
        public RotationInfo(Quaternion targetRot, float rotationSpeed)
        {
            this.targetRot = targetRot;
            this.rotationSpeed = rotationSpeed;
        }
    }
    public class CharacterMovement : MonoBehaviour, IAfterInitialze, IKnockbackable, ISkillMovement
    {
        [SerializeField] private StatSO moveSpeedStat;
        [SerializeField] private StateDataSO stunState;
        [SerializeField] private float gravity = -9.8f;
        [SerializeField] private CharacterController controller;
        [SerializeField] private SerializedDictionary<MoveType, StatSO> speedMultipliers;
        // [SerializeField] private Transform parent;
        public bool IsGround => controller.isGrounded;
        private float _moveSpeed = 12f;
        public bool CanManualMovement { get; set; } = true;

        // ISkillMovement 구현
        bool ISkillMovement.CanMove
        {
            get => CanManualMovement;
            set => CanManualMovement = value;
        }

        public MoveType MoveType { get; set; } = MoveType.Idle;
        public Vector3 Direction => _movementDirection;
        public ComponentContainer ComponentContainer => _container;
        private RotationInfo _rotationInfo;
        private StatSO _currentMultiplier => speedMultipliers.GetValueOrDefault(MoveType);
        private Vector3 _autoMovement;
        private float _autoMoveStartTime;
        private MovementDataSO _movementData;
        private Entity _entity;
        private StatOverrideBehavior _statOverrideBehavior;
        private Vector3 _velocity;
        public Vector3 Velocity => _velocity;
        private ComponentContainer _container;
        
        ComponentContainer IContainerComponent.ComponentContainer { get; set; }

        private float _verticalVelocity;
        private Vector3 _movementDirection;

        public void OnInitialize(ComponentContainer container)
        {
            _container = container;
            _entity = container.GetCompo<Entity>(true);
            _statOverrideBehavior = container.GetCompo<StatOverrideBehavior>();
        }
        public void AfterInitialize()
        {
            foreach (MoveType type in Enum.GetValues(typeof(MoveType)))
            {
                if (speedMultipliers.TryGetValue(type, out StatSO stat))
                    speedMultipliers[type] = _statOverrideBehavior.GetStat(stat);
            }
            _moveSpeed = _statOverrideBehavior.SubscribeStat(moveSpeedStat, HandleMoveSpeedChange, 1f);
        }
        private void OnDestroy()
        {
            _statOverrideBehavior.UnSubscribeStat(moveSpeedStat, HandleMoveSpeedChange);
        }

        public void SetMovementDirection(Vector2 movementInput)
        {
            _movementDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;
        }
        public void SetMovementDirection(Vector3 movementDirection)
        {
            _movementDirection = movementDirection.normalized;
        }
        private void FixedUpdate()
        {
            CalculateMovement();
            ApplyGravity();
            Move();
            ApplyRotation();
        }

        private void ApplyRotation()
        {
            if (CanManualMovement)
            {
                Transform parent = _entity.transform;
                Quaternion targetRot = _rotationInfo.targetRot;
                float rotationSpeed = _rotationInfo.rotationSpeed;
                parent.rotation = Quaternion.Lerp(parent.rotation, targetRot, Time.fixedDeltaTime * rotationSpeed);
            }
        }

        private void CalculateMovement()
        {
            if (CanManualMovement)
            {
                _velocity = /*Quaternion.Euler(0, -45f, 0) * */_movementDirection;
                _velocity *= _moveSpeed * Time.fixedDeltaTime;
                if (_currentMultiplier != null)
                    _velocity *= _currentMultiplier.Value;
            }
            else
            {
                float normalizedTime = (Time.time - _autoMoveStartTime) / _movementData.duration;
                float currentSpeed = _movementData.maxSpeed * _movementData.moveCurve.Evaluate(normalizedTime);
                Vector3 currentMovement = _autoMovement * currentSpeed;
                _velocity = currentMovement * Time.fixedDeltaTime;
            }
        }

        public void SetRotationInfo(Vector3 direction, float rotationSpeed = 3f, bool isSmooth = true)
        {
            direction.y = 0;
            if (Mathf.Approximately(direction.magnitude, 0f) || !CanManualMovement) return;
            Quaternion targetRot = Quaternion.LookRotation(direction);
            _rotationInfo = new RotationInfo(targetRot, rotationSpeed);
        }
        public void SetRotation(Vector3 direction)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = targetRot;
        }
        private void ApplyGravity()
        {
            if (IsGround && _verticalVelocity < 0)
                _verticalVelocity = -0.03f;
            else
                _verticalVelocity += gravity * Time.fixedDeltaTime;

            _velocity.y = _verticalVelocity;
        }

        private void Move()
        {
            controller.Move(_velocity);
        }

        public void StopImmediately()
        {
            _movementDirection = Vector3.zero;
        }

        //public void SetAutoMovement(Vector3 autoMovement) => _autoMovement = autoMovement;
        public void ApplyMovementData(Vector3 direction, MovementDataSO movementData)
        {
            _autoMovement = direction;
            _autoMoveStartTime = Time.time;
            _movementData = movementData;
        }
        private void HandleMoveSpeedChange(StatSO stat, float currentValue, float prevValue)
            => _moveSpeed = currentValue;
        public void KnockBack(Vector3 direction, MovementDataSO movementData)
            => ApplyMovementData(direction, movementData);

        public void SetPosition(Vector3 position)
        {
            controller.enabled = false;
            _entity.transform.position = position;
            controller.enabled = true;
        }
    }
}