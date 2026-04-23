using Chipmunk.ComponentContainers;
using Scripts.Combat.Datas;
using UnityEngine;

namespace Scripts.Combat
{
    /// <summary>
    /// CharacterMovement와 CharacterNavMovement가 공유하는 인터페이스
    /// 스킬에서 Movement 타입에 관계없이 사용 가능
    /// </summary>
    public interface ISkillMovement : IContainerComponent
    {
        Vector3 Velocity { get; }
        bool CanMove { get; set; }
        void SetRotation(Vector3 direction);
        void ApplyMovementData(Vector3 direction, MovementDataSO movementData);
        void SetPosition(Vector3 position);
    }
}

