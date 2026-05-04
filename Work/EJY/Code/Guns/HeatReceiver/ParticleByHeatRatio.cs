using UnityEngine;

namespace Work.EJY.Code.Guns.HeatReceiver
{
    public class ParticleByHeatRatio : MonoBehaviour, IHeatRatioReceiver
    {
        [SerializeField] private float rateOverTime = 10f;
        [SerializeField] private ParticleSystem particle;
        
        public ParticleSystem Particle => particle;
        
        public void SetHeatRatio(float ratio)
        {
            ratio = 0.1f + 0.9f * ratio;
            var emission = particle.emission;
            emission.rateOverTime = rateOverTime * ratio;
        }

        public void ResetRatio()
        {
            SetHeatRatio(0);
        }
    }
}