using Code.SkillSystem;
using Scripts.SkillSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Tooltip
{
    public class SkillTooltip : BaseTooltip<SkillDataSO>
    {
        [SerializeField] private TextMeshProUGUI skillName;
        [SerializeField] private TextMeshProUGUI skillDescription;
        [SerializeField] private Image skillIcon;
        
        public override int SortOrder => 10;
        
        protected override void ShowTooltip(SkillDataSO data)
        {
            skillName.text = data.skillName;
            skillDescription.text = data.skillDescription;
            skillIcon.sprite = data.skillIcon;
        }
    }
}