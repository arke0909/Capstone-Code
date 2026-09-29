using Chipmunk.ComponentContainers;
using Code.ETC;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using Scripts.SkillSystem;
using UnityEngine;

namespace Code.SkillSystem.Skills.StoneEdges
{
    public class StoneEdgeSkill : ActiveSkill
    {
        [SerializeField] private PoolItemSO stoneEdgeItem;
        [SerializeField] private float skillUseRange = 15f;
        [SerializeField] private float additionalSize = 0.75f;
        [SerializeField] private float damage = 18f;

        [Inject] private PoolManagerMono _poolManger;
        private IAimProvider _aimProvider;
        private bool _explodeOnPlayerHit;
        private float _skillSize = 1f;
        private Vector3 _skillPos;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _aimProvider = container.GetSubclassComponent<IAimProvider>();
        }

        public override bool CanUseSkill()
        {
            _skillPos = _aimProvider.GetWorldAimPosition();
            _skillPos.y = _owner.transform.position.y;
            
            float dist = Vector3.Distance(_skillPos, _owner.transform.position);
            
            return base.CanUseSkill() && skillUseRange >= dist;
        }

        private void UpgradeStoneEdgeSize()
        {
            _skillSize += additionalSize;
        }
        
        private void RollbackStoneEdgeSize()
        {
            _skillSize -= additionalSize;
        }

        private void UpgradeExplodeOnPlayerHit()
        {
            _explodeOnPlayerHit = true;
        }

        private void RollbackExplodeOnPlayerHit()
        {
            _explodeOnPlayerHit = false;
        }

        public override void OnSkillTrigger()
        {
            base.StartSkill();

            Vector3 dir = _skillPos - _owner.transform.position;

            StoneEdge stoneEdge = _poolManger.Pop<StoneEdge>(stoneEdgeItem);
            stoneEdge.Init(_owner, _skillPos, dir, _skillSize, damage, _explodeOnPlayerHit);
        }
    }
}
