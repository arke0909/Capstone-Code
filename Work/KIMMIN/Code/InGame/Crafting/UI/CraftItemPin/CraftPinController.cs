using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Work.Code.Craft
{
    public class CraftPinController : MonoBehaviour
    { 
        [SerializeField] private CraftPinUI craftPinUI; 

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
        }

        private void AddPin(CraftItemUI targetItem)
        {
            if (_pinList.Count >= MaxPinCount)
            {
                CraftItemUI oldest = _pinList[0];
                _pinList.RemoveAt(0);
                oldest.SetPin(false);
                craftPinUI.RemovePinUI(oldest.Tree);
            }

            _pinList.Add(targetItem);
            targetItem.SetPin(true);
            craftPinUI.AddPinUI(targetItem.Tree);
        }

        private void RemovePin(CraftItemUI targetItem)
        {
            targetItem.SetPin(false);
            _pinList.Remove(targetItem);
            craftPinUI.RemovePinUI(targetItem.Tree);
        }
    }
}