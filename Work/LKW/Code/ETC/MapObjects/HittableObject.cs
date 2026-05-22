using System;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace Code.ETC.MapObjects
{
    public abstract class HittableObject : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHp = 10f;
        private float _currentHp;
        
        public event Action<float> OnTakeDamage;

        protected float CurrentHp => _currentHp;
        protected float MaxHp => maxHp;
        protected bool IsDead => _currentHp <= 0f;
        
        protected virtual void Awake()
        {
            _currentHp = maxHp;
        }

        protected void SetMaxHp(float value, bool refill = true)
        {
            maxHp = value;
            if (refill) _currentHp = maxHp;
        }

        public virtual void ApplyDamage(DamageData damageData, Entity dealer = null)
        {
            if (IsDead) return;
            
            _currentHp = Mathf.Max(_currentHp - damageData.damage, 0f);
            OnTakeDamage?.Invoke(damageData.damage);
            TakeHit();
            if (_currentHp <= 0f) OnDeath();
        }

        public virtual void TakeHit()
        {
        }
        

        public void ApplyDamage(DamageContext context)
        {
            ApplyDamage(context.DamageData, context.Attacker);
        }

        protected void Kill()
        {
            if (IsDead) return;
            ApplyDamage(new DamageData { damage = _currentHp });
        }

        protected abstract void OnDeath();
    }
}
