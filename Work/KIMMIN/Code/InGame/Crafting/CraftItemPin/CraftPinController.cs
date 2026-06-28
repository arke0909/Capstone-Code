using System.Collections.Generic;
using Chipmunk.GameEvents;
using DewmoLib.Dependencies;
using UnityEngine;
using UnityEngine.Serialization;
using Work.Code.GameEvents;

namespace Work.Code.Craft
{
    public class CraftPinController : MonoBehaviour
    { 
        [SerializeField] private CraftPinUI craftPinUI; 
        [Inject] private CraftPinItemContainer _pinItemContainer;

        private const int MaxPinCount = 3;
        private readonly List<CraftItemUI> _pinList = new();

        public void ModifyPin(CraftItemUI item, bool isPinned)
        {
            if (isPinned)
            {
                AddPin(item);
            }
            else
            {
                RemovePin(item);
            }
            
            EventBus.Raise(new ChangePinCountEvent(_pinList.Count));
        }

        private void AddPin(CraftItemUI targetItem)
        {
            if (_pinList.Count >= MaxPinCount)
            {
                CraftItemUI oldest = _pinList[0];
                _pinList.RemoveAt(0);
                oldest.SetPin(false);
                craftPinUI.RemovePinUI(oldest.Tree);
                _pinItemContainer.RemoveTree(oldest.Tree);
            }

            _pinList.Add(targetItem);
            targetItem.SetPin(true);
            craftPinUI.AddPinUI(targetItem.Tree);
            _pinItemContainer.AddTree(targetItem.Tree);
        }

        private void RemovePin(CraftItemUI targetItem)
        {
            targetItem.SetPin(false);
            _pinList.Remove(targetItem);
            craftPinUI.RemovePinUI(targetItem.Tree);
            _pinItemContainer.RemoveTree(targetItem.Tree);
        }
    }
}
