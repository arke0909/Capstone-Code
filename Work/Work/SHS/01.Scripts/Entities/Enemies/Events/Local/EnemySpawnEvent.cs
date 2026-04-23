using Chipmunk.Library.Utility.GameEvents.Local;
using Code.EnemySpawn;

namespace Code.SHS.Entities.Enemies.Events.Local
{
    public struct EnemySpawnEvent : ILocalEvent
    {
        public EnemySO EnemyData;
        public EnemySpawnEvent(EnemySO enemyData)
        {
            EnemyData = enemyData;
        }
    }
}