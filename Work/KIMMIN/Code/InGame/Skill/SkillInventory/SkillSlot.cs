using System;
using Scripts.SkillSystem.Manage;
using Code.UI.Core;
using DG.Tweening;
using Scripts.SkillSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Work.Code.UI.Interaction;

namespace Work.Code.SkillInventory
{
    public class SkillSlot : DraggableUI, IUIElement<Skill>, IDroppable
    {
        [SerializeField] private ActiveSlotType slotType;
        [SerializeField] private GameObject background;
        [SerializeField] private Image skillIcon;
        [SerializeField] private Image outline;

        private readonly Color32 _highlightColor = new(150, 255, 150, 255);

        public int Index { get; set; }
        public bool IsInventorySlot { get; set; }
        public SkillType SkillType { get; set; }
        public Skill CurrentSkill { get; private set; }
        public override Sprite DragSprite => CurrentSkill?.SkillData?.skillIcon;
        
        public event Action<SkillSlot, SkillSlot> OnDropSkill;
        public event Action<Skill, int> OnEquipped;
        public event Action<Skill> OnUnequipped;
        

        public void EnableFor(Skill skill)
        {
            if (skill == null || skill.SkillData == null)
            {
                Clear();
                return;
            };
            
            background.SetActive(true);
            skillIcon.sprite = skill.SkillData.skillIcon;
            SkillType = skill.SkillType;
            CurrentSkill = skill;

            if (!IsInventorySlot)
            {
                OnEquipped?.Invoke(skill, Index);
            }
            
            UnbindTooltip();
            BindTooltip(() => skill.SkillData);
        }
        
        public void HighlightUI(bool isHighlight)
        {
            if(background == null || outline == null) return;   
            background.transform.DOKill();
            outline.transform.DOKill();

            float scale = isHighlight ? 1.1f : 1f;
            Color32 color = isHighlight ? 
                _highlightColor : Color.white;
            
            transform.DOScale(scale, 0.1f);
            outline.DOColor(color, 0.1f);
        }

        public void Clear()
        {
            UnbindTooltip();
            OnUnequipped?.Invoke(CurrentSkill);
            background.SetActive(false);
            CurrentSkill = null;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!eventData.pointerDrag.TryGetComponent<SkillSlot>(out var other)) return;
            OnDropSkill?.Invoke(this, other);
        }

        public void SetEquip(Skill skill)
        {
            if (skill == null)
                Clear();
            else 
                EnableFor(skill);
        }

        public override bool CanDrag()
        {
            return CurrentSkill != null;
        }
    }
}