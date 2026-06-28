using Chipmunk.ComponentContainers;
using Chipmunk.Modules.StatSystem;
using Code.Items.ItemInfo;
using Code.SkillSystem;
using Scripts.Combat.ItemObjects;
using Scripts.Entities;
using Scripts.SkillSystem.Manage;
using UnityEngine;

namespace Code.Items
{
    public abstract class EquipableItem : ItemBase, IEquipable
    {
        public ItemObject ItemObject;
        public SkillDataSO Skill { get; private set; }
        public EquipItemDataSO EquipItemData { get; protected set; }
        public bool IsEquipped { get; set; }

        // 스킬 레벨은 장비 등급(Rarity)과 연동: Common=1, Rare=2, Epic=3
        public int SkillLevel => (int)EquipItemData.rarity + 1;

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

            _statCompo = entity.Get<StatOverrideBehavior>();
            
            if (_statCompo != null)
            {
                foreach (var addStat in EquipItemData.addStats)
                {
                    _statCompo.AddModifier(addStat.targetStat, this, addStat.value);
                }
            }
        }

        protected void InitItemObject(Entity entity, Transform parent)
        {
            GameObject go = GameObject.Instantiate(EquipItemData.equipmentPrefab, parent);
            ItemObject = go.GetComponent<ItemObject>();
            ItemObject.InitObject(entity, this);
        }

        public virtual void OnUnequip(Entity entity)
        {
            IsEquipped = false;

            if (_statCompo != null)
            {
                foreach (var addStat in EquipItemData.addStats)
                {
                    _statCompo.RemoveModifier(addStat.targetStat, this);
                }
            }
        }

        protected void DestroyItemObject()
        {
            GameObject.Destroy(ItemObject.gameObject);
            ItemObject = null;
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

        // 장비 업그레이드 시 스킬 종류를 계승할 때 사용.
        // 스킬 레벨은 이 아이템의 Rarity에서 자동 결정되므로 별도 전달 불필요.
        public void SetSkill(SkillDataSO skill)
        {
            bool wasRegistered = _skillManager != null;

            if (wasRegistered)
                DeregisterSkill();

            Skill = skill;

            if (wasRegistered)
                RegisterSkill();
        }

        // 장비 업그레이드(제작) 시 소재 장비의 스킬 종류를 결과 장비에 계승.
        // 레벨은 결과 장비의 Rarity로 자동 결정된다.
        public void CopySkillFrom(EquipableItem source)
        {
            if (source == null)
                return;

            SetSkill(source.Skill);
        }

        public void DeregisterSkill()
        {
            _skillManager?.RemoveSkill(Skill);
            _skillManager = null;
        }
    }
}
