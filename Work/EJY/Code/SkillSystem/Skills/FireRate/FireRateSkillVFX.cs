using Chipmunk.GameEvents;
using Code.GameEvents;
using Scripts.Combat.Datas;
using Scripts.Combat.ItemObjects;
using Scripts.Entities;
using UnityEngine;
using Work.EJY.Code.Guns;
using Work.EJY.Code.Guns.HeatReceiver;

namespace Code.SkillSystem.Skills.FireRate
{
    public class FireRateSkillVFX : MonoBehaviour
    {
        [SerializeField] private ParticleByHeatRatio particleByHeatRatio;

        private GunObject _gunObject;
        private GunHeatFeedback _gunHeatFeedback;
        private Entity _entity;

        public void InitVFXCompo(Entity entity)
        {
            _entity = entity;
            entity.LocalEventBus.Subscribe<ChangeHandlingEvent>(HandleChangeHandlingEvent);
        }

        private void OnDestroy()
        {
            _entity.LocalEventBus.Unsubscribe<ChangeHandlingEvent>(HandleChangeHandlingEvent);
        }

        private void HandleChangeHandlingEvent(ChangeHandlingEvent evt)
        {
            ResetHeatRatio();

            if (evt.EquipableItem is not GunItem gun)
            {
                _gunObject = null;
                _gunHeatFeedback = null;
                return;
            }

            _gunObject = gun.GunObj;
            _gunHeatFeedback = _gunObject.GetComponentInChildren<GunHeatFeedback>();
        }

        public void PlayMuzzleSmog()
        {
            _gunHeatFeedback?.PlayMuzzleSmog();
        }

        public void StopMuzzleSmog()
        {
            _gunHeatFeedback?.StopMuzzleSmog();
        }
        
        public void SetHeatRatio(float ratio)
        {
            if (_gunObject == null) return;
            
            particleByHeatRatio.SetHeatRatio(ratio);
            _gunHeatFeedback.SetHeatRatio(ratio);
        }

        public void ResetHeatRatio()
        {
            if (_gunObject == null) return;
            
            particleByHeatRatio.ResetRatio();
            _gunHeatFeedback.ResetRatio();
        }
    }
}