using Chipmunk.ComponentContainers;
using Scripts.Combat.Datas;
using UnityEngine;

namespace Scripts.Entities
{
    public class EntityActionData : MonoBehaviour, IContainerComponent
    {
        public Vector3 HitPoint;
        public Vector3 HitNormal;
        public bool HitByPowerAttack { get; set; }

        public DamageData LastDamageData { get; set; }
        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
        }
    }
}
