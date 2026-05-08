using System;
using Code.StatusEffectSystem.StatusEffects;
using System.Collections.Generic;
using System.Linq;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Scripts.Entities;
using UnityEngine;

namespace Code.StatusEffectSystem
{
    public struct StatusEffectInfo
    {
        public int CreateDataIndex;
        public BuffSO KeySO;
        public StatusEffectEnum StatusEffect;
        public int Priority;
        public float ApplyTime;
        public float Value;
        public bool IsPercent;
        public bool UseCustomBehaviorSettings;
        public bool CanOverlap;
        public bool IsOverWrite;
        public bool UseSharedStack;
        public int MaxStack;
        public StatusEffectStackValueMode StackValueMode;
        public StatusEffectStackDecayMode StackDecayMode;
        public float StackDecayInterval;
        public bool RefreshTimerOnReapply;

        public StatusEffectInfo(BuffSO keySO, StatusEffectCreateData data, int valueLevel = 0)
        {
            CreateDataIndex = -1;
            KeySO = keySO;
            StatusEffect = data.statusEffect;
            Priority = data.priority;
            ApplyTime = keySO.applyTime;
            if (valueLevel >= data.effectValue.Length) valueLevel = data.effectValue.Length - 1;
            Value = data.effectValue[valueLevel];
            IsPercent = data.isPercent;
            UseCustomBehaviorSettings = data.useCustomBehaviorSettings;
            CanOverlap = false;
            IsOverWrite = false;
            UseSharedStack = data.useSharedStack;
            MaxStack = Mathf.Max(1, data.maxStack);
            StackValueMode = data.stackValueMode;
            StackDecayMode = data.stackDecayMode;
            StackDecayInterval = Mathf.Max(0.01f, data.stackDecayInterval);
            RefreshTimerOnReapply = data.refreshTimerOnReapply;
        }
    }

    public class EntityStatusEffect : MonoBehaviour, IContainerComponent
    {
        [SerializeField] private StatusEffectListSO statusEffectList;
        public event Action<AbstractStatusEffect> OnStatusEffectReleased;
        public ComponentContainer ComponentContainer { get; set; }

        private Dictionary<StatusEffectEnum, AbstractStatusEffect> _noneOverlapStatusEffects =
            new Dictionary<StatusEffectEnum, AbstractStatusEffect>();

        private Dictionary<BuffSO, List<AbstractStatusEffect>> _statusEffects = new();
        private Entity _target;
        private List<AbstractStatusEffect> _appliedStatusEffects = new List<AbstractStatusEffect>();

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _target = componentContainer.Get<Entity>(true);
        }

        private void OnDestroy()
        {
            ClearStatusEffect();
        }

        private void Update()
        {
            for (int i = _appliedStatusEffects.Count - 1; i >= 0; i--)
            {
                var effect = _appliedStatusEffects[i];
                if (!effect.UpdateStatusEffect(_target))
                {
                    RemoveFromDictionaryAndFlag(effect);
                }
            }
        }

        public AbstractStatusEffectDataSO GetStatusEffect(StatusEffectEnum statusEffect)
            => statusEffectList.GetStatusEffect(statusEffect);

        private AbstractStatusEffect CreateStatusEffect(StatusEffectInfo info)
        {
            var data = GetStatusEffect(info.StatusEffect);
            if (data == null)
            {
                Debug.Log($"Find data is null, StatusEffect Type is {info.StatusEffect}");
                return null;
            }
            AbstractStatusEffect newStatusEffect = data.CreateStatusEffect(_target, info);
            return newStatusEffect;
        }

        #region About StatusEffect Apply and Release

        private List<AbstractStatusEffect> GetOrCreateStatusEffectsList(StatusEffectInfo info)
        {
            var list = _statusEffects.GetValueOrDefault(info.KeySO);
            if (list == null)
            {
                list = new List<AbstractStatusEffect>();
                _statusEffects.Add(info.KeySO, list);
            }

            return list;
        }
        
        private StatusEffectInfo ApplyStatusEffectFlags(StatusEffectInfo info)
        {
            var data = GetStatusEffect(info.StatusEffect);
            if (data == null)
                return info;
            
            return data.ApplyFlag(info);
        }

        private bool TryAddSharedStack(List<AbstractStatusEffect> list, StatusEffectInfo info, out AbstractStatusEffect stackedEffect)
        {
            stackedEffect = null;

            if (!info.CanOverlap || !info.UseSharedStack)
                return false;

            stackedEffect = list.FirstOrDefault(statusEffect =>
                info.StatusEffect == statusEffect.StatusEffectEnum &&
                info.CreateDataIndex == statusEffect.CreateDataIndex);
            if (stackedEffect == null)
                return false;

            stackedEffect.AddSharedStack(info);
            return true;
        }

        private bool ResetIfAlreadyApplied(IEnumerable<AbstractStatusEffect> list, StatusEffectInfo info, out AbstractStatusEffect activeStatusEffect)
        {
            activeStatusEffect = list.FirstOrDefault(statusEffect =>
                info.StatusEffect == statusEffect.StatusEffectEnum &&
                info.CreateDataIndex == statusEffect.CreateDataIndex);
            if (activeStatusEffect == null)
                return false;

            if (info.IsOverWrite || info.Priority >= activeStatusEffect.Priority)
            {
                activeStatusEffect.SetStrongerValue(info);
                return true;
            }
            
            float nextDuration = Mathf.Max(info.ApplyTime, activeStatusEffect.RemainingTime);
            activeStatusEffect.SetRemainingTime(nextDuration);
            return true;
        }

        private bool TryRegisterNoneOverlapStatusEffect(StatusEffectInfo info, AbstractStatusEffect newStatusEffect, out AbstractStatusEffect keptEffect)
        {
            keptEffect = null;

            if (info.CanOverlap)
                return true;

            if (_noneOverlapStatusEffects.TryGetValue(info.StatusEffect, out var oldEffect))
            {
                bool shouldReplace = info.IsOverWrite || oldEffect.Priority <= newStatusEffect.Priority;
                if (!shouldReplace)
                {
                    keptEffect = oldEffect;
                    return false;
                }

                RemoveFromDictionaryAndFlag(oldEffect);
            }

            _noneOverlapStatusEffects[info.StatusEffect] = newStatusEffect;
            return true;
        }

        private void ApplyStatusEffect(AbstractStatusEffect newStatusEffect)
        {
            newStatusEffect.ApplyStatusEffect(_target);
            _appliedStatusEffects.Add(newStatusEffect);
        }

        public IEnumerable<AbstractStatusEffect> AddStatusEffect(IEnumerable<StatusEffectInfo> infos)
        {
            List<AbstractStatusEffect> statusEffects = new List<AbstractStatusEffect>();
            
            foreach (var info in infos)
            {
                var applyflagInfo = ApplyStatusEffectFlags(info);
                var list = GetOrCreateStatusEffectsList(applyflagInfo);

                if (TryAddSharedStack(list, applyflagInfo, out AbstractStatusEffect stackedEffect))
                {
                    statusEffects.Add(stackedEffect);
                    continue;
                }

                if (!applyflagInfo.CanOverlap && ResetIfAlreadyApplied(list, applyflagInfo, out AbstractStatusEffect appliedStatusEffect))
                {
                    statusEffects.Add(appliedStatusEffect);
                    continue;
                }

                var newStatusEffect = CreateStatusEffect(applyflagInfo);

                if (!TryRegisterNoneOverlapStatusEffect(applyflagInfo, newStatusEffect, out AbstractStatusEffect keptEffect))
                {
                    if (list.Count == 0)
                        _statusEffects.Remove(applyflagInfo.KeySO);

                    statusEffects.Add(keptEffect);
                    continue;
                }

                statusEffects.Add(newStatusEffect);
                list.Add(newStatusEffect);
                ApplyStatusEffect(newStatusEffect);
            }
            
            return statusEffects;
        }

        private void RemoveFromDictionaryAndFlag(AbstractStatusEffect effect)
        {
            if (_noneOverlapStatusEffects.TryGetValue(effect.StatusEffectEnum, out var noneOverlapEffect))
            {
                if (noneOverlapEffect == effect)
                {
                    _noneOverlapStatusEffects.Remove(effect.StatusEffectEnum);
                }
            }

            if (effect.KeySO != null && _statusEffects.TryGetValue(effect.KeySO, out var list))
            {
                list.Remove(effect);
                if (list.Count == 0)
                    _statusEffects.Remove(effect.KeySO);
            }

            effect.ReleaseStatusEffect(_target);
            _appliedStatusEffects.Remove(effect);
            OnStatusEffectReleased?.Invoke(effect);
        }

        public void RemoveStatusEffect(BuffSO buff)
        {
            if (_statusEffects.TryGetValue(buff, out List<AbstractStatusEffect> effectList))
            {
                for (int i = effectList.Count - 1; i >= 0; i--)
                {
                    var effect = effectList[i];
                    RemoveFromDictionaryAndFlag(effect);
                }
            }
        }

        public void ClearStatusEffect()
        {
            for (int i = _appliedStatusEffects.Count - 1; i >= 0; i--)
            {
                var effect = _appliedStatusEffects[i];
                effect.ReleaseStatusEffect(_target);
                OnStatusEffectReleased?.Invoke(effect);
            }

            _appliedStatusEffects.Clear();
            _noneOverlapStatusEffects.Clear();
            _statusEffects.Clear();
        }

        #endregion
    }
}
