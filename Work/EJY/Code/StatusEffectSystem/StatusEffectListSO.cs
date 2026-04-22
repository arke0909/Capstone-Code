using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.StatusEffectSystem
{
    public static class CodeFormat
    {
        public static readonly string EnumFormat = 
            @"using System;

namespace Code.StatusEffectSystem
{{
    public enum {0}
    {{
        NONE = 0, {1}
    }}
}}
";

        public static readonly string StartMark = "START";
        public static readonly string EndMark = "END";
        public static readonly string MethodFormat = 
            @"//" + StartMark + @"
        public AbstractStatusEffectDataSO GetStatusEffect({0} statusEffect)
        {{
            switch (statusEffect)
            {{
{1}
                default:
                    return null;
            }}
        }}
        //" + EndMark;
        
    }
    
    [CreateAssetMenu(fileName = "StatusEffectList", menuName = "SO/StatusEffect/List", order = 0)]
    public class StatusEffectListSO : ScriptableObject
    {
        public string enumName = "StatusEffectName";
        public List<AbstractStatusEffectDataSO> statusEffectData;
        
        //START
        public AbstractStatusEffectDataSO GetStatusEffect(StatusEffectEnum statusEffect)
        {
            switch (statusEffect)
            {
				case StatusEffectEnum.DEF_STATUS:
					return statusEffectData.FirstOrDefault(status => status.StatusEffectName == "Def Status");
 				case StatusEffectEnum.HP_STATUS:
					return statusEffectData.FirstOrDefault(status => status.StatusEffectName == "HP Status");
 				case StatusEffectEnum.SPEED_STATUS:
					return statusEffectData.FirstOrDefault(status => status.StatusEffectName == "Speed Status");
 				case StatusEffectEnum.HEALTH_REGEN:
					return statusEffectData.FirstOrDefault(status => status.StatusEffectName == "Health Regen");
 				case StatusEffectEnum.DAMAGE_MODIFY_STATUS:
					return statusEffectData.FirstOrDefault(status => status.StatusEffectName == "Damage Modify Status");
 				case StatusEffectEnum.DAMAGE_DEMODIFY_STATUS:
					return statusEffectData.FirstOrDefault(status => status.StatusEffectName == "Damage Demodify Status");
 				case StatusEffectEnum.RELOAD_SPEED_STATUS:
					return statusEffectData.FirstOrDefault(status => status.StatusEffectName == "Reload Speed Status");
 				case StatusEffectEnum.ADDITIONAL_DAMAGE:
					return statusEffectData.FirstOrDefault(status => status.StatusEffectName == "Additional Damage");
 				case StatusEffectEnum.DAMAGE_STORING:
					return statusEffectData.FirstOrDefault(status => status.StatusEffectName == "Damage Storing");
 				case StatusEffectEnum.DOT:
					return statusEffectData.FirstOrDefault(status => status.StatusEffectName == "Dot");
 				case StatusEffectEnum.SHIELD:
					return statusEffectData.FirstOrDefault(status => status.StatusEffectName == "Shield");
 				case StatusEffectEnum.DMGINCREASE:
					return statusEffectData.FirstOrDefault(status => status.StatusEffectName == "DmgIncrease");
 				case StatusEffectEnum.FIRERATE_STATUS:
					return statusEffectData.FirstOrDefault(status => status.StatusEffectName == "FireRate Status");
 				case StatusEffectEnum.BULLET_REDUCE_STATUS:
					return statusEffectData.FirstOrDefault(status => status.StatusEffectName == "Bullet Reduce Status");
                default:
                    return null;
            }
        }
        //END
    }
}