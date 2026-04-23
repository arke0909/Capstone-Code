using Chipmunk.ComponentContainers;
using Chipmunk.Modules.StatSystem;
using UnityEngine;

namespace Code.Combat
{
    public class EntityGunStatInfo : MonoBehaviour, IContainerComponent, IAfterInitialze
    {
        [SerializeField] private StatSO reloadSpeedMultiplierStat;
        [SerializeField] private StatSO fireRateStat;
        [SerializeField] private StatSO bulletReduceRateStat;
        
        private StatOverrideBehavior _statCompo;
        
        public ComponentContainer ComponentContainer { get; set; }

        public float ReloadSpeedMultiplier { get; private set; }
        public float FireRate { get; private set; }
        public float BulletReduceRate { get; private set; }
        
        public void OnInitialize(ComponentContainer componentContainer)
        {
            _statCompo = componentContainer.Get<StatOverrideBehavior>();
        }

        public void AfterInitialize()
        {
            var reloadSpeedMultiplier = _statCompo.GetStat(reloadSpeedMultiplierStat);
            ReloadSpeedMultiplier = reloadSpeedMultiplier.Value;
            reloadSpeedMultiplier.OnValueChanged += HandleReloadSpeedValueChanged;
            
            var fireRate = _statCompo.GetStat(fireRateStat);
            FireRate = fireRate.Value;
            fireRate.OnValueChanged += HandleFireRateValueChanged;
            
            var bulletReduceRate = _statCompo.GetStat(bulletReduceRateStat);
            BulletReduceRate = bulletReduceRate.Value;
            bulletReduceRate.OnValueChanged += HandleBulletReduceRateValueChanged;
        }
        
        private void HandleReloadSpeedValueChanged(StatSO stat, float current, float prev)
        {
            ReloadSpeedMultiplier = current;
        }
        
        private void HandleFireRateValueChanged(StatSO stat, float current, float previous)
        {
            FireRate = current;
        }
        
        private void HandleBulletReduceRateValueChanged(StatSO stat, float current, float previous)
        {
            BulletReduceRate = current;
        }
    }
}