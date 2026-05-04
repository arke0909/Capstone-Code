using UnityEngine;
using Work.Code.MapEvents;

namespace Work.Code.Map.MapEvents.BossRoomEvent
{
    public class BossRoomTeleportMapEvent : MapEvent
    {
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private BossRoomTeleporter bossRoomTeleporter;

        private BossRoomTeleporter _portalInstance;
        
        protected override void Awake()
        {
            base.Awake();
            _portalInstance = Instantiate(bossRoomTeleporter);
        }

        protected override void StartEvent()
        {
            int randIdx = Random.Range(0, spawnPoints.Length);
            Vector3 spawnPoint = spawnPoints[randIdx].position;
            
            _portalInstance.transform.position = spawnPoint;
        }
    }
}