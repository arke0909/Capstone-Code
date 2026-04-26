using System;
using System.Collections.Generic;
using System.Linq;
using Code.DataSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Work.Code.Craft;

namespace Work.LKW.Code.Items.ItemInfo
{
    public struct ItemCreateData
    {
        public ItemBase Item;
        public int Stack;

        public ItemCreateData(ItemBase item, int stack)
        {
            Item = item;
            Stack = stack;
        }
    }
    
    public enum ItemType
    {
        Material,
        Food,
        Medicine,
        Armor,
        Gun,
        Bullet,
        Helmet,
        MeleeWeapon,
        Throw,
        None
    }

    public enum Rarity
    {
        Common,
        Rare,
        Epic,
    }

    
    [Flags]
    public enum SpawnArea
    {
        Area1 = 1,
        Area2 = 2,
        Area3 = 4,
        Area4 = 8,
        Area5 = 16,
        Area6 = 32,
        None = 64
    }
    
    public abstract class ItemDataSO : ScriptableObject
    {
        [Header("Item Info")]
        
        [ExcelColumn("itemId")]
        public string itemId;
        [ExcelColumn("itemName")]
        public string itemName;
        [ExcelColumn("itemType")]
        public ItemType itemType;
        [ExcelColumn("spawnArea")]
        public SpawnArea spawnArea;
        
        public Sprite itemImage;
        
        [ExcelColumn("description")]
        public string description;
        
        [Header("Properties")]
        [ExcelColumn("rarity")]
        public Rarity rarity;
        [ExcelColumn("rarityWeight")]
        public int rarityWeight;
        [ExcelColumn("value")]
        public int value;
        [ExcelColumn("maxStack")]
         public int maxStack;
        [ExcelColumn("maxSpawnCount")] 
        public int maxSpawnCount;

        public abstract ItemCreateData CreateItem();
    }
}