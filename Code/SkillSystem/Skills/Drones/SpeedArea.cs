using System;
using System.Collections.Generic;
using DewmoLib.ObjectPool.RunTime;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;

namespace Code.SkillSystem.Skills.Drones
{
    public class SpeedArea : MonoBehaviour, IPoolable
    {
        [SerializeField] private PoolManagerSO poolManagerSO;
        [SerializeField] private PoolItemSO speedAreaItem;
        [SerializeField] private PoolItemSO speedNodeItem;
        [SerializeField] private SplineContainer load;
        [SerializeField] private SplineExtrude loadExtrude;
        
        [SerializeField] private float width = 5f;
        [SerializeField] private float remainTime = 10f;
        //[SerializeField] private float nodeInterval = 1.5f;

        private NavMeshPath _path;
        private List<SpeedNode> _activeNodes = new List<SpeedNode>();
        private Pool _myPool;

        private Vector3 _startPos;
        private Vector3 _endPos;
        private float _timer = 0;

        private void Awake()
        {
            loadExtrude.Radius = width;
        }

        public void SetStartPos(Vector3 pos) => _startPos = pos;
        public void SetEndPos(Vector3 pos) => _endPos = pos;

        public void CreateArea()
        {
            _path = new NavMeshPath();

            if (NavMesh.CalculatePath(_startPos, _endPos, -1, _path))
            {
                GenerateSpeedNodes(_path.corners);
                
            }
        }

        private void GenerateSpeedNodes(Vector3[] corners)
        {
            if (corners.Length < 2) return;

            load.Spline.Clear();
            
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Vector3 start = corners[i];
                Vector3 end = corners[i + 1];
                load.Spline.Add(start);

                Vector3 dir = (end - start).normalized;
                float dist = Vector3.Distance(start, end);

                if (poolManagerSO.Pop(speedNodeItem) is SpeedNode lineNode)
                {
                    lineNode.transform.position = (start + end) / 2f;
                    lineNode.transform.rotation = Quaternion.LookRotation(dir);
                    lineNode.transform.localScale = new Vector3(width, 1, dist);
                    _activeNodes.Add(lineNode);
                }

                if (i < corners.Length - 2)
                {
                    Vector3 nextDir = (corners[i + 2] - end).normalized;
            
                    if (poolManagerSO.Pop(speedNodeItem) is SpeedNode jointNode)
                    {
                        jointNode.transform.position = end;

                        Vector3 miterDir = (dir + nextDir).normalized;
                        jointNode.transform.rotation = Quaternion.LookRotation(miterDir);

                        float angle = Vector3.Angle(dir, nextDir);
                        float miterScaleZ = width * Mathf.Tan(angle * 0.5f * Mathf.Deg2Rad);
                
                        miterScaleZ = Mathf.Clamp(miterScaleZ, 0.1f, width);

                        jointNode.transform.localScale = new Vector3(width, 1, miterScaleZ);
                        _activeNodes.Add(jointNode);
                    }
                }
            }
            load.Spline.Add(corners[^1]);
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= remainTime)
            {
                ClearAllNodes();
                _myPool.Push(this);
            }
        }

        private void ClearAllNodes()
        {
            foreach (var node in _activeNodes)
            {
                if (node != null) node.ReturnToPool();
            }

            _activeNodes.Clear();
        }

        #region IPoolable Implementation

        public PoolItemSO PoolItem => speedAreaItem;
        public GameObject GameObject => gameObject;

        public void SetUpPool(Pool pool) => _myPool = pool;

        public void ResetItem()
        {
            _timer = 0;
        }

        #endregion

        private void OnDrawGizmos()
        {
            if (_path == null || _path.corners.Length == 0) return;

            Gizmos.color = Color.red;

            for (int i = 0; i < _path.corners.Length; i++)
            {
                Gizmos.DrawSphere(_path.corners[i], 1);

                if (i < _path.corners.Length - 1)
                {
                    Gizmos.DrawLine(_path.corners[i], _path.corners[i + 1]);
                }
            }
        }
    }
}