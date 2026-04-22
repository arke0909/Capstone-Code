using System.Collections.Generic;
using Code.SkillSystem.Upgrade;
using Scripts.SkillSystem;
using UnityEngine;

namespace Code.SkillSystem
{
    [CreateAssetMenu(fileName = "Skill data", menuName = "SO/Skill/Data", order = 0)]
    public class SkillDataSO : ScriptableObject
    {
        public string skillName;
        public Sprite skillIcon;
        [TextArea] public string skillDescription;
        public List<SkillUpgradeSO> upgradeList;
        public GameObject skillPrefab;
        public bool defaultSkill;
    }
}