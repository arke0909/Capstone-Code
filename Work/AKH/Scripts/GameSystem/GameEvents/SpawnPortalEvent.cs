using Chipmunk.GameEvents;
using Code.EnemySpawn;
using Code.UI.Minimap.Core;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using Scripts.GameSystem.Structures;
using UnityEngine;
using Work.Code.MapEvents;
using Random = UnityEngine.Random;

namespace Scripts.GameSystem.GameEvents
{
    public class SpawnPortalEvent : DropStructureEvent
    {
        [SerializeField] private BossSpawner[] targetSpawners;
        [SerializeField] private PoolItemSO structureItem;
        [SerializeField] private Sprite portalIcon;

        [Inject] private PoolManagerMono _poolManager;
        protected override void StartDropStructureEvent()
        {
            if (!TryGetRandomAreaPoint(out AreaPoint spawnPoint) || targetSpawners == null || targetSpawners.Length <= 0)
                return;

            BossSpawner targetSpawner = targetSpawners[Random.Range(0, targetSpawners.Length)];
            if (targetSpawner == null)
                return;

            var item = RegisterDropStructure(_poolManager.Pop<InvokeCallbackStructureWithPool>(structureItem));
            item.Spawn(spawnPoint.Position);
            string iconId = MinimapUtil.AddToMinimap(item, ElementType.Marker, portalIcon, true, item.transform.position);
            item.Init((entity) =>
            {
                targetSpawner.Enter(entity);
                item.Despawn();
            }, () => MinimapUtil.RemoveFromMinimap(iconId));

            EventName = $"{spawnPoint.AreaIndex + 1} 지역 텔레포트 활성화";
        }
    }
}
