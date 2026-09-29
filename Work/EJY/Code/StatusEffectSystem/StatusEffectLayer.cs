using Code.StatusEffectSystem.StatusEffects;
using Entities;
using Scripts.Entities;
using System.Collections.Generic;
using UnityEngine;

namespace Code.StatusEffectSystem
{
    public class StatusEffectLayer
    {
        public BuffSO Buff { get; private set; }
        public object Source { get; private set; }
        public float AppliedTime { get; private set; }

        internal int StatusEffectCount => _statusEffects.Count;
        internal bool IsVfxPlaying { get; private set; }

        private Entity _target;
        private VFXComponent _vfxComponent;
        private List<AbstractStatusEffect> _statusEffects = new List<AbstractStatusEffect>();
        private List<AbstractStatusEffect> _statusEffectUpdateBuffer =
            new List<AbstractStatusEffect>();

        internal StatusEffectLayer(BuffSO buff, Entity target, VFXComponent vfxComponent)
        {
            Buff = buff;
            _target = target;
            _vfxComponent = vfxComponent;
        }

        public bool TryGetStatusEffect(
            AbstractStatusEffectDataSO statusEffectData,
            out AbstractStatusEffect statusEffect)
        {
            for (int i = _statusEffects.Count - 1; i >= 0; i--)
            {
                if (_statusEffects[i].StatusEffectData != statusEffectData)
                    continue;

                statusEffect = _statusEffects[i];
                return true;
            }

            statusEffect = null;
            return false;
        }

        internal bool TryAddSharedStack(StatusEffectInfo info, out AbstractStatusEffect stackedEffect)
        {
            stackedEffect = null;

            if (!info.CanOverlap || !info.UseSharedStack)
                return false;

            if (!TryGetStatusEffect(info.StatusEffectData, info.CreateDataIndex, out stackedEffect))
                return false;

            stackedEffect.AddSharedStack(info);
            return true;
        }

        internal bool ResetIfAlreadyApplied(StatusEffectInfo info, out AbstractStatusEffect activeStatusEffect)
        {
            if (!TryGetStatusEffect(info.StatusEffectData, info.CreateDataIndex, out activeStatusEffect))
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

        internal void AddStatusEffect(AbstractStatusEffect statusEffect)
        {
            statusEffect.SetOwnerLayer(this);
            _statusEffects.Add(statusEffect);
            statusEffect.ApplyStatusEffect(_target);
        }

        public void ExtendDuration(float additionalTime)
        {
            if (additionalTime <= 0f)
                return;

            for (int i = 0; i < _statusEffects.Count; i++)
            {
                AbstractStatusEffect statusEffect = _statusEffects[i];
                statusEffect.ExtendDuration(additionalTime);
            }
        }

        internal void CollectExpiredStatusEffects(List<AbstractStatusEffect> expiredStatusEffects)
        {
            _statusEffectUpdateBuffer.Clear();
            _statusEffectUpdateBuffer.AddRange(_statusEffects);

            for (int i = _statusEffectUpdateBuffer.Count - 1; i >= 0; i--)
            {
                AbstractStatusEffect statusEffect = _statusEffectUpdateBuffer[i];
                if (!_statusEffects.Contains(statusEffect))
                    continue;

                if (!statusEffect.UpdateStatusEffect(_target))
                    expiredStatusEffects.Add(statusEffect);
            }
        }

        internal bool RemoveStatusEffect(AbstractStatusEffect statusEffect)
        {
            if (!_statusEffects.Remove(statusEffect))
                return false;

            statusEffect.ReleaseStatusEffect(_target);
            return true;
        }

        internal bool TryGetLastStatusEffect(out AbstractStatusEffect statusEffect)
        {
            if (_statusEffects.Count == 0)
            {
                statusEffect = null;
                return false;
            }

            statusEffect = _statusEffects[_statusEffects.Count - 1];
            return true;
        }

        internal void UpdateRuntimeInfo(object source)
        {
            Source = source;
            AppliedTime = Time.time;
        }

        internal void PlayVFX()
        {
            if (IsVfxPlaying || string.IsNullOrWhiteSpace(Buff.vfxName))
                return;

            if (_vfxComponent == null)
            {
                Debug.LogWarning($"{_target.name} has no VFXComponent for {Buff.name}.");
                return;
            }

            _vfxComponent.PlayVFX(Buff.vfxName, _target.transform.position, Quaternion.identity);
            IsVfxPlaying = true;
        }

        internal void StopVFX()
        {
            if (!IsVfxPlaying || string.IsNullOrWhiteSpace(Buff.vfxName))
                return;

            if (_vfxComponent != null)
                _vfxComponent.StopVFX(Buff.vfxName);

            IsVfxPlaying = false;
        }

        internal bool TryGetStatusEffect(
            AbstractStatusEffectDataSO statusEffectData,
            int createDataIndex,
            out AbstractStatusEffect statusEffect)
        {
            for (int i = _statusEffects.Count - 1; i >= 0; i--)
            {
                AbstractStatusEffect currentStatusEffect = _statusEffects[i];
                if (currentStatusEffect.StatusEffectData != statusEffectData ||
                    currentStatusEffect.CreateDataIndex != createDataIndex)
                    continue;

                statusEffect = currentStatusEffect;
                return true;
            }

            statusEffect = null;
            return false;
        }
    }
}
