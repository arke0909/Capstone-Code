using System;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.Guns.HeatReceiver;
using Scripts.Combat.Datas;
using Scripts.Combat.ItemObjects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Guns
{
    public class GunHeatFeedback : MonoBehaviour
    {
        [FormerlySerializedAs("particleByHeatRatio")] [SerializeField] private ParticleByRatio particleByRatio;
        [FormerlySerializedAs("materialByHeatRatio")] [SerializeField] private MaterialByRatio materialByRatio;

        public void PlayMuzzleSmog()
        {
            particleByRatio.Particle.Play();
        }

        public void StopMuzzleSmog()
        {
            particleByRatio.Particle.Stop(); 
        }

        public void SetHeatRatio(float ratio)
        {
            particleByRatio.SetRatio(ratio);
            materialByRatio.SetRatio(ratio);
        }

        public void ResetRatio()
        {
            particleByRatio.ResetRatio();
            materialByRatio.ResetRatio();
        }
    }
}