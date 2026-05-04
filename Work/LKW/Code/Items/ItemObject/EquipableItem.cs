using Chipmunk.ComponentContainers;
using Chipmunk.Modules.StatSystem;
using Code.SkillSystem;
using Scripts.Combat.ItemObjects;
using Scripts.Entities;
using Scripts.SkillSystem.Manage;
using UnityEngine;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.LKW.Code.Items
{
    public abstract class EquipableItem : ItemBase, IEquipable
    {
        public ItemObject ItemObject;
        public SkillDataSO Skill { get; private set; }
        public EquipItemDataSO EquipItemData { get; protected set; }
        public bool IsEquipped { get; set; }
        public int SkillLevel { get; private set; } = 1;
        private SkillManager _skillManager;
        private StatOverrideBehavior _statCompo;

        public EquipableItem(ItemDataSO itemData) : base(itemData)
        {
            ItemData = itemData;
            EquipItemData = ItemData as EquipItemDataSO;
            Skill = EquipItemData.skillDB?.GetRandomSkill();
        }

        public virtual void OnEquip(Entity entity, Transform parent)
        {
            IsEquipped = true;
            GameObject go = GameObject.Instantiate(EquipItemData.equipmentPrefab, parent);
            go.transform.localPosition = EquipItemData.modelOffset;
            ItemObject = go.GetComponent<ItemObject>();
            ItemObject.InitObject(entity, this);

            _statCompo = entity.Get<StatOverrideBehavior>();
            
            if (_statCompo != null)
            {
                foreach (var addStat in EquipItemData.addStats)
                {
                    _statCompo.AddModifier(addStat.targetStat, this,addStat.value);
                }
            }
        }

        public virtual void OnUnequip(Entity entity)
        {
            IsEquipped = false;
            GameObject.Destroy(ItemObject.gameObject);
            ItemObject = null;
            
            if (_statCompo != null)
            {
                foreach (var addStat in EquipItemData.addStats)
                {
                    _statCompo.RemoveModifier(addStat.targetStat, this);
                }
            }
           
        }
        public virtual bool LevelUpSkill()
        {
            if (_owner == null || !_owner.TryGet(out SkillManager skillManager) || Skill == null)
                return false;
            if (skillManager.TryGetSkill(Skill, out Scripts.SkillSystem.Skill skill)
                && skill.SetLevel(SkillLevel + 1))
            {
                SkillLevel += 1;
                return true;
            }
            return false;
        }

        public void RegisterSkill()
        {
            _skillManager = _owner.Get<SkillManager>();
            
            if (_skillManager != null)
            {
                _skillManager.AddSkill(Skill);
            
                if (_skillManager.TryGetSkill(Skill, out Scripts.SkillSystem.Skill skill))
                {
                    skill.SetLevel(SkillLevel);
                }
            }
        }

        public void SetSkill(SkillDataSO skill, int level = 1)
        {
            bool wasRegistered = _skillManager != null;

            if (wasRegistered)
                DeregisterSkill();

            Skill = skill;
            SkillLevel = Mathf.Max(1, level);

            if (wasRegistered)
                RegisterSkill();
        }

        public void CopySkillFrom(EquipableItem source)
        {
            if (source == null)
                return;

            SetSkill(source.Skill, source.SkillLevel);
        }

        public void DeregisterSkill()
        {
            _skillManager?.RemoveSkill(Skill);
            _skillManager = null;
        }
    }
}
