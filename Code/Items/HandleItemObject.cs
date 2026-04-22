using Chipmunk.ComponentContainers;
using Scripts.Combat.ItemObjects;
using Scripts.Entities;
using UnityEngine;
using Work.LKW.Code.Items;

namespace Work.LKW.Code.Items
{
    public class HandleItemObject : ItemObject, IContainerComponent
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        private void SetSprite(Sprite sprite) => spriteRenderer.sprite = sprite;
        public ComponentContainer ComponentContainer { get; set; }
        public void OnInitialize(ComponentContainer componentContainer)
        {
            ComponentContainer = componentContainer;
        }

        public override void InitObject(Entity owner, EquipableItem item)
        {
            base.InitObject(owner, item);
            SetSprite(item.ItemData.itemImage);
        }
    }
}