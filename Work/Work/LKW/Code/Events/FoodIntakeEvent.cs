using Chipmunk.GameEvents;

namespace Work.LKW.Code.Events
{
    public struct FoodIntakeEvent : IEvent
    {
        public int FoodAmount { get; }
        public int WaterAmount { get; }

        public FoodIntakeEvent(int foodAmount, int waterAmount)
        {
            FoodAmount = foodAmount;
            WaterAmount = waterAmount;
        }
    }
}