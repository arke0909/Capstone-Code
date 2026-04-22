using Code.SkillSystem;
using UnityEngine;

namespace Scripts.SkillSystem
{
    [CreateAssetMenu(fileName ="SkillDB",menuName ="SO/Skill/DB")]
    public class SkillDBSO : ScriptableObject
    {
        public SkillDataSO[] skillDatas;
        public SkillDataSO GetRandomSkill()
        {
            return skillDatas[Random.Range(0, skillDatas.Length)];
        }
    }
}
