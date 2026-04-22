using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.ETC;
using Code.GameEvents;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using Scripts.SkillSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.SkillSystem.Skills.Drones
{
    public class DroneSkill : ActiveSkill
    {
        [SerializeField] private PoolItemSO droneItemSO;
        [SerializeField] private bool isDamageDemodifyDecrease;
        [SerializeField] private bool createSpeedArea;
        [SerializeField] private float range = 75f;
        [SerializeField] private float sightRange = 25f;
        
        [Inject] private PoolManagerMono _poolManager;
        private IAimProvider _aimProvider;
        
        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _aimProvider = container.GetSubclassComponent<IAimProvider>();
        }

        public override void StartAndUseSkill()
        {
            Vector3 targetPos = _aimProvider.GetAimPosition();
            targetPos.y = _owner.transform.position.y;
            
            if (Vector3.Distance(targetPos, _owner.transform.position) > range) return;
            
            Drone drone = _poolManager.Pop<Drone>(droneItemSO);
            drone.transform.position = transform.position + new Vector3(0, 3.5f, 0);
            drone.SetCreateSpeedArea(createSpeedArea);
            drone.SetIsDamageModifyDecrease(isDamageDemodifyDecrease);
            drone.Init(targetPos, isDamageDemodifyDecrease,Level, sightRange);
        }
    }
}
