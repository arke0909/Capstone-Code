using AYellowpaper.SerializedCollections;
using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.SHS.Entities.Enemies;
using Code.SHS.Entities.Enemies.Events.Local;
using Code.SHS.Entities.Enemies.FSM;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Enemies.EnemyBehaviours
{
    public class EnemyBehaviourManager : MonoBehaviour, IContainerComponent, ILocalEventSubscriber<EnemySpawnEvent>
    {
        private SerializedDictionary<EnemyStateEnum, List<EnemyBehaviour>> _behaviours = new();
        private readonly List<EnemyBehaviour> _spawnedBehaviours = new();

        public ComponentContainer ComponentContainer { get; set; }
        public EnemyBehaviour CurrentBehaviour { get; private set; }

        private Enemy _enemy;

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _enemy = componentContainer.Get<Enemy>(true);
            foreach (EnemyStateEnum state in Enum.GetValues(typeof(EnemyStateEnum)))
            {
                _behaviours[state] = new List<EnemyBehaviour>();
            }
        }

        public void OnLocalEvent(EnemySpawnEvent spawnEvent)
        {
            ResetRuntimeBehaviours();
            if (spawnEvent.EnemyData == null || spawnEvent.EnemyData.behaviourPrefabs == null)
                return;

            foreach (var enemyBehaviorPatch in spawnEvent.EnemyData.behaviourPrefabs)
            {
                if (enemyBehaviorPatch == null)
                    continue;
                EnemyBehaviour behaviour = Instantiate(enemyBehaviorPatch.Value, transform);
                _spawnedBehaviours.Add(behaviour);
                enemyBehaviorPatch.ApplySetter(behaviour);
                foreach (var state in behaviour.TargetStates)
                    _behaviours[state].Add(behaviour);
                behaviour.Init(_enemy);
            }

            RebuildBehaviourCache();
        }

        public void ResetRuntimeBehaviours()
        {
            CurrentBehaviour = null;
            foreach (var behaviours in _behaviours.Values)
            {
                behaviours.Clear();
            }

            for (int i = 0; i < _spawnedBehaviours.Count; i++)
            {
                EnemyBehaviour behaviour = _spawnedBehaviours[i];
                if (behaviour != null)
                {
                    Destroy(behaviour.gameObject);
                }
            }

            _spawnedBehaviours.Clear();
        }


        private void RebuildBehaviourCache()
        {
            foreach (var behaviours in _behaviours.Values)
            {
                behaviours.RemoveAll(b => b == null);
                behaviours.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            }
        }

        public EnemyBehaviour GetOptimal(EnemyStateEnum state)
            => _behaviours[state].FirstOrDefault(behaviour => behaviour != null && behaviour.Condition());

        public void ExecuteOptimalCurrentState()
        {
            EnemyBehaviour optimalBehaviour = GetOptimal(_enemy.StateMachineBehavior.StateMachine.CurrentStateEnum);
            optimalBehaviour?.Execute();
            CurrentBehaviour = optimalBehaviour;
        }
    }
}
