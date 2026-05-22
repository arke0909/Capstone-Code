using System;
using System.Collections.Generic;
using System.Linq;
using Code.DataSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Work.Code.Craft;

namespace Code.Items.ItemInfo
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
        None   = 0,           // 0
        Area1  = 1 << 0,      // 1
        Area2  = 1 << 1,      // 2
        Area3  = 1 << 2,      // 4
        Area4  = 1 << 3,      // 8
        Area5  = 1 << 4,      // 16
        Area6  = 1 << 5,      // 32
        Area7  = 1 << 6,      // 64
        Area8  = 1 << 7,      // 128
        Area9  = 1 << 8,      // 256
        Area10 = 1 << 9,      // 512
        Area11 = 1 << 10,     // 1024
        Area12 = 1 << 11,     // 2048
        Area13 = 1 << 12,     // 4096
        Area14 = 1 << 13,     // 8192
        Area15 = 1 << 14,     // 16384
        Area16 = 1 << 15,     // 32768
        Area17 = 1 << 16,     // 65536
        Area18 = 1 << 17,     // 131072
        Area19 = 1 << 18,     // 262144
        Area20 = 1 << 19,     // 524288
        Area21 = 1 << 20,     // 1048576
        Area22 = 1 << 21,      // 2097152
        
        All = Area1 | Area2 | Area3 | Area4 | Area5 | Area6 | Area7 | Area8 | Area9 | Area10 |
              Area11 | Area12 | Area13 | Area14 | Area15 | Area16 | Area17 | Area18 |
              Area19 | Area20 | Area21 | Area22
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