using Code.InventorySystems;
using Code.InventorySystems.Equipments;
using Code.Items;
using Code.SHS.Entities.Enemies;
using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace SHS.Scripts.Entities.Levels.Growths
{
    [CreateAssetMenu(fileName = "BulletGrowthSO", menuName = "ScriptableObject/BulletGrowthSO")]
    public class BulletGrowthSO : BaseGrowthSO
    {
        [SerializeField] private BulletDataSO[] _bulletDataByGunType;
        [FormerlySerializedAs("_bulletData")]
        [SerializeField] private BulletDataSO _fallbackBulletData;
        [SerializeField] private bool _replaceCurrentMagazineImmediately = true;

        protected override void ApplyGrowthEffect(Entity entity)
        {
            if (entity is not Enemy enemy)
                return;

            TryGetSpawnGunData(enemy, out GunDataSO spawnGunData);
            if (!TryGetGrowthBullet(spawnGunData, out BulletDataSO bulletData))
                return;

            enemy.SetRuntimeBulletData(bulletData);

            if (!_replaceCurrentMagazineImmediately ||
                !TryGetEquippedGun(enemy, out GunItem equippedGun) ||
                !CanReplaceEquippedGunBullet(spawnGunData, equippedGun, bulletData))
            {
                return;
            }

            ReplaceEquippedGunBullet(enemy, equippedGun, bulletData);
        }

        private bool TryGetGrowthBullet(GunDataSO gunData, out BulletDataSO bulletData)
        {
            bulletData = null;

            if (gunData == null)
            {
                bulletData = _fallbackBulletData;
                return bulletData != null;
            }

            bulletData = FindBulletData(gunData.gunType);
            if (bulletData != null)
                return true;

            Debug.LogWarning($"{name} has no bullet data for spawn gun type {gunData.gunType}.");
            return false;
        }

        private BulletDataSO FindBulletData(GunType gunType)
        {
            if (_bulletDataByGunType != null)
            {
                foreach (BulletDataSO bulletData in _bulletDataByGunType)
                {
                    if (bulletData != null && bulletData.gunType == gunType)
                        return bulletData;
                }
            }

            if (_fallbackBulletData != null && _fallbackBulletData.gunType == gunType)
                return _fallbackBulletData;

            return null;
        }

        private bool TryGetSpawnGunData(Enemy enemy, out GunDataSO gunData)
        {
            gunData = null;

            if (enemy.EnemyData?.equipments != null)
            {
                foreach (var equipData in enemy.EnemyData.equipments)
                {
                    if (equipData.partType == EquipPartType.Hand && equipData.itemData is GunDataSO spawnGunData)
                    {
                        gunData = spawnGunData;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool CanReplaceEquippedGunBullet(GunDataSO spawnGunData, GunItem equippedGun, BulletDataSO bulletData)
        {
            if (equippedGun?.GunItemData == null || bulletData == null)
                return false;

            if (spawnGunData != null && equippedGun.GunItemData != spawnGunData)
                return false;

            return equippedGun.GunItemData.gunType == bulletData.gunType;
        }

        private bool TryGetEquippedGun(Enemy enemy, out GunItem gun)
        {
            gun = null;
            if (enemy.ComponentContainer == null ||
                !enemy.ComponentContainer.TryGetSubclassComponent(out EntityEquipment equipment) ||
                !equipment.TryGetEquippedItem(EquipPartType.Hand, out EquipableItem equippedItem) ||
                equippedItem is not GunItem equippedGun)
            {
                return false;
            }

            gun = equippedGun;
            return true;
        }

        private void ReplaceEquippedGunBullet(Enemy enemy, GunItem gun, BulletDataSO bulletData)
        {
            Inventory inventory = enemy.ComponentContainer.GetSubclassComponent<Inventory>();
            BulletItem bulletItem = bulletData.CreateItem().Item as BulletItem;
            if (inventory == null || bulletItem == null)
                return;

            int bulletCount = Mathf.Max(1, gun.GunItemData.maxAmmoCapacity);
            if (!inventory.TryAddItem(bulletItem, bulletCount))
            {
                Debug.LogWarning($"{name} could not add replacement bullets to {enemy.name}'s inventory.", enemy);
                return;
            }

            gun.ChangeBullet(bulletItem);
            gun.Reload();
        }
    }
}
