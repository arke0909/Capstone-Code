using UnityEngine;

namespace Code.SkillSystem.Skills.FireRate
{
    public class FireRateSkillVFX : MonoBehaviour
    {
        [SerializeField] private float bottomRingRateOverTime = 10f;
        [SerializeField] private ParticleSystem bottomRing;
        
        public void SetRateOverTime(float ratio)
        {
            ratio = Mathf.Clamp(ratio, 0.1f, 1);
            
            var emission = bottomRing.emission;
            emission.rateOverTime = bottomRingRateOverTime * ratio;
        }
    }
}