using System;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.GameEvents;
using Code.UI.Bar;
using DewmoLib.Dependencies;
using DewmoLib.Utiles;
using Scripts.Entities.Vitals;
using Scripts.Players;
using UnityEngine;

namespace InGame.PlayerUI
{
    public class PlayerStatUI : MonoBehaviour
    {
        [SerializeField] private BarComponent thirstBar;
        [SerializeField] private BarComponent healthBar;
        [Inject] private Player _player;
        private LocalEventBus _localEventBus;
        private void Start()
        {
            _localEventBus = _player.Get<LocalEventBus>();
            _localEventBus.Subscribe<FoodChangeEvent>(HandleChangeFood);
            _localEventBus.Subscribe<WaterChangeEvent>(HandleChangeWater);
            _localEventBus.Subscribe<HealthChangeEvent>(HandleChangeHealth);
        }
        private void OnDestroy()
        {
            _localEventBus.Subscribe<FoodChangeEvent>(HandleChangeFood);
            _localEventBus.Subscribe<WaterChangeEvent>(HandleChangeWater);
            _localEventBus.Subscribe<HealthChangeEvent>(HandleChangeHealth);
        }
        private void HandleChangeHealth(HealthChangeEvent @event)
        {
            healthBar.SetBar(@event.CurrentHealth, @event.MaxHealth);
        }

        private void HandleChangeWater(WaterChangeEvent @event)
        {
            thirstBar.SetBar(@event.CurrentWater, @event.MaxWater);
        }

        private void HandleChangeFood(FoodChangeEvent @event)
        {
            //hungerBar.SetBar(@event.CurrentFood, @event.MaxFood);
        }
    }
}