using Chipmunk.GameEvents;
using Code.SHS.Entities.Enemies;
using Code.SHS.Entities.Enemies.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.ETC.Sound
{
    public enum CombatBGMType
    {
        Field,
        Elite,
        Boss
    }

    public class CombatBGMState
    {
        private readonly HashSet<int> _activeEliteIds = new();
        private readonly HashSet<int> _activeBossIds = new();

        public CombatBGMType Current { get; private set; } = CombatBGMType.Field;

        public bool SetCombat(int enemyId, CombatBGMType type, bool isInCombat)
        {
            HashSet<int> activeIds = type switch
            {
                CombatBGMType.Elite => _activeEliteIds,
                CombatBGMType.Boss => _activeBossIds,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type,
                    "Field BGM cannot be registered as an enemy combat state.")
            };

            bool membershipChanged = isInCombat
                ? activeIds.Add(enemyId)
                : activeIds.Remove(enemyId);

            if (!membershipChanged)
                return false;

            CombatBGMType next = _activeBossIds.Count > 0
                ? CombatBGMType.Boss
                : _activeEliteIds.Count > 0
                    ? CombatBGMType.Elite
                    : CombatBGMType.Field;

            if (Current == next)
                return false;

            Current = next;
            return true;
        }
    }

    public class CombatBGMController : MonoBehaviour
    {
        [SerializeField] private BGMPlayer fieldBGMPlayer;
        [SerializeField] private BGMPlayer eliteBGMPlayer;
        [SerializeField] private BGMPlayer bossBGMPlayer;

        private readonly CombatBGMState _state = new();

        private void Awake()
        {
            EventBus.Subscribe<EnemyCombatStateChangedEvent>(HandleCombatStateChanged);
            ValidateBGM(fieldBGMPlayer, nameof(fieldBGMPlayer), true);
            ValidateBGM(eliteBGMPlayer, nameof(eliteBGMPlayer), false);
            ValidateBGM(bossBGMPlayer, nameof(bossBGMPlayer), true);
        }

        private void Start()
        {
            PlayBGM(_state.Current);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<EnemyCombatStateChangedEvent>(HandleCombatStateChanged);
        }

        private void HandleCombatStateChanged(EnemyCombatStateChangedEvent evt)
        {
            if (!TryResolveCombatBGMType(evt.Enemy, out CombatBGMType type))
                return;

            if (_state.SetCombat(evt.Enemy.GetInstanceID(), type, evt.IsInCombat))
                PlayBGM(_state.Current);
        }

        private static bool TryResolveCombatBGMType(Enemy enemy, out CombatBGMType type)
        {
            if (enemy is Boss)
            {
                type = CombatBGMType.Boss;
                return true;
            }

            if (enemy != null && enemy.EnemyData != null && enemy.EnemyData.isElite)
            {
                type = CombatBGMType.Elite;
                return true;
            }

            type = CombatBGMType.Field;
            return false;
        }

        private void PlayBGM(CombatBGMType type)
        {
            BGMPlayer player = type switch
            {
                CombatBGMType.Field => fieldBGMPlayer,
                CombatBGMType.Elite => eliteBGMPlayer,
                CombatBGMType.Boss => bossBGMPlayer,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            if (player == null || !player.IsValid)
            {
                Debug.LogError($"[{nameof(CombatBGMController)}] {type} BGM is not configured.", this);

                if (type != CombatBGMType.Field && fieldBGMPlayer != null && fieldBGMPlayer.IsValid)
                    fieldBGMPlayer.PlayBGM();

                return;
            }

            player.PlayBGM();
        }

        private void ValidateBGM(BGMPlayer player, string fieldName, bool required)
        {
            if (player != null && player.IsValid)
                return;

            string message = $"[{nameof(CombatBGMController)}] {fieldName} is not configured.";
            if (required)
                Debug.LogError(message, this);
            else
                Debug.LogWarning(message, this);
        }
    }
}
