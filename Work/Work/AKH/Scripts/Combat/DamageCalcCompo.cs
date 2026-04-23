using Chipmunk.ComponentContainers;
using Chipmunk.Modules.StatSystem;
using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;

namespace Scripts.Combat
{
    public class DamageCalcCompo : MonoBehaviour, IContainerComponent, IAfterInitialze
    {
        [SerializeField] private StatSO damageModifyStat;
        private StatOverrideBehavior _statOverrideBehaviorCompo;
        public ComponentContainer ComponentContainer { get; set; }
        public void OnInitialize(ComponentContainer componentContainer)
        {
            _statOverrideBehaviorCompo = componentContainer.Get<StatOverrideBehavior>();
        }

        public void AfterInitialize()
        {
            damageModifyStat = _statOverrideBehaviorCompo.GetStat(damageModifyStat);
        }
        public DamageData CalculateDamage(float defaultDamage, float damageMultipler, int defPierceLevel, DamageType damageType)
        {
            //약간 애매한게 책을 장착하면 플레이어 스탯을 바꿔버리는지 뭔지 잘 몰겟음
            //일단 일케함
            DamageData data = new DamageData();
            data.damage = defaultDamage * damageMultipler * damageModifyStat.Value;
            data.defPierceLevel = defPierceLevel;
            data.damageType = damageType;
            return data;
        }
    }
}
