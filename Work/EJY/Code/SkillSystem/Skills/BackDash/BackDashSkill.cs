using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chipmunk.ComponentContainers;
using Code.ETC;
using Code.StatusEffectSystem;
using Cysharp.Threading.Tasks;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Effects;
using Scripts.Entities;
using Scripts.SkillSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.SkillSystem.Skills.BackDash
{
    public class BackDashSkill : ActiveSkill
    {
        [SerializeField] private PoolManagerSO _poolManager;
        [SerializeField] private MovementDataSO backDashMovementDataSO;
        [SerializeField] private DecalObject attackAreaDecal;
        [SerializeField] private DamageCaster damageCaster;
        [SerializeField] private BuffCaster buffCaster;
        [SerializeField] private PoolItemSO effectPoolItem;
        [FormerlySerializedAs("slowStatusEffect")] [SerializeField] private BuffSO slowBuff;
        [SerializeField] private float damage = 3;
        [SerializeField] private bool applySlow;
        [SerializeField] private bool addDamageMultiply;
        private IAimProvider _aimProvider;
        private ISkillMovement _skillMovement;
        private DamageCalcCompo _damageCalcCompo;
        private float _additionalSlowTime;
        private float _additionalDamageMultiply = 0.5f;
        private bool isIncreased;
        
        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _aimProvider = container.GetSubclassComponent<IAimProvider>();
            _skillMovement = container.GetSubclassComponent<ISkillMovement>();
            _damageCalcCompo = container.GetSubclassComponent<DamageCalcCompo>();
            damageCaster.InitCaster(_owner);
        }


        public override async void StartAndUseSkill()
        {
            base.StartAndUseSkill();

            Vector3 targetPos = _aimProvider.GetAimPosition(); 
            Vector3 ownerPos = _owner.transform.position;
            ownerPos.y = targetPos.y;
            
            attackAreaDecal.SetPos(_owner.transform.position);
            
            Vector3 dir = (targetPos - ownerPos).normalized;
            
            _skillMovement.CanMove = false;
            _skillMovement.ApplyMovementData(-dir, backDashMovementDataSO);
            attackAreaDecal.SetActive(true);
            attackAreaDecal.SetParent(null);
            
            PoolingEffect effect = _poolManager.Pop(effectPoolItem) as PoolingEffect;
            effect.PlayVFX(attackAreaDecal.transform.position, Quaternion.LookRotation(_owner.transform.forward));
            
            DamageData damageData = _damageCalcCompo.CalculateDamage(damage, 1, 1, DamageType.RANGE);
            damageCaster.CastDamage(damageData ,transform.position, dir,null);

            if (applySlow)
            {
                buffCaster.CastBuff(transform.position, slowBuff.GetStatusEffectInfo());
            }

            if (addDamageMultiply && !isIncreased)
            {
                _owner.OnDamageCalc += DamageMultiply;
                _owner.OnAttack += UnsubscribeDamageCalc;
                isIncreased = true;
            }
            
            await UniTask.WaitForSeconds(backDashMovementDataSO.duration);
            
            _skillMovement.CanMove = true;
            attackAreaDecal.SetActive(false);
            attackAreaDecal.SetParent(transform);
        }

         private float DamageMultiply(Entity dealer, Transform target)
         {
             return _additionalDamageMultiply;
        }
        
        private void UnsubscribeDamageCalc(Entity dealer, IDamageable target)
        {
            _owner.OnDamageCalc -= DamageMultiply;
            _owner.OnAttack -= UnsubscribeDamageCalc;
            isIncreased = false;
        }

       

        
        
    }
}
