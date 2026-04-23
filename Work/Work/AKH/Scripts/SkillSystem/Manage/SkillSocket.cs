using Code.SkillSystem;
using Scripts.SkillSystem;
using System;
using UnityEngine;

namespace Scripts.SkillSystem.Manage
{
    [Serializable]
    public abstract class SkillSocket
    {
        public event Action<SkillDataSO> OnChange;

        [field: SerializeField] public Skill CurrentSkill { get; private set; }
        public virtual void ChangeItem(Skill newSkill)
        {
            CurrentSkill = newSkill;
            if (CurrentSkill == null)
            {
                OnChange?.Invoke(null);
            }
            else
            {
                OnChange?.Invoke(CurrentSkill.SkillData);
            }
        }
        public void Reload()
        {
            ChangeItem(CurrentSkill);
        }
    }
}
