using Work.Code.Entities;

namespace Scripts.Combat.Datas
{
    public enum AttackableState
    {
        CanAttack, NeedAmmo, NeedStack, NotEquipped, Delayed
    }

    public readonly struct AttackContext
    {
        public bool WantsAttack { get; }
        public bool AnimationEnded { get; }
        public bool IsAiming { get; }

        public AttackContext(bool wantsAttack, bool animationEnded, bool isAiming)
        {
            WantsAttack = wantsAttack;
            AnimationEnded = animationEnded;
            IsAiming = isAiming;
        }
    }

    public interface IAttackable : IDamageDelaer
    {
        public AttackableState CurrentAttackableState { get; }
        public bool UsesAnimationAttackTrigger { get; }
        public void EnterAttack();
        public void AttackTrigger();
        public void UpdateAttack(AttackContext context);
        public void EndAttack();
        public bool CanAttack() => CurrentAttackableState == AttackableState.CanAttack;
    }
}
