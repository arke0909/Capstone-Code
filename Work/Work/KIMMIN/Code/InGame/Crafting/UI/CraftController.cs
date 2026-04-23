using System;
using System.Collections.Generic;
using Code.Players;
using DewmoLib.Dependencies;
using Scripts.Players;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Crafting
{
    public class CraftController : MonoBehaviour
    {
        [Inject] private Player _player;
        public PlayerInventory Inventory { get; private set; }

        private void Awake()
        {
            Inventory = _player.GetComponentInChildren<PlayerInventory>();
        }
        
        public bool Craft(CraftTreeSO tree)
        {
            if (!Inventory.TryConsume(tree.CosumeItems)) return false;

            var result = tree.Item.CreateItem();
            Inventory.TryAddItem(result.Item, tree.Count);
            return true;
        }
    }
}