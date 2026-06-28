using Ami.BroAudio;
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
        [SerializeField] private DecalObject decalObject;
        [SerializeField] private PoolItemSO bombingItemSO;
        [SerializeField] private StatSO attackStat;
        [SerializeField] private StatSO damageModifier;
        [SerializeField] private SoundID targetOnSound;
        [SerializeField] private float defaultDamageMultiplier = 1.25f;
        [SerializeField] private bool createFloor;
        [SerializeField] private bool slowAndAdditionalDamage;

        [SerializeField] private PoolManagerSO _poolManager;
        private DamageCalcCompo _damageCalcCompo;
        private StatOverrideBehavior _statCompo;
        private IAimProvider _aimProvider;
        private bool _isAiming;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _damageCalcCompo = container.Get<DamageCalcCompo>();
            _statCompo = container.Get<StatOverrideBehavior>();
            _aimProvider = container.GetSubclassComponent<IAimProvider>();
        }

        public override void StartSkill()
        {
            _isAiming = false;
            decalObject.SetParent(null);

            BombingMissile bombingMissile = _poolManager.Pop(bombingItemSO) as BombingMissile;
            Vector3 targetPoint = decalObject.transform.position;

            bombingMissile.SetOwner(_owner);
            bombingMissile.CreateFloor = createFloor;
            bombingMissile.SlowAndAdditionalDamage = slowAndAdditionalDamage;
            bombingMissile.transform.position = new Vector3(targetPoint.x, 15f, targetPoint.z);

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
            bombingMissile.BeginFall(targetPoint);
            StopTargetOnSound();
        }

        public void StartAiming()
        {
            BroAudio.Play(targetOnSound);
            decalObject.SetActive(true);
            _isAiming = true;
        }

        public void CancelSkill()
        {
            _isAiming = false;
            decalObject.SetActive(false);
            StopTargetOnSound();
        }

        private void StopTargetOnSound()
        {
            BroAudio.Stop(targetOnSound);
        }
        
        private void Update()
        {
            if (_isAiming)
            {
                Vector3 targetPos = _aimProvider.GetWorldAimPosition();
                decalObject.SetPos(targetPos);
            }
        }

        public override void OnSkillTrigger()
        {
        }
    }
}

