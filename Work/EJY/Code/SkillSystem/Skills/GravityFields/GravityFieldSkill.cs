using Chipmunk.ComponentContainers;
using Code.ETC;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using Scripts.SkillSystem;
using UnityEngine;

namespace Code.SkillSystem.Skills.GravityFields
{
    public class GravityFieldSkill : ActiveSkill
    {
        [SerializeField] private PoolItemSO gravityFieldPoolItem;
        [SerializeField] private bool isSlowEntity;
        [SerializeField] private bool isStunEntity;
        
        [Inject] private PoolManagerMono _poolManagerMono;
        
        private IAimProvider _aimProvider;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _aimProvider = container.GetSubclassComponent<IAimProvider>();
        }

        private void UpgradeSlowEntity() => isSlowEntity = true;
        private void RollbackSlowEntity() => isSlowEntity = false;
        private void UpgradeStunEntity() => isStunEntity = true;
        private void RollbackStunEntity() => isStunEntity = false;

        
        public override void StartAndUseSkill()
        {
            Vector3 gravityFieldPos = _aimProvider.GetAimPosition();

            GravityField gf = _poolManagerMono.Pop<GravityField>(gravityFieldPoolItem);
            gf.Init(gravityFieldPos, isSlowEntity, isStunEntity);
        }
    }
}