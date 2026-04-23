using System;
using System.Collections.Generic;
using Code.StatusEffectSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.StatusEffectSystem
{
    [Serializable]
    public struct StatusEffectCreateData
    {
        public StatusEffectEnum statusEffect;
        public bool isPercent;
        [FormerlySerializedAs("level")] public int priority;
        public float[] effectValue;
        public bool isOverrideApplyTime;
        public float overrideTime;
    }
    
    [CreateAssetMenu(fileName = "BuffData", menuName = "SO/StatusEffect/BuffSO", order = 0)]
    public class BuffSO : ScriptableObject
    {
        public string buffName;
        public Sprite buffIcon;
        public List<StatusEffectCreateData> statusEffectCreateData;
        public float applyTime;
        
        public List<StatusEffectInfo> GetStatusEffectInfo(int level = 0, float additionalTime = 0) 
        {
            List<StatusEffectInfo> list = new List<StatusEffectInfo>();

            foreach (var createData in statusEffectCreateData)
            {
                float finalApplyTime = createData.isOverrideApplyTime ? createData.overrideTime : applyTime ;
                
                list.Add(new StatusEffectInfo {KeySO = this, StatusEffect = createData.statusEffect
                    ,Priority = createData.priority, ApplyTime = finalApplyTime + additionalTime,
                    Value = createData.effectValue[Mathf.Min(level, createData.effectValue.Length - 1)],IsPercent = createData.isPercent });
            }

            return list;
        }

        private void OnValidate()
        {
            buffName = name;
        }
    }
}