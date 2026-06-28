using System.Collections.Generic;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using UnityEngine;

namespace Work.Code.Misc
{
    [Provide]
    public class ArrowLineManager : MonoBehaviour, IDependencyProvider
    {
        [SerializeField] private PoolItemSO arrowLinePool;
        [Inject] private PoolManagerMono _poolManager;

        private readonly List<ArrowLine> _arrowLineList = new();

        private void OnDestroy()
        {
            ClearLines();
        }

        public ArrowLine CreateLine(GameObject targetA, GameObject targetB)
        {
            if (!targetA || !targetB)
                return null;

            ArrowLine arrowLine = _poolManager.Pop<ArrowLine>(arrowLinePool);
            arrowLine.SetTarget(targetA, targetB);
            _arrowLineList.Add(arrowLine);
            return arrowLine;
        }

        public ArrowLine CreateLine(Transform targetA, Transform targetB)
        {
            if (!targetA || !targetB)
                return null;

            ArrowLine arrowLine = _poolManager.Pop<ArrowLine>(arrowLinePool);
            arrowLine.SetTarget(targetA, targetB);
            _arrowLineList.Add(arrowLine);
            return arrowLine;
        }

        public void RemoveLine(ArrowLine arrowLine)
        {
            if (!arrowLine)
                return;

            _arrowLineList.Remove(arrowLine);
            arrowLine.ReturnToPool();
        }

        public void ClearLines()
        {
            for (int i = 0; i < _arrowLineList.Count; i++)
            {
                if (_arrowLineList[i])
                    _arrowLineList[i].ReturnToPool();
            }

            _arrowLineList.Clear();
        }
    }
}
