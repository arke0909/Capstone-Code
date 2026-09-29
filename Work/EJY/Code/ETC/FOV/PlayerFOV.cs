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
        [SerializeField] private float meshRefreshInterval = 0.05f;
        [SerializeField] private float meshResolution = 1f;
        [SerializeField] private int iterationCount = 3;
        [SerializeField] private float distanceThreshold = 0.2f;
        
        public List<Transform> visibleTargets = new List<Transform>();
        
        [Range(0, 360f)] public float viewAngle;
        public float viewRadius;

        private Collider[] _enemiesInView;
        private MeshFilter _meshFilter;
        private Mesh _viewMesh;
        private readonly List<Vector3> _viewPoints = new List<Vector3>(128);
        private readonly List<Vector3> _vertices = new List<Vector3>(129);
        private readonly List<int> _triangles = new List<int>(384);
        private float _meshRefreshTimer;

        private void Awake()
        {
            _meshFilter = transform.Find("ViewVisual").GetComponent<MeshFilter>();
            _viewMesh = new Mesh();
            _viewMesh.MarkDynamic();
            _meshFilter.mesh = _viewMesh;
            _meshRefreshTimer = meshRefreshInterval;
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

            Vector3 origin = transform.position;
            Vector3 forward = transform.forward;
            float sqrViewRadius = viewRadius * viewRadius;
            float cosHalfViewAngle = Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad);
            int cnt = Physics.OverlapSphereNonAlloc(origin, viewRadius, _enemiesInView, whatIsEnemy);

            for (int i = 0; i < cnt; ++i)
            {
                Transform enemy = _enemiesInView[i].transform;
                Vector3 direction = enemy.position - origin;
                float sqrDistance = direction.sqrMagnitude;
                if (sqrDistance <= Mathf.Epsilon || sqrDistance > sqrViewRadius)
                    continue;

                float distance = Mathf.Sqrt(sqrDistance);
                Vector3 normalizedDirection = direction / distance;
                if (Vector3.Dot(forward, normalizedDirection) < cosHalfViewAngle)
                    continue;

                if (!Physics.Raycast(origin, normalizedDirection, distance, whatIsObstacle))
                    visibleTargets.Add(enemy);
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
            if (meshRefreshInterval > 0f)
            {
                _meshRefreshTimer += Time.deltaTime;
                if (_meshRefreshTimer < meshRefreshInterval)
                    return;

                _meshRefreshTimer = 0f;
            }

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
                    minPoint = castInfo.point;
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
            int stepCount = Mathf.Max(1, Mathf.RoundToInt(viewAngle * meshResolution));
            float stepAngleSize = viewAngle / stepCount;
            _viewPoints.Clear();

            ViewCastInfo oldCastInfo = new ViewCastInfo();
            
            for (int i = 0; i <= stepCount; ++i)
            {
                float angle = transform.eulerAngles.y - viewAngle * 0.5f + stepAngleSize * i;
                
                ViewCastInfo castInfo = ViewCast(angle);

                if (i > 0)
                {
                    bool edgeExceeded = Mathf.Abs(oldCastInfo.distance - castInfo.distance) > distanceThreshold;

                    if (oldCastInfo.isHit != castInfo.isHit || oldCastInfo.isHit && edgeExceeded)
                    {
                        EdgeInfo edge = FindEdge(oldCastInfo, castInfo);
                        if (edge.pointA != Vector3.zero) _viewPoints.Add(edge.pointA);
                        if (edge.pointB != Vector3.zero) _viewPoints.Add(edge.pointB);
                    }
                }
                
                _viewPoints.Add(castInfo.point);
                oldCastInfo = castInfo;
            }

            _vertices.Clear();
            _triangles.Clear();
            _vertices.Add(Vector3.zero);

            for (int i = 0; i < _viewPoints.Count; ++i)
            {
                _vertices.Add(transform.InverseTransformPoint(_viewPoints[i]));
                if (i < _viewPoints.Count - 1)
                {
                    _triangles.Add(0);
                    _triangles.Add(i + 1);
                    _triangles.Add(i + 2);
                }
            }
            
            _viewMesh.Clear();
            _viewMesh.SetVertices(_vertices);
            _viewMesh.SetTriangles(_triangles, 0);
            _viewMesh.RecalculateBounds();
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
