using System;
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

namespace Work.Code.Crafting
{
    public class PinCraftItem : UIBase
    {
        public const int PIN_CAPACITY = 3;

        [SerializeField] private RectTransform root;
        private Dictionary<CraftTreeSO, PinCraftItemUI> _pinItems = new();
        private List<PinCraftItemUI> _pinCraftItems = new();
        private Queue<CraftTreeSO> _orderQueue = new();

        [Inject] private Player _player;
        private PlayerInventory _inventory;
        private Coroutine _rebuildCoroutine;

        private void Start()
        {
            _pinCraftItems = GetComponentsInChildren<PinCraftItemUI>(true).ToList();
            _inventory = _player.Get<PlayerInventory>();

            foreach (var pinUI in _pinCraftItems)
            {
                pinUI.Init(_inventory);
            }
        }

        private void LateUpdate()
        {
            if (_rebuildCoroutine != null)
            {
                StopCoroutine(_rebuildCoroutine);
                _rebuildCoroutine = null;
            }
                
            _rebuildCoroutine = StartCoroutine(RebuildLayout());
        }
        
        private IEnumerator RebuildLayout()
        {
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(root);
        }

        public void PushItem(CraftTreeSO craftTree)
        {
            if (_pinItems.ContainsKey(craftTree)) return;

            var item = _pinCraftItems.First(x => !x.IsActive);
            item.EnableFor(craftTree);
            _pinItems.Add(craftTree, item);
            _orderQueue.Enqueue(craftTree);

            RefreshUI();
        }

        public void PopItem(CraftTreeSO craftTree)
        {
            if (_pinItems.TryGetValue(craftTree, out PinCraftItemUI pinCraftItem))
            {
                pinCraftItem.Clear();
                _pinItems.Remove(craftTree);

                var newQueue = new Queue<CraftTreeSO>();
                while (_orderQueue.Count > 0)
                {
                    var item = _orderQueue.Dequeue();
                    if (item != craftTree)
                        newQueue.Enqueue(item);
                }

                _orderQueue = newQueue;

                RefreshUI();
            }
        }

        private void RefreshUI()
        {
            int i = 0;
            foreach (var craft in _orderQueue)
            {
                if (_pinItems.TryGetValue(craft, out var ui))
                {
                    ui.transform.SetSiblingIndex(i++);
                }
            }
        }
    }
}