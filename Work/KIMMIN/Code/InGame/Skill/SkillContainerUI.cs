using System;
using Scripts.SkillSystem.UI;
using UnityEngine;

namespace Work.Code.Skills
{
    public class SkillContainerUI : MonoBehaviour
    {
        private ActiveSkilUI[] _skillUis;

        private void Awake()
        {
            _skillUis = GetComponentsInChildren<ActiveSkilUI>();
        }
    }
}