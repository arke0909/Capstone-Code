using Ami.BroAudio;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Chipmunk.Modules.StatSystem;
using Code.SHS.Entities.Enemies;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities;
using Scripts.Entities.Vitals;
using SHS.Scripts.Combats.Events;
using System;
using UnityEngine;
using Work.Code.GameEvents;

namespace Assets.Work.AKH.Scripts.Entities.Vitals
{
    public class HealthCompo : VitalManageCompo<HealthChangeEvent>, IDamageable
    {
        [SerializeField] private StatSO defStat, damageDemodifyStat,dropExpStat;
        [SerializeField] private SoundID hitSound;
        [SerializeField] private SoundID healSound;

        private ShieldCompo _shieldCompo;
        public event Action<float> OnTakeDamage;
        public event Action<DamageContext> OnHit;
        public event Func<DamageContext, bool> OnBeforeHit;

        public override void OnInitialize(ComponentContainer componentContainer)
        {
            base.OnInitialize(componentContainer);
            _shieldCompo = componentContainer.Get<ShieldCompo>();
        }
        protected override void Update()
        {
            base.Update();
            if (Input.GetKeyDown(KeyCode.End) && _entity is Enemy)
                ApplyDamage(new DamageData() { damage = 100 });
        }

        public override void AfterInitialize()
        {
            base.AfterInitialize();
            defStat = _statCompo.GetStat(defStat);
            dropExpStat = _statCompo.GetStat(dropExpStat);
            damageDemodifyStat = _statCompo.GetStat(damageDemodifyStat);
        }

        public bool Heal(float amount, bool playSfx = true)
        {
            if (_entity.IsDead || amount <= 0f)
                return false;

            float before = CurrentValue;
            CurrentValue += amount;

            bool healed = CurrentValue > before;
            if (healed && playSfx && healSound.IsValid())
                BroAudio.Play(healSound);

            return healed;
        }

        public void ApplyDamage(DamageData damageData, Vector3 hitPoint, Vector3 hitNormal, Entity dealer)
        {
            TakeDamage(damageData);
            _localEventBus.Raise(new DamagedEvent(_entity, damageData, hitPoint, hitNormal, dealer));
        }

        public void ApplyDamage(DamageContext context)
        {
            if (OnBeforeHit != null)
            {
                foreach (var func in OnBeforeHit.GetInvocationList())
                {
                    var del = (Func<DamageContext, bool>)func;
                    if (del(context))
                        return;
                }
            }

            if (TakeDamage(context.DamageData))
            {
                _localEventBus.Raise(new DamagedEvent(_entity,
                    context.DamageData, context.HitPoint,
                    context.HitNormal, context.Attacker));

                if (Mathf.Approximately(CurrentValue, 0))
                {
                    _entity.Dead();
                    if(context.Attacker !=null && dropExpStat !=null)
                        context.Attacker.OnKill?.Invoke(dropExpStat.Value);
                    _localEventBus.Raise(new EntityDeadEvent(_entity, context.HitPoint, context.HitNormal));
                }

                if (hitSound.IsValid())
                    BroAudio.Play(hitSound);

                OnHit?.Invoke(context);
            }
        }

        private bool TakeDamage(DamageData damageData)
        {
            if (_entity.IsDead)
                return false;
            float finalDefModify = 2 / (Mathf.Max(defStat.Value - damageData.defPierceLevel, 0) + 2);
            float finalDamage = damageData.damage * finalDefModify * damageDemodifyStat.Value;

            if (_shieldCompo != null)
            {
                finalDamage = _shieldCompo.DamageDecreaseByShield(finalDamage);
            }

            CurrentValue -= finalDamage;
            OnTakeDamage?.Invoke(finalDamage);
            _entity.OnHitEvent?.Invoke();
            EventBus.Raise(new DamageTextEvent(finalDamage, transform.position));
            return true;
        }

        public void ApplyDamage(DamageData damageData, Entity dealer = null)
        {
            Vector3 hitPoint = transform.position;
            Vector3 hitNormal = Vector3.zero;
            DamageContext context = new DamageContext
            {
                DamageData = damageData,
                HitPoint = hitPoint,
                HitNormal = hitNormal,
                Attacker = dealer
            };
            if (dealer)
            {
                hitNormal = (dealer.transform.position - transform.position).normalized;

                if (TryGetComponent<Collider>(out var col))
                    hitPoint = col.ClosestPoint(dealer.transform.position);
                context.Source = dealer.gameObject;
            }


            ApplyDamage(context);
        }
    }
}
