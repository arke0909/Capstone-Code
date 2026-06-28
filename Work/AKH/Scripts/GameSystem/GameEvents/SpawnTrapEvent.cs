using Code.ETC.MapObjects;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using System.Collections.Generic;
using UnityEngine;
using Work.Code.MapEvents;

namespace Scripts.GameSystem.GameEvents
{
    public class SpawnTrapEvent : DropStructureEvent
    {
        [SerializeField] private PoolItemSO trapItem;
        [SerializeField] private int trapCount; //일차에따라 증가시켜도 될것같긴 한데 굳이? 싶음
        [Inject] private PoolManagerMono _poolManager;
        protected override void Awake()
        {
            base.Awake();
            EventName = "맵에 함정이 배치되었습니다";
        }
        protected override void StartDropStructureEvent()
        {
            if(TryGetRandomAreaPoints(trapCount,out List<AreaPoint> points))
            {
                foreach(var point in points)
                {
                    Trap trap = RegisterDropStructure(_poolManager.Pop<Trap>(trapItem));
                    trap.Spawn(point.Position);
                }
            }
            else
            {
                Debug.Log("asasdsad");
            }
        }
    }
}
