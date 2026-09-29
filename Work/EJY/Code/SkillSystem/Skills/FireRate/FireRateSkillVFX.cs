using Chipmunk.GameEvents;
using Code.GameEvents;
using Scripts.Combat.Datas;
using Scripts.Combat.ItemObjects;
using Scripts.Entities;
using UnityEngine;
using UnityEngine.Serialization;
using Code.Guns;
using Code.Guns.HeatReceiver;

namespace Code.SkillSystem.Skills.FireRate
{
    public class FireRateSkillVFX : MonoBehaviour
    {
        [FormerlySerializedAs("particleByHeatRatio")] [SerializeField] private ParticleByRatio particleByRatio;

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
            _entity?.LocalEventBus.Unsubscribe<ChangeHandlingEvent>(HandleChangeHandlingEvent);
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
            Debug.Assert(_gunHeatFeedback != null, $"{gun} has not heat feedback");
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
            
            particleByRatio.SetRatio(ratio);
            _gunHeatFeedback.SetHeatRatio(ratio);
        }

        public void ResetHeatRatio()
        {
            if (_gunObject == null) return;
            
            particleByRatio.ResetRatio();
            _gunHeatFeedback.ResetRatio();
        }
    }
}