using Chipmunk.ComponentContainers;
using Scripts.Entities.Vitals;
using System;

namespace Work.Code.Entities.Experiencement
{
    public class ExpCompo : VitalManageCompo<ExpChangeEvent> 
    {
        public override void OnInitialize(ComponentContainer componentContainer)
        {
            base.OnInitialize(componentContainer);
            _entity.OnKill += HandleKillEnemy;
        }
        public override void OnDestroy()
        {
            _entity.OnKill -= HandleKillEnemy;
            base.OnDestroy();
        }
        private void HandleKillEnemy(float exp)
        {
            CurrentValue += exp;
        }
    }
}