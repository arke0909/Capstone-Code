using System;
using Chipmunk.ComponentContainers;
using Code.Players;
using Scripts.Players;
using Scripts.Players.States;
using UnityEngine;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Craft
{
    public class CraftContext
    {
        public PlayerInventory TargetInventory { get; set; }
        public CraftTreeSO TargetTreeSO { get; set; }
        public CraftContext(PlayerInventory targetInventory, CraftTreeSO targetTreeSO)
        {
            TargetInventory = targetInventory;
            TargetTreeSO = targetTreeSO;
        }
        public void Deconstruct(out PlayerInventory inventory,out CraftTreeSO targetTree)
        {
            inventory = TargetInventory;
            targetTree = TargetTreeSO;
        }
    }
    
    public class CraftModel
    {
        private Player _player;
        public PlayerInventory Inventory { get; private set; }
        
        public CraftModel(Player player)
        {
            Inventory = player.Get<PlayerInventory>();
            _player = player;
        }
        
        public void TryCraft(CraftTreeSO tree)
        {
            _player.Blackboard.Set("SelectedCraftSO", new CraftContext(Inventory,tree));
            _player.ChangeState(PlayerStateEnum.CraftItem);
        }
    }
}