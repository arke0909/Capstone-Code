using Code.Items.ItemInfo;
using Code.SkillSystem;
using Code.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Work.Code.Skills;

namespace Code.UI.Tooltip
{
    public class SkillTooltipData
    {
        public SkillDataSO SkillData { get; }
        public int Level { get; }

        public SkillTooltipData(SkillDataSO skillData, int level)
        {
            SkillData = skillData;
            Level = level;
        }
    }

    public class SkillTooltip : BaseTooltip<SkillTooltipData>
    {
        [SerializeField] private TextMeshProUGUI skillName;
        [SerializeField] private TextMeshProUGUI skillDescription;
        [SerializeField] private Image skillIcon;

        private SkillUpgradeUI[] _skillUpgradeUis;

        public override int SortOrder => 10;

        protected override void Awake()
        {
            base.Awake();
            _skillUpgradeUis = GetComponentsInChildren<SkillUpgradeUI>(true);
        }

        protected override void ShowTooltip(SkillTooltipData tooltipData)
        {
            SkillDataSO data = tooltipData.SkillData;
            skillName.text = data.skillName;
            skillDescription.text = data.skillDescription;
            skillIcon.sprite = data.skillIcon;

            for (int i = 1; i <= (int)Rarity.Epic; i++)
            {
                var upgradeUI = _skillUpgradeUis[i - 1];
                upgradeUI.DisableUI();

                if (_skillUpgradeUis.Length < i || upgradeUI == null ||
                    data.upgradeList.Count <= i || data.upgradeList[i] == null)
                    continue;

                upgradeUI.SkillUpgradeName.text = $"레벨{i} | {data.upgradeList[i].UpgradeTitle}";
                upgradeUI.SkillUpgradeDescription.text = $"{data.upgradeList[i].upgradeDescription}";
                upgradeUI.SetColor(tooltipData.Level > i ? UIDefine.GreenColor : Color.white);
                upgradeUI.EnableUI();
            }
        }
    }
}
