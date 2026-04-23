using Chipmunk.Library.Utility.GameEvents.Local;

namespace Scripts.Entities.Vitals
{
    public interface IVitalEvent : ILocalEvent
    {
        void Init(float value, float maxValue);
    }
    public struct HealthChangeEvent : IVitalEvent
    {
        public float CurrentHealth { get; set; }
        public float MaxHealth { get; set; }

        public void Init(float value, float maxValue)
        {
            CurrentHealth = value;
            MaxHealth = maxValue;
        }
    }
    public struct FoodChangeEvent : IVitalEvent
    {
        public float CurrentFood { get; set; }
        public float MaxFood { get; set; }

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

        public void Init(float prev, float current)
        {
            PrevExp = prev;
            CurrentExp = current;
        }
    }
}