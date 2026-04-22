using System;
using Chipmunk.ComponentContainers;
using Code.Players;
using Code.UI.Core;
using DewmoLib.Dependencies;
using Scripts.Players;
using UnityEngine;
using UnityEngine.InputSystem;
using Work.LKW.Code.Items;

namespace Work.Code.Admin
{
    public class AdminPanel : UIPanel
    {
        [SerializeField] private Transform itemRoot;
        [SerializeField] private AdminItem itemUI;
        [SerializeField] private ItemDataBaseSO itemDB;
        
        [Inject] private Player _player;
        private PlayerInventory _inventory;

        private void Start()
        {
            base.Awake();

            _inventory = _player.Get<PlayerInventory>();
            foreach (var item in itemDB.allItems)
            {
                var ui = Instantiate(itemUI, itemRoot);
                ui.EnableFor(item);
                ui.SetInventory(_inventory);
            }
        }

        private void Update()
        {
            if (Keyboard.current.f5Key.wasPressedThisFrame)
            {
                ToggleUI(true);
            }
        }
    }
}