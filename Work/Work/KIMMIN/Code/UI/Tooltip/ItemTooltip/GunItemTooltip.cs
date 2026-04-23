using Scripts.Combat.Datas;
using TMPro;
using UnityEngine;
using Work.Code.UI.Utility;
using Work.LKW.Code.Items.ItemInfo;

namespace Code.UI.Tooltip
{
    public class GunItemTooltip : ItemSlotTooltip<GunDataSO>
    {
        [SerializeField] private TextMeshProUGUI damage;
        [SerializeField] private TextMeshProUGUI firerate;
        [SerializeField] private TextMeshProUGUI bulletCount;
        [SerializeField] private TextMeshProUGUI fireCount;
        [SerializeField] private TextMeshProUGUI spread;
            
        protected override void ShowData(GunDataSO data)
        {
            BindStat(damage, StatType.Damage, data.defaultDamage);
            BindStat(firerate, StatType.FireRate, data.fireRate);
            BindStat(fireCount, StatType.FireCount, data.bulletPerShot);
            BindStat(bulletCount, StatType.BulletCount, data.maxAmmoCapacity);
            BindStat(spread, StatType.Spread, data.defaultSpread);
        }
    }
}