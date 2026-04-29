using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Code
{
    public struct ViewCastInfo
    {
        public bool isHit;
        public Vector3 point;
        public float distance;
        public float angle;
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;
    }
    
    public class PlayerFOV : MonoBehaviour
    {
        [SerializeField] private LayerMask whatIsEnemy;
        [SerializeField] private LayerMask whatIsObstacle;
        [SerializeField] private float enemyFindDelay = 0.2f;
        [SerializeField] private float meshResolution = 1f;
        [SerializeField] private int iterationCount = 3;
        [SerializeField] private float distanceThreshold = 0.2f;
        
        public List<Transform> visibleTargets = new List<Transform>();
        
        [Range(0, 360f)] public float viewAngle;
        public float viewRadius;

        private Collider[] _enemiesInView;
        private MeshFilter _meshFilter;
        private Mesh _viewMesh;

        private void Awake()
        {
            _meshFilter = transform.Find("ViewVisual").GetComponent<MeshFilter>();
            _viewMesh = new Mesh();
            
            _meshFilter.mesh = _viewMesh;
            
            
        }

        private IEnumerator Start()
        {
            WaitForSeconds delay = new WaitForSeconds(enemyFindDelay);
            
            _enemiesInView = new Collider[20];
            
            while (true)
            {
                yield return delay;
                FindVisibleTargets();
            }
        }

        private void FindVisibleTargets()
        {
            visibleTargets.Clear();
            int cnt = Physics.OverlapSphereNonAlloc(transform.position, viewRadius, _enemiesInView, whatIsEnemy);

            for (int i = 0; i < cnt; ++i)
            {
                Transform enemy = _enemiesInView[i].transform;
                Vector3 direction = enemy.position - transform.position;

                if (Vector3.Angle(transform.forward, direction.normalized) < viewAngle * 0.5f)
                {
                    if (!Physics.Raycast(transform.position, direction.normalized, direction.magnitude, whatIsObstacle))
                    {
                        visibleTargets.Add(enemy);
                    }
                }
            }
        }

        public Vector3 DirFromAngle(float degree, bool isGlobal = false)
        {
            if (!isGlobal)
            {
                degree += transform.eulerAngles.y; 
            }
            float radian = degree * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(radian), 0, Mathf.Cos(radian));
        }

        private void LateUpdate()
        {
            DrawFieldOfView();
        }

        private EdgeInfo FindEdge(ViewCastInfo minCast, ViewCastInfo maxCast)
        {
            float minAngle = minCast.angle;
            float maxAngle = maxCast.angle;
            
            Vector3 minPoint = Vector3.zero;
            Vector3 maxPoint = Vector3.zero;

            for (int i = 0; i < iterationCount; ++i)
            {
                float angle = (minAngle + maxAngle) / 2;
                ViewCastInfo castInfo = ViewCast(angle);
                
                bool edgeDistanceThreshold = Mathf.Abs(minCast.distance - castInfo.distance) > distanceThreshold;

                if (castInfo.isHit == minCast.isHit && !edgeDistanceThreshold)
                {
                    minAngle = angle;
                    minPoint =  castInfo.point;
                }
                else
                {
                    maxAngle = angle;
                    maxPoint = castInfo.point;
                }
            }
            
            return new EdgeInfo{pointA = minPoint, pointB = maxPoint};
        }

        private void DrawFieldOfView()
        {
            int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
            float stepAngleSize = viewAngle / stepCount;

            Vector3 center = transform.position;
            List<Vector3> viewPoints = new List<Vector3>();

            ViewCastInfo oldCastInfo = new ViewCastInfo();
            
            for (int i = 0; i <= stepCount; ++i)
            {
                float angle = transform.eulerAngles.y - viewAngle * 0.5f + stepAngleSize * i;
                
                ViewCastInfo castInfo = ViewCast(angle);

                if (i > 0)
                {
                    bool edgeExceeded = Mathf.Abs(oldCastInfo.distance - castInfo.distance) > distanceThreshold;

                    if (oldCastInfo.isHit != castInfo.isHit || (oldCastInfo.isHit && edgeExceeded))
                    {
                        EdgeInfo edge = FindEdge(oldCastInfo, castInfo);
                        if(edge.pointA != Vector3.zero) viewPoints.Add(edge.pointA);
                        if(edge.pointB != Vector3.zero) viewPoints.Add(edge.pointB);
                    }
                }
                
                viewPoints.Add(castInfo.point);
                oldCastInfo = castInfo;
                //Debug.DrawRay(center, center + DirFromAngle(angle, true) * viewRadius, Color.red);
            }
            
            int vertexCount = viewPoints.Count + 1;
            Vector3[] vertices = new Vector3[vertexCount];
            int[] triangles = new int[(vertexCount - 2) * 3];
                
            
            vertices[0] = Vector3.zero;
            for(int i = 0; i < vertexCount - 1; ++i)
            {
                vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);
                if (i < vertexCount - 2)
                {
                    int tIndex = i * 3;
                    triangles[tIndex] = 0;
                    triangles[tIndex + 1] = i + 1;
                    triangles[tIndex + 2] = i + 2;
                }
            }
            
            _viewMesh.Clear();
            _viewMesh.SetVertices(vertices);
            _viewMesh.SetTriangles(triangles, 0);
            _viewMesh.RecalculateNormals();
        }

        private ViewCastInfo ViewCast(float angle)
        {
            Vector3 direction = DirFromAngle(angle, true);
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, viewRadius, whatIsObstacle))
            {
                return new ViewCastInfo{isHit = true, point = hit.point, angle = angle, distance = hit.distance};
            }
            return new ViewCastInfo{isHit = false, point = transform.position + direction * viewRadius, angle = angle, distance = viewRadius};
        }
    }
}