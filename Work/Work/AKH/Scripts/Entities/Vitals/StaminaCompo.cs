using Scripts.Players;
using UnityEngine;

namespace Scripts.Entities.Vitals
{
    public class StaminaCompo : VitalManageCompo<StaminaChangeEvent>
    {
        protected override void Update()
        {
            base.Update();
            if (Input.GetKeyDown(KeyCode.Backspace) && _entity is Player)
                _statCompo.AddModifier(ManageStat,"Cheat", 10000f);
        }
    }
}
