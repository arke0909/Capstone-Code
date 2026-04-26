using Scripts.Combat.Datas;

namespace SHS.Scripts
{
    public interface IProjectileShooter
    {
        float DefaultDamage { get; }
        float ProjectileSpeed { get; }
        float DamageMultiplier { get; }
        int DefPierceLevel { get; }
    }
}