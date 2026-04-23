using System;
using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.GameEvents;
using InGame.InventorySystem;
using Scripts.Combat.ItemObjects;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Work.LKW.Code.Items;
using Work.SHS.Items.Events;

namespace SHS.Scripts.Entities.Players
{
    public class ItemGrabBehavior : MonoBehaviour, IContainerComponent, ILocalEventSubscriber<ItemEquippedEvent>,
        ILocalEventSubscriber<ItemUnEquippedEvent>
    {
        [SerializeField] private TwoBoneIKConstraint leftHandIK, rightHandIK;
        
        // 이거 나중에 쓸거면 구조 좀 바꿔야함
        [SerializeField] private bool useItemParentConstraint = false;

        [ShowIf("useItemParentConstraint")] [SerializeField]
        private MultiParentConstraint itemParentConstraint;

        [ShowIf("useItemParentConstraint")] [SerializeField]
        private Transform itemParent;


        [SerializeField] private Transform leftHandTarget, rightHandTarget;
        private GrabableObjectBehavior currentGrabableObject;

        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            SetWeight(0);
        }

        public void OnLocalEvent(ItemEquippedEvent eventData)
        {
            if (eventData.EquipableItem is EquipableItem equipableItem &&
                equipableItem.ItemObject.GrabableObjectBehavior != null)
            {
                GrabableObjectBehavior grabableObjectBehavior = equipableItem.ItemObject.GrabableObjectBehavior;
                currentGrabableObject = grabableObjectBehavior;

                if (useItemParentConstraint)
                {
                    equipableItem.ItemObject.transform.SetParent(itemParent, false);
                    equipableItem.ItemObject.transform.localPosition = Vector3.zero;
                }

                SetWeight(1);
            }
        }

        private void Update()
        {
            if (currentGrabableObject != null)
            {
                if (currentGrabableObject.LeftGrabPoint != null)
                {
                    leftHandTarget.position = currentGrabableObject.LeftGrabPoint.position;
                    leftHandTarget.rotation = currentGrabableObject.LeftGrabPoint.rotation;
                }

                if (currentGrabableObject.RightGrabPoint != null)
                {
                    rightHandTarget.position = currentGrabableObject.RightGrabPoint.position;
                    rightHandTarget.rotation = currentGrabableObject.RightGrabPoint.rotation;
                }
            }
        }

        public void OnLocalEvent(ItemUnEquippedEvent eventData)
        {
            SetWeight(0);
        }

        public void SetWeight(int i)
        {
            leftHandIK.weight = currentGrabableObject?.LeftGrabPoint == null ? 0 : i;
            if (useItemParentConstraint)
                rightHandIK.weight = currentGrabableObject?.RightGrabPoint == null ? 0 : i;
        }
    }
}