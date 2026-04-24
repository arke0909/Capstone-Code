﻿using Chipmunk.ComponentContainers;
using Chipmunk.Modules.StatSystem;
using Code.StatusEffectSystem;
using Cysharp.Threading.Tasks;
using Entities;
using Scripts.Combat;
using Scripts.Entities;
using Scripts.FSM;
using UnityEngine;

namespace Scripts.SkillSystem.Skills
{
    public class AdrenalineSkill : ActiveSkill, IUseStateSkill
    {
        [SerializeField] private BuffSO adrenalineData;
        [SerializeField] private StateDataSO _targetState;
        [SerializeField] private float additionalTime = 0.3f;
        [SerializeField] private bool addReloadSpeed;
        [SerializeField] private bool getAdditionalTime;
        [SerializeField] private BuffSO reloadSpeedData;
        
        public StateDataSO TargetState { get => _targetState; set => _targetState = value; }

        public SkillAnimType AnimType => SkillAnimType.Default;

        private EntityStatusEffect _buffCompo;
        private VFXComponent _vfxCompo;
        private StatOverrideBehavior _statCompo;
        private float _remainingBuffTime;
        private bool _isBuffActive = false;
        private int _buffLevel;
        private int _reloadLevel;
        
        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _buffCompo = container.Get<EntityStatusEffect>();
            _vfxCompo = container.Get<VFXComponent>();
            _statCompo = container.Get<StatOverrideBehavior>();
        }
        

        private async UniTaskVoid RunBuffLoop()
        {
            _isBuffActive = true;
            _remainingBuffTime = adrenalineData.applyTime;

            _buffCompo.AddStatusEffect(adrenalineData.GetStatusEffectInfo(_buffLevel));
            
            _vfxCompo.PlayVFX("AdrenalineEffect", transform.position, Quaternion.identity);

            while (_remainingBuffTime > 0)
            {
                _remainingBuffTime -= Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            _remainingBuffTime = 0;
            _isBuffActive = false;
            _vfxCompo.StopVFX("AdrenalineEffect");
        }

        private void AddReloadSpeed()
        {
            _buffCompo.AddStatusEffect(reloadSpeedData.GetStatusEffectInfo(_reloadLevel));
        }
        
         private async UniTaskVoid OnHitGetAdditionalTime()
         {
             _owner.OnHit += OnHitTarget;
             
             await UniTask.WaitUntil(()=> !_isBuffActive);
             
             _owner.OnHit -= OnHitTarget;
         }

         private void OnHitTarget(Entity dealer, IDamageable target)
         {
             if (!_isBuffActive) return;
        
             _remainingBuffTime += additionalTime;
         }

        public void OnSkillTrigger()
        {
            if (!_isBuffActive)
            {
                RunBuffLoop().Forget();
                if (addReloadSpeed)
                    AddReloadSpeed();
                if (getAdditionalTime)
                    OnHitGetAdditionalTime().Forget();
            }
        }
    }
}


