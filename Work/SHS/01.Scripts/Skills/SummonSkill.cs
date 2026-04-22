using System;
using Scripts.SkillSystem.Skills;
using Scripts.FSM;
using Scripts.SkillSystem;
using SHS.Scripts.Summon;
using UnityEngine;

namespace SHS.Scripts.Skills
{
    public class SummonSkill : ActiveSkill,IUseStateSkill
    {
        [SerializeField] private GameObject summonPrefab;
        [SerializeField] private Transform summonTransform;

        [field: SerializeField] public SkillAnimType AnimType { get; private set; }
        [field:SerializeField] public StateDataSO TargetState { get; set; }

        private void OnValidate()
        {
            Debug.Assert(summonPrefab.GetComponent<ISummonable>() != null,
                "SummonPrefab does not implement ISummonable");
        }
        public void OnSkillTrigger()
        {
            Summon();

        }
        private GameObject Summon()
        {
            GameObject summon = null;
            if (summonTransform == null)
            {
                summon = Instantiate(summonPrefab, transform.position, transform.rotation);
            }
            else
            {
                summon = Instantiate(summonPrefab, summonTransform.position, summonTransform.rotation);
            }

            return summon;
        }


    }
}
