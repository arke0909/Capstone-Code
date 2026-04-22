using Chipmunk.ComponentContainers;
using Chipmunk.Modules.StatSystem;
using Code.ETC;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.FSM;
using Scripts.SkillSystem;
using UnityEngine;
using Scripts.SkillSystem.Skills;

namespace Code.SkillSystem.Skills.Bombing
{
    public class BombingSkill : ActiveSkill, IAimSkill
    {
        [SerializeField] private StateDataSO targetState;
        [SerializeField] private DecalObject decalObject;
        [SerializeField] private PoolItemSO bombingItemSO;
        [SerializeField] private StatSO attackStat;
        [SerializeField] private StatSO damageModifier;
        [SerializeField] private float defaultDamageMultiplier = 1.25f;
        [SerializeField] private bool createFloor;
        [SerializeField] private bool slowAndAdditionalDamage;

        [SerializeField] private PoolManagerSO _poolManager;
        private DamageCalcCompo _damageCalcCompo;
        private StatOverrideBehavior _statCompo;
        private IAimProvider _aimProvider;
        private bool _isAiming;

        public StateDataSO TargetState { get => targetState; set => targetState = value; }
        public SkillAnimType AnimType => SkillAnimType.Default;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _damageCalcCompo = container.Get<DamageCalcCompo>();
            _statCompo = container.Get<StatOverrideBehavior>();
            _aimProvider = container.GetSubclassComponent<IAimProvider>();
        }

        public override void StartAndUseSkill()
        {
            _isAiming = false;
            decalObject.SetParent(null);

            BombingMissile bombingMissile = _poolManager.Pop(bombingItemSO) as BombingMissile;
            bombingMissile.SetOwner(_owner);
            bombingMissile.CreateFloor = createFloor;
            bombingMissile.SlowAndAdditionalDamage = slowAndAdditionalDamage;
            bombingMissile.transform.position = new Vector3(decalObject.transform.position.x, 15, decalObject.transform.position.z);

            void HandleMissilePush()
            {
                decalObject.SetParent(transform);
                decalObject.gameObject.SetActive(false);
                bombingMissile.OnPush -= HandleMissilePush;
            }

            bombingMissile.OnPush += HandleMissilePush;

            DamageData damageData = _damageCalcCompo.CalculateDamage(_statCompo.GetStat(attackStat).Value, defaultDamageMultiplier + _statCompo.GetStat(damageModifier).Value
                , 0, DamageType.MELEE);
            bombingMissile.SetDamageData(damageData);
        }

        public void StartAiming()
        {
            decalObject.SetActive(true);
            _isAiming = true;
        }

        public void CancelSkill()
        {
            _isAiming = false;
            decalObject.SetActive(false);
        }

        private void Update()
        {
            if (_isAiming)
            {
                Vector3 targetPos = _aimProvider.GetAimPosition();
                decalObject.SetPos(targetPos);
            }
        }

        public void OnSkillTrigger()
        {
        }
    }
}

