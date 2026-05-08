using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.StatusEffectSystem
{
    [Serializable]
    [InlineProperty]
    [HideLabel]
    public struct StatusEffectCreateData
    {
        [LabelText("Type")]
        public StatusEffectEnum statusEffect;

        [LabelText("Percent")]
        public bool isPercent;

        [LabelText("Priority")]
        public int priority;

        [LabelText("Values")]
        public float[] effectValue;

        [LabelText("Override Apply Time")]
        public bool isOverrideApplyTime;

        [ShowIf(nameof(ShowOverrideTime))]
        [LabelText("Override Time")]
        [MinValue(0.01f)]
        public float overrideTime;

        [LabelText("Custom Behavior")]
        public bool useCustomBehaviorSettings;

        [ShowIf(nameof(ShowCustomBehavior))]
        [LabelText("Can Overlap")]
        public bool canOverlap;

        [ShowIf(nameof(ShowCustomBehavior))]
        [LabelText("Overwrite")]
        public bool isOverWrite;

        [ShowIf(nameof(ShowStackToggle))]
        [LabelText("Use Shared Stack")]
        public bool useSharedStack;

        [ShowIf(nameof(ShowSharedStackSettings))]
        [LabelText("Max Stack")]
        [MinValue(1)]
        public int maxStack;

        [ShowIf(nameof(ShowSharedStackSettings))]
        [LabelText("Stack Value Mode")]
        public StatusEffectStackValueMode stackValueMode;

        [ShowIf(nameof(ShowSharedStackSettings))]
        [LabelText("Stack Decay Mode")]
        public StatusEffectStackDecayMode stackDecayMode;

        [ShowIf(nameof(ShowDecayInterval))]
        [LabelText("Decay Interval")]
        [MinValue(0.01f)]
        public float stackDecayInterval;

        [ShowIf(nameof(ShowSharedStackSettings))]
        [LabelText("Refresh Timer On Reapply")]
        public bool refreshTimerOnReapply;

        private bool ShowOverrideTime => isOverrideApplyTime;
        private bool ShowCustomBehavior => useCustomBehaviorSettings;
        private bool ShowStackToggle => ShowCustomBehavior && canOverlap;
        private bool ShowSharedStackSettings => ShowStackToggle && useSharedStack;
        private bool ShowDecayInterval => ShowSharedStackSettings && stackDecayMode == StatusEffectStackDecayMode.DecreaseOneByOne;
    }
    
    [CreateAssetMenu(fileName = "BuffData", menuName = "SO/StatusEffect/BuffSO", order = 0)]
    public class BuffSO : ScriptableObject
    {
        public string buffName;
        public Sprite buffIcon;
        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
        public List<StatusEffectCreateData> statusEffectCreateData;
        public float applyTime;
        
        public List<StatusEffectInfo> GetStatusEffectInfo(int level = 0, float additionalTime = 0) 
        {
            List<StatusEffectInfo> list = new List<StatusEffectInfo>();

            for (int i = 0; i < statusEffectCreateData.Count; i++)
            {
                var createData = statusEffectCreateData[i];
                float finalApplyTime = createData.isOverrideApplyTime ? createData.overrideTime : applyTime ;
                int maxLv = Mathf.Min(level, createData.effectValue.Length - 1);
                
                list.Add(new StatusEffectInfo 
                {
                    CreateDataIndex = i,
                    KeySO = this,
                    StatusEffect = createData.statusEffect,
                    Priority = createData.priority,
                    ApplyTime = finalApplyTime + additionalTime,
                    Value = createData.effectValue[maxLv],
                    IsPercent = createData.isPercent,
                    UseCustomBehaviorSettings = createData.useCustomBehaviorSettings,
                    CanOverlap = createData.canOverlap,
                    IsOverWrite = createData.isOverWrite,
                    UseSharedStack = createData.useSharedStack,
                    MaxStack = Mathf.Max(1, createData.maxStack),
                    StackValueMode = createData.stackValueMode,
                    StackDecayMode = createData.stackDecayMode,
                    StackDecayInterval = Mathf.Max(0.01f, createData.stackDecayInterval),
                    RefreshTimerOnReapply = createData.refreshTimerOnReapply
                });
            }

            return list;
        }

        private void OnValidate()
        {
            buffName = name;
        }
    }
}
