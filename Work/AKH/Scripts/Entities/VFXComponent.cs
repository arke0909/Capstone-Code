using Chipmunk.ComponentContainers;
using Scripts.Effects;
using Scripts.Entities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Entities
{
    public class VFXComponent : MonoBehaviour, IContainerComponent, IAfterInitialze
    {
        private Dictionary<string, IPlayableVFX> _playableDictionary;
        private readonly Dictionary<IPlayableVFX, Renderer[]> _renderersByVFX = new(); //일단 임시로 파티클만 적용할게요
        private readonly HashSet<IPlayableVFX> _registeredVisibleVFX = new();
        private FindableRenderer _findableRenderer;
        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _playableDictionary = new Dictionary<string, IPlayableVFX>();
            GetComponentsInChildren<IPlayableVFX>().ToList()
                .ForEach(playable => _playableDictionary.Add(playable.VFXName, playable));
            _findableRenderer = componentContainer.Get<FindableRenderer>();
        }

        public void AfterInitialize()
        {
            RegisterAllChildRenderers();
        }

        public void PlayVFX(string vfxName, Vector3 position, Quaternion rotation, bool isChildren = true)
        {
            if (_playableDictionary.TryGetValue(vfxName, out IPlayableVFX vfx))
            {
                Debug.Assert(vfx != default(IPlayableVFX), $"{vfxName} is not exist");
                if (!isChildren)
                {
                    UnregisterVisibleRenderers(vfx);
                    vfx.EffectTransform.SetParent(null);
                    vfx.EffectTransform.gameObject.SetActive(true);
                }
                else
                {
                    RegisterVisibleRenderers(vfx);
                }

                vfx.PlayVFX(position, rotation);
            }
        }

        public void StopVFX(string vfxName)
        {
            if (_playableDictionary.TryGetValue(vfxName, out IPlayableVFX vfx))
            {
                Debug.Assert(vfx != default(IPlayableVFX), $"{vfxName} is not exist");
                vfx.EffectTransform.SetParent(transform);
                //vfx.EffectTransform.localPosition = Vector3.zero;
                vfx.StopVFX();
            }
        }

        private void RegisterAllChildRenderers()
        {
            if (_findableRenderer == null)
                return;

            foreach (IPlayableVFX playableVFX in _playableDictionary.Values)
            {
                RegisterVisibleRenderers(playableVFX);
            }
        }

        private void RegisterVisibleRenderers(IPlayableVFX vfx)
        {
            if (_findableRenderer == null || vfx == null)
                return;

            if (_registeredVisibleVFX.Add(vfx) == false)
                return;

            _findableRenderer.AddRenderers(GetRenderers(vfx));
        }

        private void UnregisterVisibleRenderers(IPlayableVFX vfx)
        {
            if (_findableRenderer == null || vfx == null)
                return;

            if (_registeredVisibleVFX.Remove(vfx) == false)
                return;

            _findableRenderer.RemoveRenderers(GetRenderers(vfx));
        }

        private Renderer[] GetRenderers(IPlayableVFX vfx)
        {
            if (_renderersByVFX.TryGetValue(vfx, out Renderer[] renderers))
                return renderers;

            renderers = vfx.EffectTransform.GetComponentsInChildren<Renderer>(true);
            _renderersByVFX[vfx] = renderers;
            return renderers;
        }
    }
}
