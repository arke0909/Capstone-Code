using Chipmunk.ComponentContainers;
using Code.ETC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Combat.Fovs
{
    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;
    }

    [Serializable]
    public struct FOVInfo
    {
        [Range(0, 360)] public float viewAngle;
        public float viewRadius;
        public LayerMask _enemyMask;
        public LayerMask _obstacleMask;
    }
    [DefaultExecutionOrder(10000)]
    public class FovCompo : MonoBehaviour, IContainerComponent
    {
        [Serializable]
        private struct BoundaryPoint
        {
            public Vector3 point;
            public bool isWallSide;
            public bool isTransition;
        }
        public Vector3 FovDirection
        {
            get
            {
                return _fovDirection.sqrMagnitude > 0.0001f ? _fovDirection : GetFallbackDirection();
            }
        }

        public ComponentContainer ComponentContainer { get; set; }

        [SerializeField, Min(0.01f)] private float _edgeSoftnessWorld = 0.9f;
        [SerializeField] private bool _detachFromOwnerOnInitialize = true;
        [SerializeField] private bool _ignoreParentRotation = true;
        [SerializeField, Range(0f, 2f)] private float _directionJitterAngle = 0.2f;
        [SerializeField, Min(0f)] private float _directionSmoothSpeed = 45f;

        public float _enemyFindDelay;
        public float _meshResolution;
        public int _edgeResolveIterations;
        public float _edgeDistanceThreshold;

        public List<Transform> visibleTargets = new();
        public FOVInfo[] fovInfos;

        private IAimProvider aimProvider;
        private Transform _ownerTransform;
        private Vector3 _fovDirection = Vector3.forward;
        private List<MeshFilter> _viewMeshFilter;
        private Mesh[] _viewMesh;
        private Coroutine find;
        private readonly HashSet<Transform> before = new();
        private bool _isInitialized;
        private bool _isCameraPreCullRegistered;

        public Vector3 DirFromAngle(float degree, bool angleIsGlobal)
        {
            if (!angleIsGlobal)
                degree += transform.eulerAngles.y;

            float rad = degree * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
        }
        public void OnInitialize(ComponentContainer componentContainer)
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            _ownerTransform = transform.parent;
            if (_detachFromOwnerOnInitialize && _ownerTransform != null)
                transform.SetParent(null, true);

            _viewMeshFilter = new List<MeshFilter>(fovInfos.Length);
            foreach (Transform item in transform)
                _viewMeshFilter.Add(item.GetComponent<MeshFilter>());
            _viewMesh = new Mesh[_viewMeshFilter.Count];
            for (int i = 0; i < _viewMeshFilter.Count; i++)
            {
                _viewMesh[i] = new Mesh { name = "View Mesh" };
                _viewMeshFilter[i].mesh = _viewMesh[i];
            }
            aimProvider = componentContainer?.GetSubclassComponent<IAimProvider>();
            RefreshFovPose();
        }

        private void Start()
        {
#if UNITY_EDITOR //testCode
            if (transform.parent == null)
                OnInitialize(null);
#endif
            SetEnable(true);
        }

        private void OnEnable()
        {
            RegisterCameraPreCull();
        }

        private void OnDisable()
        {
            UnregisterCameraPreCull();
        }

        private void LateUpdate()
        {
            RefreshFovPose();
            RebuildFovMeshes();
        }

        private void RegisterCameraPreCull()
        {
            if (_isCameraPreCullRegistered)
                return;

            Camera.onPreCull += HandleCameraPreCull;
            _isCameraPreCullRegistered = true;
        }

        private void UnregisterCameraPreCull()
        {
            if (!_isCameraPreCullRegistered)
                return;

            Camera.onPreCull -= HandleCameraPreCull;
            _isCameraPreCullRegistered = false;
        }

        private void HandleCameraPreCull(Camera camera)
        {
            if (!isActiveAndEnabled || !IsMainRenderCamera(camera))
                return;

            RefreshFovPose();
            RebuildFovMeshes();
        }

        private static bool IsMainRenderCamera(Camera camera)
        {
            Camera mainCamera = Camera.main;
            return mainCamera == null || camera == mainCamera;
        }

        private void RefreshFovPose()
        {
            StabilizeTransform();
            RefreshFovDirection();
        }

        private void RebuildFovMeshes()
        {
            if (_viewMesh == null || fovInfos == null)
                return;

            int count = Mathf.Min(fovInfos.Length, _viewMesh.Length);
            for (int i = 0; i < count; i++)
                DrawFieldOfView(fovInfos[i], _viewMesh[i]);
        }

        private void StabilizeTransform()
        {
            if (!_ignoreParentRotation)
                return;

            transform.SetPositionAndRotation(GetAimOrigin(), Quaternion.identity);
        }

        private Vector3 GetAimOrigin()
        {
            return _ownerTransform != null ? _ownerTransform.position : transform.position;
        }

        private void RefreshFovDirection()
        {
            Vector3 targetDirection = ResolveTargetDirection();
            if (_fovDirection.sqrMagnitude <= 0.0001f)
            {
                _fovDirection = targetDirection;
                return;
            }

            if (Vector3.Angle(_fovDirection, targetDirection) <= _directionJitterAngle)
                return;

            if (_directionSmoothSpeed <= 0f)
            {
                _fovDirection = targetDirection;
                return;
            }

            float t = 1f - Mathf.Exp(-_directionSmoothSpeed * Time.deltaTime);
            _fovDirection = Vector3.Slerp(_fovDirection, targetDirection, t).normalized;
        }

        private Vector3 ResolveTargetDirection()
        {
            if (aimProvider == null)
                return GetFallbackDirection();

            Vector3 direction = aimProvider.GetAimPosition() - transform.position;
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : GetFallbackDirection();
        }

        private Vector3 GetFallbackDirection()
        {
            Vector3 direction = _ownerTransform != null ? _ownerTransform.forward : transform.forward;

            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
        }
        private EdgeInfo FindEdge(FOVInfo fovInfo, ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
        {
            float minAngle = minViewCast.angle;
            float maxAngle = maxViewCast.angle;
            Vector3 minPoint = Vector3.zero;
            Vector3 maxPoint = Vector3.zero;

            for (int i = 0; i < _edgeResolveIterations; i++)
            {
                float angle = (minAngle + maxAngle) * 0.5f;
                ViewCastInfo castInfo = ViewCast(fovInfo, angle);
                bool edgeDistanceThresholdExceeded = Mathf.Abs(minViewCast.distance - castInfo.distance) > _edgeDistanceThreshold;

                if (castInfo.hit == minViewCast.hit && !edgeDistanceThresholdExceeded)
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

            return new EdgeInfo { pointA = minPoint, pointB = maxPoint };
        }

        private void DrawFieldOfView(FOVInfo fovInfo, Mesh mesh)
        {
            int stepCount = Mathf.Max(1, Mathf.RoundToInt(fovInfo.viewAngle * _meshResolution));
            float stepAngleSize = fovInfo.viewAngle / stepCount;
            List<BoundaryPoint> boundaryPoints = new List<BoundaryPoint>();
            ViewCastInfo oldViewCastInfo = new ViewCastInfo();

            for (int i = 0; i <= stepCount; i++)
            {

                float angle = Quaternion.LookRotation(FovDirection).eulerAngles.y - fovInfo.viewAngle * 0.5f + stepAngleSize * i;
                ViewCastInfo info = ViewCast(fovInfo, angle);

                if (i > 0)
                {
                    bool edgeDistanceThresholdExceeded = Mathf.Abs(oldViewCastInfo.distance - info.distance) > _edgeDistanceThreshold;
                    bool crossingHitState = oldViewCastInfo.hit != info.hit;
                    bool splitHitBoundary = oldViewCastInfo.hit && info.hit && edgeDistanceThresholdExceeded;
                    if (crossingHitState || splitHitBoundary)
                    {
                        EdgeInfo edge = FindEdge(fovInfo, oldViewCastInfo, info);
                        bool pointAWallSide = crossingHitState ? oldViewCastInfo.hit : true;
                        bool pointBWallSide = crossingHitState ? info.hit : true;

                        if (edge.pointA != Vector3.zero)
                            AddBoundaryPoint(boundaryPoints, edge.pointA, pointAWallSide, true);
                        if (edge.pointB != Vector3.zero)
                            AddBoundaryPoint(boundaryPoints, edge.pointB, pointBWallSide, true);
                    }
                }

                oldViewCastInfo = info;
                AddBoundaryPoint(boundaryPoints, info.point, info.hit, false);
            }

            bool isFullCircle = fovInfo.viewAngle >= 359.5f;
            if (isFullCircle)
                MergeLoopEndpoints(boundaryPoints);

            if (boundaryPoints.Count < 2)
            {
                mesh.Clear();
                return;
            }

            List<BoundaryPoint> localBoundaryPoints = new List<BoundaryPoint>(boundaryPoints.Count);
            for (int i = 0; i < boundaryPoints.Count; i++)
            {
                BoundaryPoint worldPoint = boundaryPoints[i];
                localBoundaryPoints.Add(new BoundaryPoint
                {
                    point = transform.InverseTransformPoint(worldPoint.point),
                    isWallSide = worldPoint.isWallSide,
                    isTransition = worldPoint.isTransition
                });
            }

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            int centerIndex = AddVertex(Vector3.zero, new Vector2(1f, 0f), vertices, uvs);
            List<int> boundaryIndices = new List<int>(localBoundaryPoints.Count);
            for (int i = 0; i < localBoundaryPoints.Count; i++)
                boundaryIndices.Add(AddVertex(localBoundaryPoints[i].point, new Vector2(1f, 0f), vertices, uvs));

            for (int i = 0; i < localBoundaryPoints.Count - 1; i++)
            {
                triangles.Add(centerIndex);
                triangles.Add(boundaryIndices[i]);
                triangles.Add(boundaryIndices[i + 1]);
            }

            if (isFullCircle)
            {
                triangles.Add(centerIndex);
                triangles.Add(boundaryIndices[localBoundaryPoints.Count - 1]);
                triangles.Add(boundaryIndices[0]);
            }

            for (int i = 0; i < localBoundaryPoints.Count - 1; i++)
            {
                BoundaryPoint from = localBoundaryPoints[i];
                BoundaryPoint to = localBoundaryPoints[i + 1];
                if (!ShouldApplyEdgeBand(from, to))
                    continue;

                float widthScale = (from.isTransition || to.isTransition) ? 1.45f : 1f;
                AddBoundaryBand(from.point, to.point, vertices, uvs, triangles, widthScale);
            }

            if (isFullCircle)
            {
                BoundaryPoint from = localBoundaryPoints[localBoundaryPoints.Count - 1];
                BoundaryPoint to = localBoundaryPoints[0];
                if (ShouldApplyEdgeBand(from, to))
                {
                    float widthScale = (from.isTransition || to.isTransition) ? 1.45f : 1f;
                    AddBoundaryBand(from.point, to.point, vertices, uvs, triangles, widthScale);
                }
            }
            else
            {
                AddRadialBoundaryBand(localBoundaryPoints[0].point, vertices, uvs, triangles);
                AddRadialBoundaryBand(localBoundaryPoints[localBoundaryPoints.Count - 1].point, vertices, uvs, triangles);
            }

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
        }

        private static void AddBoundaryPoint(List<BoundaryPoint> points, Vector3 point, bool isWallSide, bool isTransition)
        {
            const float mergeDistanceSqr = 0.000001f;
            int lastIndex = points.Count - 1;
            if (lastIndex >= 0 && (points[lastIndex].point - point).sqrMagnitude <= mergeDistanceSqr)
            {
                BoundaryPoint merged = points[lastIndex];
                merged.isWallSide &= isWallSide;
                merged.isTransition |= isTransition;
                points[lastIndex] = merged;
                return;
            }

            points.Add(new BoundaryPoint
            {
                point = point,
                isWallSide = isWallSide,
                isTransition = isTransition
            });
        }

        private static void MergeLoopEndpoints(List<BoundaryPoint> points)
        {
            const float mergeDistanceSqr = 0.000001f;
            int lastIndex = points.Count - 1;
            if (lastIndex <= 0)
                return;

            BoundaryPoint first = points[0];
            BoundaryPoint last = points[lastIndex];
            if ((first.point - last.point).sqrMagnitude > mergeDistanceSqr)
                return;

            first.isWallSide &= last.isWallSide;
            first.isTransition |= last.isTransition;
            points[0] = first;
            points.RemoveAt(lastIndex);
        }

        private static bool ShouldApplyEdgeBand(BoundaryPoint from, BoundaryPoint to)
        {
            return true;
        }

        private void AddBoundaryBand(Vector3 a, Vector3 b, List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, float widthScale = 1f)
        {
            Vector3 edge = b - a;
            if (edge.sqrMagnitude <= 0.000001f)
                return;

            Vector3 normal = Vector3.Cross(Vector3.up, edge).normalized;
            if (normal.sqrMagnitude <= 0.000001f)
                return;

            Vector3 midpoint = (a + b) * 0.5f;
            const float testOffset = 0.1f;
            float scoreA = (midpoint + normal * testOffset).sqrMagnitude;
            float scoreB = (midpoint - normal * testOffset).sqrMagnitude;
            Vector3 outward = scoreA >= scoreB ? normal : -normal;

            float safeScale = Mathf.Max(widthScale, 0.01f);
            float inset = Mathf.Max(_edgeSoftnessWorld * safeScale, 0.01f);

            Vector3 outerA = a + outward * inset;
            Vector3 outerB = b + outward * inset;

            int ia = AddVertex(a, new Vector2(1f, 0f), vertices, uvs);
            int ib = AddVertex(b, new Vector2(1f, 0f), vertices, uvs);
            int oa = AddVertex(outerA, new Vector2(0f, 0f), vertices, uvs);
            int ob = AddVertex(outerB, new Vector2(0f, 0f), vertices, uvs);

            triangles.Add(ia);
            triangles.Add(oa);
            triangles.Add(ob);

            triangles.Add(ia);
            triangles.Add(ob);
            triangles.Add(ib);
        }
        private void AddRadialBoundaryBand(Vector3 boundaryPoint, List<Vector3> vertices, List<Vector2> uvs, List<int> triangles)
        {
            AddRadialBoundaryBand(boundaryPoint, vertices, uvs, triangles, 1f);
        }

        private void AddRadialBoundaryBand(Vector3 boundaryPoint, List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, float widthScale)
        {
            Vector3 dir = new Vector3(boundaryPoint.x, 0f, boundaryPoint.z);
            if (dir.sqrMagnitude <= 0.000001f)
                return;

            dir.Normalize();
            float signedAngle = Vector3.SignedAngle(Vector3.forward, dir, Vector3.up);
            Vector3 outward = signedAngle < 0f
                ? new Vector3(-dir.z, 0f, dir.x)
                : new Vector3(dir.z, 0f, -dir.x);

            AddRadialBoundaryBand(boundaryPoint, outward, vertices, uvs, triangles, widthScale);
        }

        private void AddRadialBoundaryBand(Vector3 boundaryPoint, Vector3 outward, List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, float widthScale)
        {
            if (outward.sqrMagnitude <= 0.000001f)
                return;

            outward.Normalize();
            float safeScale = Mathf.Max(widthScale, 0.01f);
            float inset = Mathf.Max(_edgeSoftnessWorld * safeScale, 0.01f);
            Vector3 outerCenter = outward * inset;
            Vector3 outerBoundary = boundaryPoint + outward * inset;

            int ia = AddVertex(Vector3.zero, new Vector2(1f, 0f), vertices, uvs);
            int ib = AddVertex(boundaryPoint, new Vector2(1f, 0f), vertices, uvs);
            int oa = AddVertex(outerCenter, new Vector2(0f, 0f), vertices, uvs);
            int ob = AddVertex(outerBoundary, new Vector2(0f, 0f), vertices, uvs);

            triangles.Add(ia);
            triangles.Add(oa);
            triangles.Add(ob);

            triangles.Add(ia);
            triangles.Add(ob);
            triangles.Add(ib);
        }

        private static Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector3 ab = b - a;
            float lengthSqr = ab.sqrMagnitude;
            if (lengthSqr <= 0.000001f)
                return a;

            float t = Mathf.Clamp01(Vector3.Dot(point - a, ab) / lengthSqr);
            return a + ab * t;
        }

        private static int AddVertex(Vector3 vertex, Vector2 uv, List<Vector3> vertices, List<Vector2> uvs)
        {
            int index = vertices.Count;
            vertices.Add(vertex);
            uvs.Add(uv);
            return index;
        }

        private ViewCastInfo ViewCast(FOVInfo fovInfo, float globalAngle)
        {
            Vector3 dir = DirFromAngle(globalAngle, true);
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, fovInfo.viewRadius, fovInfo._obstacleMask))
            {
                return new ViewCastInfo { hit = true, point = hit.point, distance = hit.distance, angle = globalAngle };
            }

            return new ViewCastInfo
            {
                hit = false,
                point = transform.position + dir * fovInfo.viewRadius,
                distance = fovInfo.viewRadius,
                angle = globalAngle
            };
        }

        private IEnumerator FindEnemyWithDelay()
        {
            WaitForSeconds time = new WaitForSeconds(_enemyFindDelay);
            while (true)
            {
                yield return time;
                visibleTargets.ForEach(item => before.Add(item));
                visibleTargets.Clear();
                foreach (FOVInfo item in fovInfos)
                    FindVisibleEnemies(item);
                ClearBefore();
            }
        }

        public void SetEnable(bool val)
        {
            if (!val)
            {
                visibleTargets.ForEach(item => before.Add(item));
                visibleTargets.Clear();
                ClearBefore();
                if (find != null)
                    StopCoroutine(find);
            }
            else
            {
                find = StartCoroutine(FindEnemyWithDelay());
            }

            gameObject.SetActive(val);
        }

        private void ClearBefore()
        {
            foreach (Transform enemy in before)
            {
                if (enemy != null && enemy.TryGetComponent(out IFindable findable))
                {
                    if (--findable.SightCount == 0)
                        findable.Escape();
                }
            }
            before.Clear();
        }

        private Collider[] enemiesInView = new Collider[100];
        private void FindVisibleEnemies(FOVInfo fovInfo)
        {
            Array.Clear(enemiesInView, 0, enemiesInView.Length);
            int cnt = Physics.OverlapSphereNonAlloc(transform.position, fovInfo.viewRadius, enemiesInView, fovInfo._enemyMask);
            for (int i = 0; i < cnt; i++)
            {
                Transform enemy = enemiesInView[i].transform;
                if (visibleTargets.Contains(enemy))
                    continue;

                Vector3 enemyPos = enemy.position;
                Vector3 dir = enemyPos - transform.position;
                dir.y = 0;
                Vector3 dirToEnemy = dir.normalized;
                if (Vector3.Angle(FovDirection, dirToEnemy) < fovInfo.viewAngle * 0.5f)
                {
                    if (!Physics.Raycast(transform.position, dirToEnemy, dir.magnitude, fovInfo._obstacleMask))
                    {
                        visibleTargets.Add(enemy);
                        if (!before.Contains(enemy) && enemy.TryGetComponent(out IFindable findable))
                        {
                            if (++findable.SightCount == 1)
                                findable.Founded();
                        }
                        else if (before.Contains(enemy))
                        {
                            before.Remove(enemy);
                        }
                    }
                }
            }
        }


    }
}
