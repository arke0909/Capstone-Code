using System;
using System.Collections.Generic;
using Chipmunk.ComponentContainers;
using UnityEngine;

namespace Scripts.Combat
{
    [Serializable]
    public class ShieldInstance
    {
        public float shieldAmount;
        public bool isBroken;
        public event Action OnBroken;
        
        public ShieldInstance(float shieldAmount)
        {
            this.shieldAmount = Mathf.Max(shieldAmount, 0);
        }

        public float Consume(float damage)
        {
            if (damage <= 0 || shieldAmount <= 0)
                return damage;

            float consumedAmount = Mathf.Min(shieldAmount, damage);
            shieldAmount -= consumedAmount;

            return damage - consumedAmount;
        }

        public void Remove()
        {
            OnBroken?.Invoke();
            OnBroken = null;
        }
    }

    public class ShieldCompo : MonoBehaviour, IContainerComponent
    {
        private const float EmptyThreshold = 0.001f;

        [SerializeField] private List<ShieldInstance> shields = new List<ShieldInstance>();

        public ComponentContainer ComponentContainer { get; set; }
        public event Action<float> OnShieldAmountChanged;

        public float CurrentShieldAmount
        {
            get
            {
                float total = 0;

                for (int i = 0; i < shields.Count; i++)
                {
                    if (shields[i] == null) continue;
                    total += shields[i].shieldAmount;
                }

                return total;
            }
        }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            ComponentContainer = componentContainer;
            RemoveEmptyShields();
        }

        public ShieldInstance AddShield(float shieldAmount, Action onBroken = null)
        {
            ShieldInstance shield = new ShieldInstance(shieldAmount);
            shield.OnBroken += onBroken;
            Debug.Log(shieldAmount);
            AddShield(shield);
            return shield;
        }

        public void AddShield(ShieldInstance shield)
        {
            if (shield == null || shield.shieldAmount <= EmptyThreshold)
                return;

            shields.Add(shield);
            NotifyShieldChanged();
        }

        public void RemoveShield(ShieldInstance shield)
        {
            if (shield == null)
                return;

            if (shields.Remove(shield))
            {
                shield.Remove();
                NotifyShieldChanged();
            }
        }

        public void ClearShield()
        {
            if (shields.Count == 0)
                return;

            foreach (var shield in shields)
            {
                shield?.Remove();
            }
            
            shields.Clear();
            NotifyShieldChanged();
        }

        public float DamageDecreaseByShield(float damage)
        {
            if (damage <= 0)
                return 0;

            if (shields.Count == 0)
                return damage;

            for (int i = shields.Count - 1; i >= 0 && damage > EmptyThreshold; i--)
            {
                ShieldInstance shield = shields[i];

                if (shield == null || shield.shieldAmount <= 0)
                {
                    RemoveShield(i);
                    continue;
                }

                damage = shield.Consume(damage);
            }

            if (damage <= EmptyThreshold)
            {
                damage = 0;
            }

            NotifyShieldChanged();
            return damage;
        }

        private void RemoveEmptyShields()
        {
            for (int i = shields.Count - 1; i >= 0; i--)
            {
                if (shields[i] == null || shields[i].shieldAmount <= 0)
                {
                    RemoveShield(i);
                }
            }
        }

        private void RemoveShield(int i)
        {
            var shield = shields[i];
            shield.Remove();
            shields.RemoveAt(i);
        }

        private void NotifyShieldChanged()
        {
            RemoveEmptyShields();
            OnShieldAmountChanged?.Invoke(CurrentShieldAmount);
        }
    }
}