using Code.UI.Core;
using TMPro;
using UnityEngine;

namespace Work.Code.Skills
{
    public class SkillUpgradeUI : LayoutUIBase
    {
        [field: SerializeField] public TextMeshProUGUI SkillUpgradeName { get; set; }
        [field: SerializeField] public TextMeshProUGUI SkillUpgradeDescription { get; set; }

        public void SetColor(Color color)
        {
            SkillUpgradeName.color = color;
            SkillUpgradeDescription.color = color;
        }
    }
}