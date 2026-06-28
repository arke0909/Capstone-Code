using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Work.Code.GameEvents;

namespace Code.SHS.Entities.Enemies
{
    public class Boss : Enemy
    {

        public override void OnInitialize(ComponentContainer componentContainer)
        {
            base.OnInitialize(componentContainer);
        }

        public override void Dead()
        {
            base.Dead();
            EventBus.Raise(new DefeatBossEvent());
        }
    }
}
