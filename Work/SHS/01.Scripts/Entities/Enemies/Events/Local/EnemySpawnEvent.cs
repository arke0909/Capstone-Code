using Chipmunk.Library.Utility.GameEvents.Local;
using Code.EnemySpawn;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.Events.Local
{
    public struct EnemySpawnEvent : ILocalEvent
    {
        public EnemySO EnemyData;
        public Vector3 Position;
        public Quaternion Rotation;

        public EnemySpawnEvent(EnemySO enemyData, Vector3 position, Quaternion rotation)
        {
            EnemyData = enemyData;
            Position = position;
            Rotation = rotation;
        }
    }
}
