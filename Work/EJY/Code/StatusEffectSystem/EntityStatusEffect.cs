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
        public BuffSO KeySO;
        public StatusEffectEnum StatusEffect;
        public int Level;
        public float ApplyTime;
        public float Value;
        public bool IsPercent;
        public bool CanOverlap;
        public bool IsOverWrite;

        public StatusEffectInfo(BuffSO keySO, StatusEffectCreateData data, int valueLevel = 0)
        {
            KeySO = keySO;
            StatusEffect = data.statusEffect;
            Level = data.level;
            ApplyTime = keySO.applyTime;
            if (valueLevel >= data.effectValue.Length) valueLevel = data.effectValue.Length - 1;
            Value = data.effectValue[valueLevel];
            IsPercent = data.isPercent;
            CanOverlap = false;
            IsOverWrite = false;
        }
    }

    public class EntityStatusEffect : MonoBehaviour, IContainerComponent
    {
        [SerializeField] private StatusEffectListSO statusEffectList;
        public event Action<AbstractStatusEffect> OnStatusEffectApplied;
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

        private bool ResetIfAlreadyApplied(IEnumerable<AbstractStatusEffect> list, StatusEffectInfo info, out AbstractStatusEffect newStatusEffect)
        {
            newStatusEffect = list.FirstOrDefault(statusEffect =>
                info.StatusEffect == statusEffect.StatusEffectEnum);
            if (newStatusEffect != null)
            {
                newStatusEffect.SetRemainingTime(Mathf.Max(info.ApplyTime, newStatusEffect.CurrentTime));
                return true;
            }

            return false;
        }


        private void ApplyNoneOverlapStatusEffect(StatusEffectInfo info, AbstractStatusEffect newStatusEffect)
        {
            if (info.CanOverlap) return;

            if (_noneOverlapStatusEffects.TryGetValue(info.StatusEffect, out var oldEffect))
            {
                // 레벨에 상관없이 덮어써야함 or 레벨이 더 높음
                if (info.IsOverWrite || oldEffect.Level <= newStatusEffect.Level)
                {
                    RemoveFromDictionaryAndFlag(oldEffect);
                }
            }

            _noneOverlapStatusEffects[info.StatusEffect] = newStatusEffect;
        }

        private void ApplyStatusEffect(AbstractStatusEffect newStatusEffect)
        {
            newStatusEffect.ApplyStatusEffect(_target);
            _appliedStatusEffects.Add(newStatusEffect);
            OnStatusEffectApplied?.Invoke(newStatusEffect);
        }

        public AbstractStatusEffect AddStatusEffect(StatusEffectInfo info)
        {
            var list = GetOrCreateStatusEffectsList(info);

            // 이미 걸린 상태이상이라면 리셋, 리셋 성공하면 다음 버프
            if (ResetIfAlreadyApplied(list, info, out AbstractStatusEffect appliedStatusEffect))
                return appliedStatusEffect;

            // 새로 상태이상 객체 생성
            var newStatusEffect = CreateStatusEffect(info);

            // 기절같이 중첩이 안된다면 덮어쓰기
            ApplyNoneOverlapStatusEffect(info, newStatusEffect);

            list.Add(newStatusEffect);
            ApplyStatusEffect(newStatusEffect);
            return newStatusEffect;
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

        public bool IsStatusEffectExist(StatusEffectEnum statusEffect)
            => _noneOverlapStatusEffects.ContainsKey(statusEffect);
    }
}