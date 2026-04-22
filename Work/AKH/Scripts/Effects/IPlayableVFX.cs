using UnityEngine;

namespace Scripts.Effects
{
    public interface IPlayableVFX
    {
        public string VFXName { get; }
        public Transform EffectTransform { get; }
        public void PlayVFX(Vector3 position,Quaternion rotation);
        public void StopVFX();
    }
}