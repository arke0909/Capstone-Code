using Chipmunk.Library.Utility.GameEvents.Local;

namespace Scripts.Entities.Vitals
{
    public interface IVitalEvent : ILocalEvent
    {
        float Value { get; }
        float MaxValue { get; }
        void Init(float value, float maxValue);
    }
    public struct HealthChangeEvent : IVitalEvent
    {
        public float CurrentHealth { get; set; }
        public float MaxHealth { get; set; }

        public float Value => CurrentHealth;

        public float MaxValue => MaxHealth;

        public void Init(float value, float maxValue)
        {
            CurrentHealth = value;
            MaxHealth = maxValue;
        }
    }

    public readonly struct HealthShieldChangeEvent : ILocalEvent
    {
        public float CurrentHealth { get; }
        public float MaxHealth { get; }
        public float CurrentShield { get; }

        public HealthShieldChangeEvent(float currentHealth, float maxHealth, float currentShield)
        {
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            CurrentShield = currentShield;
        }
    }

    public readonly struct HealthShieldStateRequestEvent : ILocalEvent
    {
    }

    public struct FoodChangeEvent : IVitalEvent
    {
        public float CurrentFood { get; set; }
        public float MaxFood { get; set; }

        public float Value => CurrentFood;

        public float MaxValue => MaxFood;

        public void Init(float value, float maxValue)
        {
            CurrentFood = value;
            MaxFood = maxValue;
        }
    }
    public struct WaterChangeEvent : IVitalEvent
    {
        public float CurrentWater { get; set; }
        public float MaxWater { get; set; }

        public float Value => CurrentWater;

        public float MaxValue => MaxWater;

        public void Init(float value, float maxValue)
        {
            CurrentWater = value;
            MaxWater = maxValue;
        }
    }
    public struct StaminaChangeEvent : IVitalEvent
    {
        public float CurrentStamina { get; set; }
        public float MaxStamina { get; set; }

        public float Value => CurrentStamina;

        public float MaxValue => MaxStamina;

        public void Init(float value, float maxValue)
        {
            CurrentStamina = value;
            MaxStamina = maxValue;
        }
    }
    
    public struct ExpChangeEvent : IVitalEvent
    {
        public float PrevExp { get; set; }
        public float CurrentExp { get; set; }

        public float Value => PrevExp;

        public float MaxValue => CurrentExp;

        public void Init(float prev, float current)
        {
            PrevExp = prev;
            CurrentExp = current;
        }
    }
}
