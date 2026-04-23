using Scripts.Combat.Datas;

namespace SHS.Scripts
{
    public interface IBulletShooter
    {
        float DefaultDamage { get; }
        float ProjectileSpeed { get; }
        BulletDataSO BulletData { get; }
    }
}