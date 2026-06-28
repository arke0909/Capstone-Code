using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Work.Code.MapEvents
{
    public interface ISpawnableStructure
    {
        void Spawn(Vector3 targetPos);
        void Despawn();
    }
    public readonly struct AreaPoint
    {
        public int AreaIndex { get; }
        public Transform Root { get; }
        public Transform Point { get; }
        public Vector3 Position => Point.position;

        public AreaPoint(int areaIndex, Transform root, Transform point)
        {
            AreaIndex = areaIndex;
            Root = root;
            Point = point;
        }
    }
    public abstract class DropStructureEvent : MapEvent
    {
        [SerializeField] private Transform[] roots;

        private readonly Stack<ISpawnableStructure> _dropStructures = new();

        protected int AreaCount => roots?.Length ?? 0;

        protected sealed override void StartEvent()
        {
            ClearDropStructures();
            StartDropStructureEvent();
        }

        protected abstract void StartDropStructureEvent();

        protected T RegisterDropStructure<T>(T dropStructure) where T : ISpawnableStructure
        {
            if (dropStructure != null)
                _dropStructures.Push(dropStructure);
            return dropStructure;
        }

        private void ClearDropStructures()
        {
            while(_dropStructures.Count > 0)
            {
                ISpawnableStructure dropStructure = _dropStructures.Pop();
                dropStructure?.Despawn();
            }
        }

        protected bool TryGetRandomAreaPoint(out AreaPoint areaPoint)
        {
            areaPoint = default;

            if (AreaCount <= 0)
                return false;

            int startIndex = Random.Range(0, AreaCount);
            for (int i = 0; i < AreaCount; i++)
            {
                int areaIndex = (startIndex + i) % AreaCount;
                if (TryGetRandomAreaPoint(areaIndex, out areaPoint))
                    return true;
            }

            return false;
        }

        protected bool TryGetRandomAreaPoint(int areaIndex, out AreaPoint areaPoint)
        {
            areaPoint = default;

            if (roots == null || areaIndex < 0 || areaIndex >= roots.Length)
                return false;

            Transform root = roots[areaIndex];
            if (root == null || root.childCount <= 0)
                return false;

            Transform point = root.GetChild(Random.Range(0, root.childCount));
            areaPoint = new AreaPoint(areaIndex, root, point);
            return true;
        }

        protected bool TryGetRandomAreaPoints(int count, out List<AreaPoint> areaPoints)
        {
            areaPoints = new List<AreaPoint>();

            if (count <= 0)
                return true;

            if (roots == null || roots.Length <= 0)
                return false;

            List<AreaPoint> candidates = new();
            for (int areaIndex = 0; areaIndex < roots.Length; areaIndex++)
            {
                Transform root = roots[areaIndex];
                if (root == null)
                    continue;

                foreach (Transform point in root)
                {
                    if (point != null)
                        candidates.Add(new AreaPoint(areaIndex, root, point));
                }
            }

            if (candidates.Count < count)
                return false;

            for (int i = 0; i < count; i++)
            {
                int rand = Random.Range(i, candidates.Count);
                (candidates[i], candidates[rand]) = (candidates[rand], candidates[i]);
                areaPoints.Add(candidates[i]);
            }

            return true;
        }

        protected void FillShuffledAreaIndices(Span<int> indices)
        {
            int length = Mathf.Min(indices.Length, AreaCount);

            for (int i = 0; i < length; i++)
            {
                indices[i] = i;
            }

            for (int i = 0; i < length; i++)
            {
                int rand = Random.Range(i, length);
                (indices[i], indices[rand]) = (indices[rand], indices[i]);
            }
        }

#if UNITY_EDITOR
        protected virtual string GetAreaPointName(int areaIndex, int pointIndex)
        {
            return $"Area{areaIndex + 1}_Pos{pointIndex + 1}";
        }

        protected virtual void OnValidate()
        {
            if (roots == null)
                return;

            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] == null)
                    continue;

                foreach (Transform trm in roots[i])
                {
                    trm.name = GetAreaPointName(i, trm.GetSiblingIndex());
                }
            }
        }
#endif
    }
}
