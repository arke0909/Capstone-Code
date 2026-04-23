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
            GameObject enemyObject = Instantiate(policeData.enemyPrefab, spawnPos.position, Quaternion.identity);
            Enemy police = enemyObject.GetComponent<Enemy>();
            
            police.SpawnEnemy(spawnPos.position,policeData);
            
            police.GetComponent<NavMeshAgent>().SetDestination(targetPos.position);
            police.ChangeState(EnemyStateEnum.SprintTo);
            
            police.OnDeadEvent.AddListener(() =>
            {
                policeCount--;
                if (policeCount <= 0)
                {
                    OnEndDispatch.Invoke();
                }
            });
        }
    }
}