using Chipmunk.Modules.StatSystem;
using TMPro;
using UnityEngine;
using Work.Code.UI.Utility;
using Work.LKW.Code.Items.ItemInfo;

namespace Code.UI.Tooltip
{
    public class ArmorItemTooltip : ItemSlotTooltip<ArmorDataSO>
    {
        [SerializeField] private TextMeshProUGUI[] statText;
        [SerializeField] private StatSO healthStat;
        [SerializeField] private StatSO defenseStat;
        [SerializeField] private StatSO capacityStat;
        
        protected override void ShowData(ArmorDataSO data)
        {
            for (int i = 0; i < data.addStats.Length; i++)
            {
                statText[i].gameObject.SetActive(true);
                BindStat(statText[i], GetStatType(data.addStats[i].targetStat), data.addStats[i].value);
            }
        }

        public StatType GetStatType(StatSO stat)
        {
            if (stat == healthStat) return StatType.Health;
            if (stat == defenseStat) return StatType.Defense;
            if (stat == capacityStat) return StatType.Capacity;
            return StatType.Health;
        }

        public override void DisableUI(bool hasTween = false)
        {
            base.DisableUI(hasTween);
            foreach (var stat in statText)
            {
                stat.gameObject.SetActive(false);
            }
        }
    }
}