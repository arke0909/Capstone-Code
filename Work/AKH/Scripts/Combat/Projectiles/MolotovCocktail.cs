using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat.Areas;
using UnityEngine;

namespace Scripts.Combat.Projectiles
{
    public class MolotovCocktail : Throw
    {
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private PoolItemSO fireAreaItem;
        [SerializeField] private PoolManagerSO poolManager;
        private void OnCollisionEnter(Collision collision)
        {
            Vector3 areaPos = transform.position;
            if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit,50, groundLayer))
                areaPos = hit.point;
            var area = poolManager.Pop(fireAreaItem) as Area;
            area.Init(_owner, areaPos);
            _myPool.Push(this);
        }
    }
}
