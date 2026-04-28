using Ami.BroAudio;
using Code.DataSystem;
using Scripts.Entities;
using UnityEngine;

namespace Work.LKW.Code.Items.ItemInfo
{
    public abstract class WeaponDataSO : HandItemDataSO
    {
        [ExcelColumn("maxDurability")] 
        public int maxDurability;
        [ExcelColumn("attackRange")] 
        public float attackRange;
        [ExcelColumn("defaultDamage")] 
        public float defaultDamage;
        [ExcelColumn("attackSpeed")] 
        public float attackSpeed;
        [ExcelColumn("fireRate")] // 연사 속도
        public float fireRate;
        
        public SoundID attackSoundID;
    }
}