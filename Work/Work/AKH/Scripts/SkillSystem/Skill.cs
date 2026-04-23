using Scripts.SkillSystem.Manage;
using Chipmunk.ComponentContainers;
using Code.SkillSystem;
using Code.SkillSystem.Upgrade;
using Scripts.Entities;
using UnityEngine;

namespace Scripts.SkillSystem
{
    public abstract class Skill : MonoBehaviour
    {
        [field: SerializeField] public SkillDataSO SkillData { get; private set; }
        public int Level
            => _level;

        public bool SetLevel(int value)
        {
            if (value <= 0 || value > SkillData.upgradeList.Count)
            {
                Debug.LogWarning($"Skill Level must be between 1 and {SkillData.upgradeList.Count} but {value}");
                return false;
            }
            
            if (value < _level)
            {
                for (int i = _level - 1; i >= value; i--)
                {
                    SkillUpgradeSO upgradeSO = SkillData.upgradeList[i];
                    upgradeSO?.RollbackSkill(this);
                }
                
                _level = value;
                return true;
            }
            else if (value > _level)
            {
                for (int i = _level; i < value; i++)
                {
                    
                    SkillUpgradeSO upgradeSO = SkillData.upgradeList[i];
                    upgradeSO?.UpgradeSkill(this);
                }

                _level = value;
                return true;
            }

            return false;
        }

        protected ComponentContainer _container;
        protected Entity _owner;
        private int _level = 1;
        public virtual void Init(ComponentContainer container)
        {
            _container = container;
            _owner = _container.Get<Entity>(true);

        }
        public abstract SkillType SkillType { get; }
    }
}
