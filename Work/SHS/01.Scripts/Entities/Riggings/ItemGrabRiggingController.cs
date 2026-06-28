using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.GameEvents;
using Code.Items;
using SHS.Scripts.Entities.Players;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Work.SHS.Items.Events;

namespace SHS.Scripts.Entities.Rigings
{
    public class ItemGrabRiggingController : MonoBehaviour, IContainerComponent,
        ILocalEventSubscriber<ChangeHandlingEvent>
    {
        protected enum GrabRigMode
        {
            AnimationHolder,
            IkGrab
        }

        [SerializeField] private TwoBoneIKConstraint leftHandIK, rightHandIK;
        [SerializeField] private Transform animationItemParent, rigItemParent;
        [SerializeField] private Transform leftHandTarget, rightHandTarget;

        private GrabableObjectBehavior currentGrabableObject;
        private Transform _currentItemTransform;
        private Vector3 _itemLocalPosition;
        private Quaternion _itemLocalRotation;
        private GrabRigMode _currentMode = GrabRigMode.AnimationHolder;

        public ComponentContainer ComponentContainer { get; set; }

        public virtual void OnInitialize(ComponentContainer componentContainer)
        {
            ClearWeight();
        }

        public void OnLocalEvent(ChangeHandlingEvent eventData)
        {
            if (eventData.EquipableItem == null)
            {
                ClearWeight();
                currentGrabableObject = null;
                _currentItemTransform = null;
                return;
            }
            
            EquipableItem equipableItem = eventData.EquipableItem;
            currentGrabableObject = equipableItem.ItemObject.GrabableObjectBehavior;
            _currentItemTransform = equipableItem.ItemObject.transform;
            _itemLocalPosition = _currentItemTransform.localPosition;
            _itemLocalRotation = _currentItemTransform.localRotation;

            SetMode(_currentMode);
        }

        private void Update()
        {
            if (currentGrabableObject == null)
                return;

            leftHandTarget.position = currentGrabableObject.LeftGrabPoint.position;
            leftHandTarget.rotation = currentGrabableObject.LeftGrabPoint.rotation;

            rightHandTarget.position = currentGrabableObject.RightGrabPoint.position;
            rightHandTarget.rotation = currentGrabableObject.RightGrabPoint.rotation;
        }

        public void SetWeight(float weight)
        {
            leftHandIK.weight = weight;
            rightHandIK.weight = weight;
        }

        protected void SetMode(GrabRigMode mode)
        {
            _currentMode = mode;

            if (_currentItemTransform == null)
            {
                ClearWeight();
                return;
            }

            SetItemParent(mode);
            SetWeight(mode == GrabRigMode.IkGrab ? 1f : 0f);
        }

        private void SetItemParent(GrabRigMode mode)
        {
            Transform targetParent = mode == GrabRigMode.IkGrab
                ? rigItemParent
                : animationItemParent;

            if (targetParent == null)
                throw new MissingReferenceException($"{(mode == GrabRigMode.IkGrab ? nameof(rigItemParent) : nameof(animationItemParent))} is not assigned.");

            _currentItemTransform.SetParent(targetParent, false);
            _currentItemTransform.localPosition = _itemLocalPosition;
            _currentItemTransform.localRotation = _itemLocalRotation;
        }

        private void ClearWeight()
        {
            leftHandIK.weight = 0f;
            rightHandIK.weight = 0f;
        }
    }
}
