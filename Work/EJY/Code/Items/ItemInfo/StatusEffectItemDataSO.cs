using Code.StatusEffectSystem;
using UnityEngine;
using UnityEngine.Serialization;
using Code.Items;
using Code.Items.ItemInfo;

namespace Code.Items.ItemInfo
{
    [CreateAssetMenu(fileName = "StatusEffectItem", menuName = "SO/Item/StatusEffectItem", order = 0)]
    public class StatusEffectItemDataSO : UseItemDataSO
    {
        [FormerlySerializedAs("statusEffects")] public BuffSO buffs;
        
        public override ItemCreateData CreateItem()
        {
            return new ItemCreateData(new StatusEffectItem(this), Random.Range(1, maxStack));
        }
    }
}