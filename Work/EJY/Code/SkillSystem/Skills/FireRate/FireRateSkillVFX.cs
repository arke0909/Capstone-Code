using Chipmunk.GameEvents;
using Code.GameEvents;
using Scripts.Combat.Datas;
using Scripts.Combat.ItemObjects;
using UnityEngine;
using Work.EJY.Code.Guns;
using Work.EJY.Code.Guns.HeatReceiver;

namespace Code.SkillSystem.Skills.FireRate
{
    public class FireRateSkillVFX : MonoBehaviour
    {
        [SerializeField] private ParticleByHeatRatio particleByHeatRatio;

        private GunObject _gunObject;
        private GunOverheatVisual _gunOverheatVisual;

        private void Awake()
        {
            Bus.Subscribe<ChangeHandlingEvent>(HandleChangeHandlingEvent);
        }

        private void OnDestroy()
        {
            Bus.Unsubscribe<ChangeHandlingEvent>(HandleChangeHandlingEvent);
        }

        private void HandleChangeHandlingEvent(ChangeHandlingEvent evt)
        {
            ResetHeatRatio();

            if (evt.EquipableItem is not GunItem gun)
            {
                _gunObject = null;
                _gunOverheatVisual = null;
                return;
            }

            _gunObject = gun.GunObj;
            _gunOverheatVisual = _gunObject.GetComponentInChildren<GunOverheatVisual>();
        }

        public void PlayMuzzleSmog()
        {
            _gunOverheatVisual?.PlayMuzzleSmog();
        }

        public void StopMuzzleSmog()
        {
            _gunOverheatVisual?.StopMuzzleSmog();
        }
        
        public void SetHeatRatio(float ratio)
        {
            if (_gunObject == null) return;
            
            particleByHeatRatio.SetHeatRatio(ratio);
            _gunOverheatVisual.SetHeatRatio(ratio);
        }

        public void ResetHeatRatio()
        {
            if (_gunObject == null) return;
            
            particleByHeatRatio.ResetRatio();
            _gunOverheatVisual.ResetRatio();
        }
    }
}