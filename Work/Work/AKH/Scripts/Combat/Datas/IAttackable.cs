using Work.Code.Entities;

namespace Scripts.Combat.Datas
{
    public enum AttackableState
    {
        CanAttack, NeedAmmo, NeedStack, NotEquipped, Delayed
    }
    public interface IAttackable : IDamageDelaer
    {
        public AttackableState CurrentAttackableState { get; }
        public void EnterAttack();
        public void AttackTrigger();
        public void EndAnimation();
        public bool CanAttack() => CurrentAttackableState == AttackableState.CanAttack;
    }
}
