using UnityEngine;

namespace Work.EJY.Code.Guns.HeatReceiver
{
    public class ParticleByHeatRatio : MonoBehaviour, IHeatRatioReceiver
    {
        [SerializeField] private float rateOverTime = 10f;
        [Range(0f,1f),SerializeField] private float baseHeatRatio = 0;
        [SerializeField] private ParticleSystem particle;
        
        public ParticleSystem Particle => particle;
        
        public void SetHeatRatio(float ratio)
        {
            ratio = baseHeatRatio + (1 - baseHeatRatio) * ratio;
            var emission = particle.emission;
            emission.rateOverTime = rateOverTime * ratio;
        }

        public void ResetRatio()
        {
            SetHeatRatio(0);
        }
    }
}