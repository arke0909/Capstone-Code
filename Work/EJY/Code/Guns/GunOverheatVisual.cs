using System;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Scripts.Combat.Datas;
using Scripts.Combat.ItemObjects;
using UnityEngine;
using Work.EJY.Code.Guns.HeatReceiver;

namespace Work.EJY.Code.Guns
{
    public class GunOverheatVisual : MonoBehaviour
    {
        [SerializeField] private ParticleByHeatRatio particleByHeatRatio;
        [SerializeField] private MaterialByHeatRatio materialByHeatRatio;

        public void PlayMuzzleSmog()
        {
            particleByHeatRatio.Particle.Play();
        }

        public void StopMuzzleSmog()
        {
            particleByHeatRatio.Particle.Stop(); 
        }

        public void SetHeatRatio(float ratio)
        {
            particleByHeatRatio.SetHeatRatio(ratio);
            materialByHeatRatio.SetHeatRatio(ratio);
        }

        public void ResetRatio()
        {
            particleByHeatRatio.ResetRatio();
            materialByHeatRatio.ResetRatio();
        }
    }
}