using System;
using System.Collections;
using System.Collections.Generic;
using Code.EnemySpawn;
using Code.SHS.Entities.Enemies;
using Code.SHS.Entities.Enemies.FSM;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Code.ETC
{
    public class PoliceStation : MonoBehaviour
    {
        public UnityEvent OnEndDispatch;
        
        [SerializeField] private EnemySO policeData;
        [SerializeField] private Transform spawnPos;
        [SerializeField] private Transform targetPos;
        [SerializeField] private PoliceCar car;
        [SerializeField] private int spawnCount = 5;
        [SerializeField] private float spawnDelay = 0.4f;

        private WaitForSeconds _waitForSeconds;
        
        private int policeCount = 0;

        private void Start()
        {
            car.OnDispatch.AddListener(SpawnAllPolice);
            _waitForSeconds = new WaitForSeconds(spawnDelay);
            policeCount = spawnCount;
        }

        private void OnDestroy()
        {
            car.OnDispatch.RemoveListener(SpawnAllPolice);
        }

        private void SpawnAllPolice()
        {
            StartCoroutine(SpawnAllPoliceCoroutine());
        }

        public IEnumerator SpawnAllPoliceCoroutine()
        {
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnPolice();
                yield return _waitForSeconds;
                Debug.Log("dd");
            }
        }

        public void SpawnPolice()
        {
            Enemy police = EnemySpawnUtility.SpawnEnemy(policeData, spawnPos.position, Quaternion.identity);
            if (police == null)
                return;
            
            police.GetComponent<NavMeshAgent>().SetDestination(targetPos.position);
            police.ChangeState(EnemyStateEnum.SprintTo);
            
            UnityAction handleDead = null;
            handleDead = () =>
            {
                police.OnDeadEvent.RemoveListener(handleDead);
                policeCount--;
                if (policeCount <= 0)
                {
                    OnEndDispatch.Invoke();
                }
            };
            police.OnDeadEvent.AddListener(handleDead);
        }
    }
}
