using TMPro;
using UnityEngine;
using Work.LKW.Code.Items.ItemInfo;

namespace Code.UI.Tooltip
{
    public class WeaponItemTooltip : ItemSlotTooltip<WeaponDataSO>
    {
        [SerializeField] private TextMeshProUGUI durabilityText;
        [SerializeField] private TextMeshProUGUI defaultDamageText;
        [SerializeField] private TextMeshProUGUI attackDelayText;
        
        
        protected override void ShowData(WeaponDataSO data)
        {
            durabilityText.text = $"내구성 : {data.maxDurability}";
            defaultDamageText.text = $"공격력 : {data.defaultDamage}";
            attackDelayText.text = $"발사 속도 : {data.attackSpeed}";
        }
    }
}