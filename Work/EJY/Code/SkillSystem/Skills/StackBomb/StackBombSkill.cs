using Chipmunk.ComponentContainers;
using Code.Players;
using Cysharp.Threading.Tasks;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using Scripts.SkillSystem;
using UnityEngine;

namespace Code.SkillSystem.Skills.StackBomb
{
    public class StackBombSkill : ActiveSkill
    {
        [SerializeField] private PoolItemSO stackBombItem;
        [SerializeField] private Transform firePosTrm;
        [SerializeField] private float skillCastRange = 10f;
        
        [Inject] private PoolManagerMono _poolManager;
        private TargetingComponent _targeting;
        private Transform _targetTrm;
        private Vector3 _rayHitPoint;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _targeting = container.GetSubclassComponent<TargetingComponent>();
        }

        public override bool CanUseSkill()
        {
            return base.CanUseSkill()
                   && _targeting.TryResolveSoftTarget(out _targetTrm, out _rayHitPoint)
                   && Vector3.Distance(_rayHitPoint, _owner.transform.position) <= skillCastRange;
        }

        public override void StartSkill()
        {
            StackBomb stackBomb = _poolManager.Pop<StackBomb>(stackBombItem);
            stackBomb.transform.position = firePosTrm.position;
            stackBomb.Init(_owner, _targetTrm).Forget();
        }
    }
}
