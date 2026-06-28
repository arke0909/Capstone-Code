using Chipmunk.ComponentContainers;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Effects;
using Scripts.Entities;
using UnityEngine;

namespace SHS.Scripts.Combats
{
    public class DefaultAttack : MonoBehaviour, IAttackable, IContainerComponent
    {
        [SerializeField] private OverlapDamageCaster damageCaster;
        [SerializeField] private float defaultDamage = 5f;
        [SerializeField] private int defPierceLevel;
        [SerializeField] private PoolItemSO poolingEffectSO;

        private Entity _owner;
        private DamageCalcCompo _damageCalcCompo;

        public ComponentContainer ComponentContainer { get; set; }
        public GameObject Dealer => gameObject;
        public Entity Owner => _owner;
        public float AttackRange => damageCaster.CastRadius;
        public AttackableState CurrentAttackableState => AttackableState.CanAttack;
        public bool UsesAnimationAttackTrigger => true;

        public void OnInitialize(ComponentContainer componentContainer)
        {
            Debug.Assert(damageCaster != null, "DefaultAttack requires DamageCaster.", this);

            _owner = componentContainer.Get<Entity>(true);
            _damageCalcCompo = componentContainer.Get<DamageCalcCompo>();

            Debug.Assert(_owner != null, "DefaultAttack requires Entity.", this);
            Debug.Assert(_damageCalcCompo != null, "DefaultAttack requires DamageCalcCompo.", this);

            damageCaster.InitCaster(_owner);
        }

        public void EnterAttack()
        {
        }

        public void AttackTrigger()
        {
            DamageData damageData = _damageCalcCompo.CalculateDamage(
                defaultDamage,
                1f,
                defPierceLevel,
                DamageType.MELEE);
            damageData.hitEffectPoolItem = poolingEffectSO;

            damageCaster.CastDamage(
                damageData,
                damageCaster.transform.position,
                transform.forward,
                null);
        }

        public void UpdateAttack(AttackContext context)
        {
        }

        public void EndAttack()
        {
        }
    }
}
