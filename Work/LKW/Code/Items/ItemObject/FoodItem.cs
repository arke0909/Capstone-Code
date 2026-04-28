using Scripts.Entities;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.LKW.Code.Items
{
    public class FoodItem : UsableItem
    {
        public int StaminiaAmount { get; set; }
        
        public FoodItem(ItemDataSO itemData,int staminaAmount) : base(itemData)
        {
            StaminiaAmount = staminaAmount;
        }

        public override void Use(Entity user)
        {
            base.Use(user);
            //if (user.TryGet<WaterCompo>(out var waterCompo))
            //    user.Get<WaterCompo>().CurrentValue += WaterAmount;
            //if (user.TryGet<FoodCompo>(out var foodCompo))
            //    user.Get<FoodCompo>().CurrentValue += FoodAmount;
            //여기서 발생한 이벤트 받아서 아이템에따른 음식량 수분량 증가 또는 감소 시켜주면됨
        }
    }
}