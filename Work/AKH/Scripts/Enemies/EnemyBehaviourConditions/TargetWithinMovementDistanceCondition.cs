using System;
using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies;
using Scripts.Combat.Datas;
using Scripts.SkillSystem.Manage;
using Scripts.SkillSystem.Skills;
using UnityEngine;

namespace Scripts.Enemies.EnemyBehaviourConditions
{
    [Serializable]
    public class TargetWithinMovementDistanceCondition : EnemyBehaviourCondition
    {
        [SerializeField] private bool useSkillSlotMovementData = true;
        [SerializeField] private ActiveSlotType slotType = ActiveSlotType.C;
        [SerializeField] private MovementDataSO movementData;
        [SerializeField, Min(1)] private int sampleCount = 24;
        [SerializeField] private float distanceOffset;
        [SerializeField] private bool usePlanarDistance = true;

        private ActiveSkillComponent _skillComponent;

        public override void Init(Enemy enemy)
        {
            base.Init(enemy);
            _skillComponent = enemy.Get<ActiveSkillComponent>();
        }

        public override bool Condition()
        {
            if (_enemy == null || _targetProvider == null || _targetProvider.Target == null)
                return false;

            MovementDataSO data = GetMovementData();
            float movementDistance = CalculateMovementDistance(data, sampleCount) + distanceOffset;
            if (movementDistance <= 0f)
                return false;

            return GetTargetDistance() <= movementDistance;
        }

        private float GetTargetDistance()
        {
            Vector3 origin = _enemy.transform.position;
            Vector3 targetPosition = _targetProvider.Target.transform.position;

            if (usePlanarDistance)
            {
                origin.y = 0f;
                targetPosition.y = 0f;
            }

            return Vector3.Distance(origin, targetPosition);
        }

        private MovementDataSO GetMovementData()
        {
            if (!useSkillSlotMovementData)
                return movementData;

            if (_skillComponent == null ||
                !_skillComponent.Sockets.TryGetValue(slotType, out ActiveSkillSocket socket) ||
                socket.CurrentActiveSkill is not IMovementDataProvider movementDataProvider)
            {
                return movementData;
            }

            return movementDataProvider.MovementData;
        }

        private static float CalculateMovementDistance(MovementDataSO data, int samples)
        {
            if (data == null || data.maxSpeed <= 0f || data.duration <= 0f || data.moveCurve == null)
                return 0f;

            int stepCount = Mathf.Max(1, samples);
            float normalizedStep = 1f / stepCount;
            float curveArea = 0f;
            float previousValue = Mathf.Max(0f, data.moveCurve.Evaluate(0f));

            for (int i = 1; i <= stepCount; i++)
            {
                float normalizedTime = i * normalizedStep;
                float currentValue = Mathf.Max(0f, data.moveCurve.Evaluate(normalizedTime));
                curveArea += (previousValue + currentValue) * 0.5f * normalizedStep;
                previousValue = currentValue;
            }

            return data.maxSpeed * data.duration * curveArea;
        }

#if UNITY_EDITOR
        public override void DrawGizmos(Transform trm)
        {
            float movementDistance = CalculateMovementDistance(movementData, sampleCount) + distanceOffset;
            if (movementDistance <= 0f)
                return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(trm.position, movementDistance);
        }
#endif
    }
}
