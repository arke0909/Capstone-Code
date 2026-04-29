using System;
using UnityEngine;

namespace Scripts.Effects
{
    public class PlayParticleVFX : MonoBehaviour, IPlayableVFX
    {
        [field:SerializeField] public string VFXName { get; private set; }

        public Transform EffectTransform => transform;

        [SerializeField] private bool isOnPosition;
        [SerializeField] private ParticleSystem particle;
        
        public void PlayVFX(Vector3 position, Quaternion rotation)
        {
            if(isOnPosition == false)
                transform.SetPositionAndRotation(position, rotation);
            
            particle.Play(true); //트루는 안해줘도 되긴 해
        }

        public void StopVFX()
        {
            particle.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(VFXName) == false)
                gameObject.name = VFXName;
        }
    }
}