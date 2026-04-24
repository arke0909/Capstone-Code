﻿using Chipmunk.ComponentContainers;
using Code.StatusEffectSystem;
using Entities;
using Scripts.Combat;
using UnityEngine;

namespace Scripts.SkillSystem.Skills
{
    public class RefractionSkill : ActiveSkill
    {
        [SerializeField] private BuffSO speedIncreaseData;
        [SerializeField] private BuffSO damageStoringData;
        [SerializeField] private float skillDuration;
        [SerializeField] private bool dashEnable;
        [SerializeField] private bool _canDamageStoring;
        private bool _isUsing;
        private Vector3 _returnPos;
        private float _skillTimer = 0;
        private ISkillMovement _movement;
        private VFXComponent _vfxCompo;
        private EntityStatusEffect _statusEffect;
        private int _speedIncreaseLevel;
        private int _damageStoringLevel;
        
        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _vfxCompo = container.Get<VFXComponent>();
            _statusEffect = container.Get<EntityStatusEffect>();
            _movement = container.GetSubclassComponent<ISkillMovement>();
        }
        public override void StartAndUseSkill()
        {
            base.StartAndUseSkill();
            if (!_isUsing)
            {
                _returnPos = _owner.transform.position;
                _vfxCompo.PlayVFX("RefractionEffect", transform.position, Quaternion.identity, false);
                _skillTimer = skillDuration;
                _isUsing = true;

                _statusEffect.AddStatusEffect(speedIncreaseData.GetStatusEffectInfo(_speedIncreaseLevel));
                
                //if (dashEnable)
                    //_skillComponent.ChangeSkill(typeof(DashSkill),(int)ActiveSlotType.Space);
                if (_canDamageStoring)
                {
                    _statusEffect.AddStatusEffect(damageStoringData.GetStatusEffectInfo(_damageStoringLevel));
                }
            }
            else
            {
                _vfxCompo.StopVFX("RefractionEffect");
                _movement.SetPosition(_returnPos);
                _isUsing = false;
                
                _statusEffect.RemoveStatusEffect(speedIncreaseData);
                
                //if (dashEnable)
                    //_skillComponent.ChangeSkill(typeof(RollingSkill),(int)ActiveSlotType.Space);
                if(_canDamageStoring)
                    _statusEffect.RemoveStatusEffect(damageStoringData);
            }
        }
        protected void Update()
        {
            if (_isUsing)
            {
                _skillTimer = Mathf.Max(_skillTimer - Time.deltaTime, 0);
                if (Mathf.Approximately(_skillTimer, 0))
                    StartAndUseSkill();
            }
        }
    }
}

