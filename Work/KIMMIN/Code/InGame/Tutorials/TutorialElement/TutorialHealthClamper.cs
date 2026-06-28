using System;
using Assets.Work.AKH.Scripts.Entities.Vitals;
using Chipmunk.ComponentContainers;
using Chipmunk.Modules.StatSystem;
using DewmoLib.Dependencies;
using Scripts.Players;
using UnityEngine;

namespace Work.Code.Tutorials
{
    public class TutorialHealthClamper : MonoBehaviour
    {
        [Inject] private Player _player;
        private HealthCompo _healthCompo;

        private void Start()
        {
            _healthCompo = _player.Get<HealthCompo>();
            _healthCompo.OnValueChanged += HandleHeatlhChagned;
        }

        private void OnDestroy()
        {
            _healthCompo.OnValueChanged -= HandleHeatlhChagned;
        }

        private void HandleHeatlhChagned(StatSO vitalstat, float before, float after)
        {
            if (after < 30f)
            {
                _healthCompo.CurrentValue = 30f;
            }
        }
    }
}