using Chipmunk.ComponentContainers;
using Code.StatusEffectSystem.StatusEffects;
using Entities;
using Scripts.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.StatusEffectSystem
{
    public struct StatusEffectInfo
    {
        public int CreateDataIndex;
        public BuffSO KeySO;
        public AbstractStatusEffectDataSO StatusEffectData;

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
            StatusEffectData = data.statusEffectData;
            Priority = data.priority;
            ApplyTime = keySO.applyTime;
            valueLevel = Mathf.Clamp(valueLevel, 0, data.effectValue.Length - 1);
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
        public event Action<AbstractStatusEffect> OnStatusEffectReleased;
        public event Action<StatusEffectLayer> OnStatusEffectLayerReleased;
        public ComponentContainer ComponentContainer { get; set; }

        private Dictionary<AbstractStatusEffectDataSO, StatusEffectLayer> _noneOverlapStatusEffectLayers =
            new Dictionary<AbstractStatusEffectDataSO, StatusEffectLayer>();

        private Dictionary<BuffSO, StatusEffectLayer> _statusEffectLayers =
            new Dictionary<BuffSO, StatusEffectLayer>();

        private List<AbstractStatusEffect> _expiredStatusEffects = new List<AbstractStatusEffect>();
        private List<StatusEffectLayer> _layerUpdateBuffer = new List<StatusEffectLayer>();
        private Entity _target;
        private VFXComponent _vfxComponent;

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _target = componentContainer.Get<Entity>(true);
            _vfxComponent = componentContainer.Get<VFXComponent>();
        }

        private void OnDestroy()
        {
            ClearStatusEffect();
        }

        private void Update()
        {
            _expiredStatusEffects.Clear();
            _layerUpdateBuffer.Clear();
            _layerUpdateBuffer.AddRange(_statusEffectLayers.Values);

            for (int i = 0; i < _layerUpdateBuffer.Count; i++)
            {
                StatusEffectLayer layer = _layerUpdateBuffer[i];
                if (_statusEffectLayers.TryGetValue(layer.Buff, out StatusEffectLayer activeLayer) &&
                    activeLayer == layer)
                    layer.CollectExpiredStatusEffects(_expiredStatusEffects);
            }

            for (int i = 0; i < _expiredStatusEffects.Count; i++)
                RemoveStatusEffectInstance(_expiredStatusEffects[i]);
        }

        private AbstractStatusEffect CreateStatusEffect(StatusEffectInfo info)
        {
            var data = info.StatusEffectData;
            if (data == null)
            {
                Debug.LogWarning($"Status effect data is null. Buff={info.KeySO?.name}, Index={info.CreateDataIndex}", this);
                return null;
            }

            return data.CreateStatusEffect(_target, info);
        }

        private StatusEffectLayer GetOrCreateLayer(BuffSO buff)
        {
            if (_statusEffectLayers.TryGetValue(buff, out StatusEffectLayer layer))
                return layer;

            layer = new StatusEffectLayer(buff, _target, _vfxComponent);
            _statusEffectLayers.Add(buff, layer);
            return layer;
        }

        private void RemoveLayerIfEmpty(StatusEffectLayer layer)
        {
            if (layer == null || layer.StatusEffectCount > 0)
                return;

            layer.StopVFX();

            if (_statusEffectLayers.TryGetValue(layer.Buff, out StatusEffectLayer activeLayer) &&
                activeLayer == layer)
                _statusEffectLayers.Remove(layer.Buff);
        }

        #region About StatusEffect Apply and Release

        private StatusEffectInfo ApplyStatusEffectFlags(StatusEffectInfo info)
        {
            if (info.StatusEffectData == null)
                return info;

            return info.StatusEffectData.ApplyFlag(info);
        }

        private bool TryRegisterNoneOverlapStatusEffect(
            StatusEffectInfo info,
            AbstractStatusEffect newStatusEffect,
            StatusEffectLayer newLayer,
            out AbstractStatusEffect keptEffect)
        {
            keptEffect = null;

            if (info.CanOverlap)
                return true;

            if (_noneOverlapStatusEffectLayers.TryGetValue(
                    info.StatusEffectData,
                    out StatusEffectLayer oldLayer))
            {
                if (!oldLayer.TryGetStatusEffect(info.StatusEffectData, out AbstractStatusEffect oldEffect))
                {
                    _noneOverlapStatusEffectLayers.Remove(info.StatusEffectData);
                }
                else
                {
                    bool shouldReplace = info.IsOverWrite || oldEffect.Priority <= newStatusEffect.Priority;
                    if (!shouldReplace)
                    {
                        keptEffect = oldEffect;
                        return false;
                    }

                    RemoveStatusEffectInstance(oldEffect);
                }
            }

            _noneOverlapStatusEffectLayers[info.StatusEffectData] = newLayer;
            return true;
        }

        public IEnumerable<AbstractStatusEffect> AddStatusEffect(BuffSO buffSO, object source = null, int level = 0, float additionalTime = 0)
        {
            if (buffSO == null)
            {
                Debug.LogWarning("Tried to add a null BuffSO.", this);
                return Enumerable.Empty<AbstractStatusEffect>();
            }

            return AddStatusEffect(buffSO.GetStatusEffectInfo(level, additionalTime), source);
        }


        public IEnumerable<AbstractStatusEffect> AddStatusEffect(
            IEnumerable<StatusEffectInfo> infos,
            object source = null)
        {
            if (infos == null)
            {
                Debug.LogWarning("Tried to add null status effect infos.", this);
                return Enumerable.Empty<AbstractStatusEffect>();
            }

            List<StatusEffectInfo> preparedInfos = new List<StatusEffectInfo>();
            HashSet<AbstractStatusEffectDataSO> noneOverlapData =
                new HashSet<AbstractStatusEffectDataSO>();
            BuffSO buff = null;

            foreach (StatusEffectInfo rawInfo in infos)
            {
                if (rawInfo.KeySO == null)
                {
                    Debug.LogError("Status effect info has no BuffSO key.", this);
                    return Enumerable.Empty<AbstractStatusEffect>();
                }

                if (buff == null)
                    buff = rawInfo.KeySO;
                else if (buff != rawInfo.KeySO)
                {
                    Debug.LogError(
                        "A status effect request can only contain one BuffSO layer.",
                        this);
                    return Enumerable.Empty<AbstractStatusEffect>();
                }

                StatusEffectInfo info = ApplyStatusEffectFlags(rawInfo);
                if (info.StatusEffectData == null)
                {
                    Debug.LogError(
                        $"{buff.name} contains a null StatusEffectDataSO at index {info.CreateDataIndex}.",
                        this);
                    return Enumerable.Empty<AbstractStatusEffect>();
                }

                if (!info.StatusEffectData.CanApplyTo(_target, out string reason))
                {
                    Debug.LogError(
                        $"{buff.name} cannot apply {info.StatusEffectData.name} to " +
                        $"{(_target == null ? "null" : _target.name)}: {reason}",
                        this);
                    return Enumerable.Empty<AbstractStatusEffect>();
                }

                if (!info.CanOverlap && !noneOverlapData.Add(info.StatusEffectData))
                {
                    Debug.LogError(
                        $"{buff.name} contains duplicate non-overlapping data: " +
                        $"{info.StatusEffectData.name}.",
                        this);
                    return Enumerable.Empty<AbstractStatusEffect>();
                }

                preparedInfos.Add(info);
            }

            if (preparedInfos.Count == 0)
            {
                Debug.LogWarning("Tried to add an empty status effect request.", this);
                return Enumerable.Empty<AbstractStatusEffect>();
            }

            _statusEffectLayers.TryGetValue(buff, out StatusEffectLayer existingLayer);

            for (int i = 0; i < preparedInfos.Count; i++)
            {
                StatusEffectInfo info = preparedInfos[i];
                bool reusesLayerEffect = existingLayer != null &&
                    existingLayer.TryGetStatusEffect(
                        info.StatusEffectData,
                        info.CreateDataIndex,
                        out _);

                if (reusesLayerEffect)
                    continue;

                if (info.CanOverlap ||
                    !_noneOverlapStatusEffectLayers.TryGetValue(
                        info.StatusEffectData,
                        out StatusEffectLayer oldLayer) ||
                    !oldLayer.TryGetStatusEffect(
                        info.StatusEffectData,
                        out AbstractStatusEffect oldEffect))
                    continue;

                if (!info.IsOverWrite && oldEffect.Priority > info.Priority)
                {
                    Debug.LogWarning(
                        $"{buff.name} was rejected because {info.StatusEffectData.name} " +
                        $"has lower priority than the active effect.",
                        this);
                    return Enumerable.Empty<AbstractStatusEffect>();
                }
            }

            List<AbstractStatusEffect> preparedStatusEffects =
                new List<AbstractStatusEffect>(preparedInfos.Count);

            for (int i = 0; i < preparedInfos.Count; i++)
            {
                AbstractStatusEffect statusEffect = CreateStatusEffect(preparedInfos[i]);
                if (statusEffect == null)
                {
                    Debug.LogError(
                        $"{buff.name} failed to prepare all status effects. Nothing was applied.",
                        this);
                    return Enumerable.Empty<AbstractStatusEffect>();
                }

                preparedStatusEffects.Add(statusEffect);
            }

            StatusEffectLayer layer = GetOrCreateLayer(buff);
            List<AbstractStatusEffect> statusEffects =
                new List<AbstractStatusEffect>(preparedInfos.Count);

            for (int i = 0; i < preparedInfos.Count; i++)
            {
                StatusEffectInfo info = preparedInfos[i];

                if (layer.TryAddSharedStack(info, out AbstractStatusEffect stackedEffect))
                {
                    layer.UpdateRuntimeInfo(source);
                    statusEffects.Add(stackedEffect);
                    continue;
                }

                if (!info.CanOverlap &&
                    layer.ResetIfAlreadyApplied(info, out AbstractStatusEffect appliedStatusEffect))
                {
                    layer.UpdateRuntimeInfo(source);
                    statusEffects.Add(appliedStatusEffect);
                    continue;
                }

                AbstractStatusEffect newStatusEffect = preparedStatusEffects[i];
                if (!TryRegisterNoneOverlapStatusEffect(
                        info,
                        newStatusEffect,
                        layer,
                        out AbstractStatusEffect keptEffect))
                {
                    Debug.LogError(
                        $"{buff.name} changed while it was being applied. " +
                        $"{keptEffect.StatusEffectData.name} remained active.",
                        this);
                    RemoveLayerIfEmpty(layer);
                    return statusEffects;
                }

                _statusEffectLayers[buff] = layer;
                layer.UpdateRuntimeInfo(source);
                layer.AddStatusEffect(newStatusEffect);
                statusEffects.Add(newStatusEffect);
            }

            if (layer.StatusEffectCount > 0 &&
                _statusEffectLayers.TryGetValue(buff, out StatusEffectLayer activeLayer) &&
                activeLayer == layer)
                layer.PlayVFX();

            return statusEffects;
        }

        private void RemoveStatusEffectInstance(AbstractStatusEffect statusEffect)
        {
            StatusEffectLayer layer = statusEffect.OwnerLayer;
            if (layer == null || !layer.RemoveStatusEffect(statusEffect))
                return;

            if (_noneOverlapStatusEffectLayers.TryGetValue(
                    statusEffect.StatusEffectData,
                    out StatusEffectLayer registeredLayer) &&
                registeredLayer == layer &&
                !layer.TryGetStatusEffect(statusEffect.StatusEffectData, out _))
                _noneOverlapStatusEffectLayers.Remove(statusEffect.StatusEffectData);

            bool isLayerReleased = layer.StatusEffectCount == 0;
            RemoveLayerIfEmpty(layer);
            OnStatusEffectReleased?.Invoke(statusEffect);

            if (isLayerReleased)
                OnStatusEffectLayerReleased?.Invoke(layer);
        }

        public void RemoveStatusEffect(BuffSO buff)
        {
            RemoveStatusEffect(buff, null);
        }

        public void RemoveStatusEffect(BuffSO buff, object source)
        {
            if (!TryGetLayer(buff, out StatusEffectLayer layer))
                return;

            if (source != null && layer.Source != source)
                return;

            while (layer.TryGetLastStatusEffect(out AbstractStatusEffect statusEffect))
                RemoveStatusEffectInstance(statusEffect);
        }

        public bool TryGetLayer(BuffSO buff, out StatusEffectLayer layer)
        {
            if (buff != null)
                return _statusEffectLayers.TryGetValue(buff, out layer);

            layer = null;
            return false;
        }

        public void ClearStatusEffect()
        {
            while (_statusEffectLayers.Count > 0)
            {
                StatusEffectLayer layer = _statusEffectLayers.Values.First();
                if (layer.TryGetLastStatusEffect(out AbstractStatusEffect statusEffect))
                {
                    RemoveStatusEffectInstance(statusEffect);
                    continue;
                }

                RemoveLayerIfEmpty(layer);
            }

            _expiredStatusEffects.Clear();
            _layerUpdateBuffer.Clear();
            _noneOverlapStatusEffectLayers.Clear();
        }
        #endregion
    }
}
