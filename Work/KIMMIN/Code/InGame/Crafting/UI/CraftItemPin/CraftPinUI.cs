using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chipmunk.ComponentContainers;
using Code.Players;
using Code.UI.Core;
using DewmoLib.Dependencies;
using Scripts.Players;
using UnityEngine;
using UnityEngine.UI;

namespace Work.Code.Craft
{
    public class CraftPinUI : UIBase
    {
        [SerializeField] private RectTransform root;
        
        private Dictionary<CraftTreeSO, CraftPinItemUI> _pinItems = new();
        private List<CraftTreeSO> _orderList = new();

        private CraftPinItemUI[] _pinCraftItems;

        [Inject] private Player _player;
        private PlayerInventory _inventory;
        private Coroutine _rebuildCoroutine;

        private void Start()
        {
            _inventory = _player.Get<PlayerInventory>();
            _pinCraftItems = GetComponentsInChildren<CraftPinItemUI>(true);

            foreach (CraftPinItemUI pinUI in _pinCraftItems)
            {
                pinUI.Init(_inventory);
            }
        }

        public void AddPinUI(CraftTreeSO craftTree)
        {
            if (_pinItems.ContainsKey(craftTree))
                return;

            CraftPinItemUI craftPinItemUI = _pinCraftItems.First(x => !x.IsActive);
            craftPinItemUI.EnablePin(craftTree);
            _pinItems.Add(craftTree, craftPinItemUI);
            _orderList.Add(craftTree);

            RefreshUI();
        }

        public void RemovePinUI(CraftTreeSO targetTree)
        {
            if (_pinItems.TryGetValue(targetTree, out CraftPinItemUI pinCraftItem))
            {
                pinCraftItem.ClearUI();
                _pinItems.Remove(targetTree);
                _orderList.Remove(targetTree);
                
                RefreshUI();
            }
        }

        private void RefreshUI()
        {
            int i = 0;
            foreach (CraftTreeSO craftTree in _orderList)
            {
                if (_pinItems.TryGetValue(craftTree, out CraftPinItemUI ui))
                {
                    ui.transform.SetSiblingIndex(i++);
                }
            }

            RequestRebuild();
        }
        
        private void RequestRebuild()
        {
            if (_rebuildCoroutine != null)
            {
                StopCoroutine(_rebuildCoroutine);
            }
                
            _rebuildCoroutine = StartCoroutine(RebuildLayout());
        }
        
        private IEnumerator RebuildLayout()
        {
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(root);
        }
    }
}