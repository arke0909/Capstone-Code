using Chipmunk.ComponentContainers;
using Scripts.Effects;
using Scripts.Entities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Entities
{
    public class VFXComponent : MonoBehaviour, IContainerComponent
    {
        private Dictionary<string, IPlayableVFX> _playableDictionary;

        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _playableDictionary = new Dictionary<string, IPlayableVFX>();
            GetComponentsInChildren<IPlayableVFX>().ToList()
                .ForEach(playable => _playableDictionary.Add(playable.VFXName, playable));
        }

        public void PlayVFX(string vfxName, Vector3 position, Quaternion rotation,bool isChildren = true)
        {
            IPlayableVFX vfx = _playableDictionary.GetValueOrDefault(vfxName);
            Debug.Assert(vfx != default(IPlayableVFX), $"{vfxName} is not exist");
            if (!isChildren)
                vfx.EffectTransform.SetParent(null);
            vfx.PlayVFX(position, rotation);
        }

        public void StopVFX(string vfxName)
        {
            IPlayableVFX vfx = _playableDictionary.GetValueOrDefault(vfxName);
            Debug.Assert(vfx != default(IPlayableVFX), $"{vfxName} is not exist");
            vfx.EffectTransform.SetParent(transform);
            //vfx.EffectTransform.localPosition = Vector3.zero;
            vfx.StopVFX();
        }
    }
}