using Chipmunk.ComponentContainers;
using Code.ETC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
        private const int MinEdgeResolveIterations = 7;
        private const float TransitionCapScale = 1.25f;

        // [최적화] UV 상수를 static readonly로 캐싱 — AddVertex 호출마다 struct 생성 불필요
        private static readonly Vector2 UvVisible = new Vector2(1f, 0f);
        private static readonly Vector2 UvHidden  = new Vector2(0f, 0f);

        [Serializable]
        private struct BoundaryPoint
        {
            public Vector3 point;
            public bool isWallSide;
            public bool isTransition;
        }

        public Vector3 FovDirection
        {
            get => _fovDirection.sqrMagnitude > 0.0001f ? _fovDirection : GetFallbackDirection();
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
        // [최적화] visibleTargets.Contains() O(n) → HashSet O(1) 중복 검사용
        private readonly HashSet<Transform> _visibleTargetsSet = new();
        private readonly List<BoundaryPoint> _boundaryPoints = new();
        private readonly List<Vector3> _vertices = new();
        private readonly List<Vector2> _uvs = new();
        private readonly List<int> _triangles = new();
        private readonly List<int> _boundaryIndices = new();
        private readonly List<Vector3> _innerPoints = new();
        private readonly List<float> _widthScales = new();
        private readonly List<Vector3> _edgeOutwards = new();
        private readonly List<int> _innerIndices = new();
        private readonly List<int> _outerIndices = new();
        private bool _isInitialized;
        private bool _isCameraPreCullRegistered;
        private int _lastRenderPoseFrame = -1;

        // [최적화] NonAlloc 배열 — readonly로 선언해 재할당 방지
        private readonly Collider[] enemiesInView = new Collider[100];

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

        private void RegisterCameraPreCull()
        {
            if (_isCameraPreCullRegistered)
                return;

            Camera.onPreCull += HandleCameraPreCull;
            RenderPipelineManager.beginCameraRendering += HandleBeginCameraRendering;
            _isCameraPreCullRegistered = true;
        }

        private void UnregisterCameraPreCull()
        {
            if (!_isCameraPreCullRegistered)
                return;

            Camera.onPreCull -= HandleCameraPreCull;
            RenderPipelineManager.beginCameraRendering -= HandleBeginCameraRendering;
            _isCameraPreCullRegistered = false;
        }

        private void HandleCameraPreCull(Camera camera) => HandleCameraPreRender(camera);

        private void HandleBeginCameraRendering(ScriptableRenderContext context, Camera camera)
            => HandleCameraPreRender(camera);

        private void HandleCameraPreRender(Camera camera)
        {
            if (!isActiveAndEnabled || !IsMainRenderCamera(camera))
                return;

            if (_lastRenderPoseFrame == Time.frameCount)
                return;

            _lastRenderPoseFrame = Time.frameCount;
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
            => _ownerTransform != null ? _ownerTransform.position : transform.position;

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

            Vector3 direction = GetCurrentAimPosition() - transform.position;
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : GetFallbackDirection();
        }

        private Vector3 GetCurrentAimPosition() => aimProvider.GetAimPosition();

        private Vector3 GetFallbackDirection()
        {
            Vector3 direction = _ownerTransform != null ? _ownerTransform.forward : transform.forward;
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
        }

        // [최적화] origin 파라미터 추가 — 호출마다 transform.position native 접근 제거
        private EdgeInfo FindEdge(FOVInfo fovInfo, ViewCastInfo minViewCast, ViewCastInfo maxViewCast, Vector3 origin)
        {
            float minAngle = minViewCast.angle;
            float maxAngle = maxViewCast.angle;
            Vector3 minPoint = Vector3.zero;
            Vector3 maxPoint = Vector3.zero;

            int resolveIterations = Mathf.Max(_edgeResolveIterations, MinEdgeResolveIterations);
            for (int i = 0; i < resolveIterations; i++)
            {
                float angle = (minAngle + maxAngle) * 0.5f;
                ViewCastInfo castInfo = ViewCast(fovInfo, angle, origin);
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

            // [최적화] FovDirection 프로퍼티를 루프마다 호출하지 않고 1회 캐싱
            // [최적화] Quaternion.LookRotation().eulerAngles.y → Mathf.Atan2으로 교체 (Quaternion 생성 제거)
            Vector3 fovDir = FovDirection;
            float baseAngle = Mathf.Atan2(fovDir.x, fovDir.z) * Mathf.Rad2Deg - fovInfo.viewAngle * 0.5f;

            // [최적화] transform.position native 접근을 루프 밖으로 1회 캐싱
            Vector3 origin = transform.position;

            List<BoundaryPoint> boundaryPoints = _boundaryPoints;
            boundaryPoints.Clear();
            ViewCastInfo oldViewCastInfo = default;

            for (int i = 0; i <= stepCount; i++)
            {
                float angle = baseAngle + stepAngleSize * i;
                ViewCastInfo info = ViewCast(fovInfo, angle, origin);

                if (i > 0)
                {
                    bool edgeDistanceThresholdExceeded = Mathf.Abs(oldViewCastInfo.distance - info.distance) > _edgeDistanceThreshold;
                    bool crossingHitState = oldViewCastInfo.hit != info.hit;
                    bool splitHitBoundary = oldViewCastInfo.hit && info.hit && edgeDistanceThresholdExceeded;
                    if (crossingHitState || splitHitBoundary)
                    {
                        EdgeInfo edge = FindEdge(fovInfo, oldViewCastInfo, info, origin);
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

            Span<BoundaryPoint> localBoundaryPoints = stackalloc BoundaryPoint[boundaryPoints.Count];
            for (int i = 0; i < boundaryPoints.Count; i++)
            {
                BoundaryPoint worldPoint = boundaryPoints[i];
                localBoundaryPoints[i] = new BoundaryPoint
                {
                    point = transform.InverseTransformPoint(worldPoint.point),
                    isWallSide = worldPoint.isWallSide,
                    isTransition = worldPoint.isTransition
                };
            }

            List<Vector3> vertices = _vertices;
            List<Vector2> uvs = _uvs;
            List<int> triangles = _triangles;
            vertices.Clear();
            uvs.Clear();
            triangles.Clear();

            // [최적화] new Vector2(1f,0f) 반복 생성 → 캐싱된 UvVisible/UvHidden 사용
            int centerIndex = AddVertex(Vector3.zero, UvVisible, vertices, uvs);
            List<int> boundaryIndices = _boundaryIndices;
            boundaryIndices.Clear();
            for (int i = 0; i < localBoundaryPoints.Length; i++)
                boundaryIndices.Add(AddVertex(localBoundaryPoints[i].point, UvVisible, vertices, uvs));

            for (int i = 0; i < localBoundaryPoints.Length - 1; i++)
            {
                triangles.Add(centerIndex);
                triangles.Add(boundaryIndices[i]);
                triangles.Add(boundaryIndices[i + 1]);
            }

            if (isFullCircle)
            {
                triangles.Add(centerIndex);
                triangles.Add(boundaryIndices[localBoundaryPoints.Length - 1]);
                triangles.Add(boundaryIndices[0]);
            }

            AddContinuousBoundaryBand(localBoundaryPoints, isFullCircle, vertices, uvs, triangles);

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

        private void AddContinuousBoundaryBand(ReadOnlySpan<BoundaryPoint> boundaryPoints, bool isFullCircle, List<Vector3> vertices, List<Vector2> uvs, List<int> triangles)
        {
            int polygonPointCount = isFullCircle ? boundaryPoints.Length : boundaryPoints.Length + 1;
            if (polygonPointCount < 3)
                return;

            List<Vector3> innerPoints = _innerPoints;
            List<float> widthScales = _widthScales;
            innerPoints.Clear();
            widthScales.Clear();
            if (!isFullCircle)
            {
                innerPoints.Add(Vector3.zero);
                widthScales.Add(1f);
            }

            for (int i = 0; i < boundaryPoints.Length; i++)
            {
                innerPoints.Add(boundaryPoints[i].point);
                widthScales.Add(GetBoundaryPointWidthScale(boundaryPoints, i, isFullCircle));
            }

            float signedArea = CalculateSignedAreaXZ(innerPoints);
            if (Mathf.Abs(signedArea) <= 0.000001f)
                return;

            List<Vector3> edgeOutwards = _edgeOutwards;
            BuildEdgeOutwards(innerPoints, signedArea, edgeOutwards);
            if (edgeOutwards.Count != polygonPointCount)
                return;

            List<int> innerIndices = _innerIndices;
            List<int> outerIndices = _outerIndices;
            innerIndices.Clear();
            outerIndices.Clear();
            for (int i = 0; i < polygonPointCount; i++)
            {
                Vector3 outward = GetMiterOutward(edgeOutwards, i, out float miterScale);
                float inset = Mathf.Max(_edgeSoftnessWorld * Mathf.Max(widthScales[i], 0.01f), 0.01f);
                Vector3 outerPoint = innerPoints[i] + outward * inset * miterScale;

                innerIndices.Add(AddVertex(innerPoints[i], UvVisible, vertices, uvs));
                outerIndices.Add(AddVertex(outerPoint, UvHidden, vertices, uvs));
            }

            for (int i = 0; i < polygonPointCount; i++)
            {
                int next = (i + 1) % polygonPointCount;
                AddBoundaryQuad(innerIndices[i], innerIndices[next], outerIndices[i], outerIndices[next], triangles);
            }

            int boundaryIndexOffset = isFullCircle ? 0 : 1;
            for (int i = 0; i < boundaryPoints.Length; i++)
            {
                if (!boundaryPoints[i].isTransition)
                    continue;

                AddTransitionCap(
                    boundaryIndexOffset + i,
                    innerPoints,
                    edgeOutwards,
                    widthScales,
                    vertices,
                    uvs,
                    triangles);
            }
        }

        private static float GetBoundaryPointWidthScale(ReadOnlySpan<BoundaryPoint> boundaryPoints, int index, bool isFullCircle)
        {
            bool currentTransition = boundaryPoints[index].isTransition;
            bool previousTransition = isFullCircle
                ? boundaryPoints[(index - 1 + boundaryPoints.Length) % boundaryPoints.Length].isTransition
                : index > 0 && boundaryPoints[index - 1].isTransition;
            bool nextTransition = isFullCircle
                ? boundaryPoints[(index + 1) % boundaryPoints.Length].isTransition
                : index < boundaryPoints.Length - 1 && boundaryPoints[index + 1].isTransition;

            return currentTransition || previousTransition || nextTransition ? 1.45f : 1f;
        }

        private static float CalculateSignedAreaXZ(List<Vector3> points)
        {
            float area = 0f;
            // [최적화] .Count 프로퍼티 접근을 루프 밖으로 캐싱
            int count = points.Count;
            for (int i = 0; i < count; i++)
            {
                Vector3 current = points[i];
                Vector3 next = points[(i + 1) % count];
                area += current.x * next.z - next.x * current.z;
            }

            return area * 0.5f;
        }

        private static void BuildEdgeOutwards(List<Vector3> points, float signedArea, List<Vector3> outwards)
        {
            outwards.Clear();
            float outwardSign = signedArea >= 0f ? 1f : -1f;
            // [최적화] .Count 캐싱
            int count = points.Count;
            for (int i = 0; i < count; i++)
            {
                Vector3 edge = points[(i + 1) % count] - points[i];
                if (edge.sqrMagnitude <= 0.000001f)
                {
                    outwards.Add(Vector3.zero);
                    continue;
                }

                outwards.Add(Vector3.Cross(Vector3.up, edge).normalized * outwardSign);
            }
        }

        private static Vector3 GetMiterOutward(List<Vector3> edgeOutwards, int index, out float miterScale)
        {
            // [최적화] .Count 캐싱
            int count = edgeOutwards.Count;
            Vector3 previousOutward = edgeOutwards[(index - 1 + count) % count];
            Vector3 nextOutward = edgeOutwards[index];
            Vector3 miter = previousOutward + nextOutward;

            if (miter.sqrMagnitude <= 0.000001f)
            {
                miterScale = 1f;
                return nextOutward.sqrMagnitude > 0.000001f ? nextOutward : previousOutward;
            }

            miter.Normalize();
            float alignment = Vector3.Dot(miter, nextOutward);
            if (alignment <= 0.15f)
            {
                miterScale = 1f;
                return nextOutward;
            }

            miterScale = Mathf.Clamp(1f / alignment, 1f, 2.5f);
            return miter;
        }

        private static void AddBoundaryQuad(int innerA, int innerB, int outerA, int outerB, List<int> triangles)
        {
            triangles.Add(innerA);
            triangles.Add(outerA);
            triangles.Add(outerB);

            triangles.Add(innerA);
            triangles.Add(outerB);
            triangles.Add(innerB);
        }

        private void AddTransitionCap(int pointIndex, List<Vector3> innerPoints, List<Vector3> edgeOutwards, List<float> widthScales, List<Vector3> vertices, List<Vector2> uvs, List<int> triangles)
        {
            int edgeCount = edgeOutwards.Count;
            Vector3 previousOutward = edgeOutwards[(pointIndex - 1 + edgeCount) % edgeCount];
            Vector3 nextOutward = edgeOutwards[pointIndex];
            if (previousOutward.sqrMagnitude <= 0.000001f || nextOutward.sqrMagnitude <= 0.000001f)
                return;

            Vector3 miterOutward = GetMiterOutward(edgeOutwards, pointIndex, out float miterScale);
            if (miterOutward.sqrMagnitude <= 0.000001f)
                return;

            float inset = Mathf.Max(_edgeSoftnessWorld * Mathf.Max(widthScales[pointIndex], 0.01f), 0.01f);
            Vector3 innerPoint = innerPoints[pointIndex];
            Vector3 previousCapPoint = innerPoint + previousOutward.normalized * inset * TransitionCapScale;
            Vector3 miterCapPoint = innerPoint + miterOutward.normalized * inset * Mathf.Min(miterScale, 2f) * TransitionCapScale;
            Vector3 nextCapPoint = innerPoint + nextOutward.normalized * inset * TransitionCapScale;

            int inner       = AddVertex(innerPoint,        UvVisible, vertices, uvs);
            int previousCap = AddVertex(previousCapPoint,  UvHidden,  vertices, uvs);
            int miterCap    = AddVertex(miterCapPoint,     UvHidden,  vertices, uvs);
            int nextCap     = AddVertex(nextCapPoint,      UvHidden,  vertices, uvs);

            triangles.Add(inner);
            triangles.Add(previousCap);
            triangles.Add(miterCap);

            triangles.Add(inner);
            triangles.Add(miterCap);
            triangles.Add(nextCap);
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

        // [최적화] origin 파라미터로 transform.position 캐싱 전달 — 매 레이캐스트마다 native 접근 불필요
        private ViewCastInfo ViewCast(FOVInfo fovInfo, float globalAngle, Vector3 origin)
        {
            Vector3 dir = DirFromAngle(globalAngle, true);
            if (Physics.Raycast(origin, dir, out RaycastHit hit, fovInfo.viewRadius, fovInfo._obstacleMask))
                return new ViewCastInfo { hit = true, point = hit.point, distance = hit.distance, angle = globalAngle };

            return new ViewCastInfo
            {
                hit = false,
                point = origin + dir * fovInfo.viewRadius,
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
                // [최적화] .ForEach(lambda) → foreach — 매 틱마다 delegate 객체 할당 제거 (GC alloc 감소)
                foreach (Transform item in visibleTargets)
                    before.Add(item);
                visibleTargets.Clear();
                _visibleTargetsSet.Clear();
                foreach (FOVInfo item in fovInfos)
                    FindVisibleEnemies(item);
                ClearBefore();
            }
        }

        public void SetEnable(bool val)
        {
            if (!val)
            {
                // [최적화] .ForEach(lambda) → foreach (GC 감소)
                foreach (Transform item in visibleTargets)
                    before.Add(item);
                visibleTargets.Clear();
                _visibleTargetsSet.Clear();
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

        private void FindVisibleEnemies(FOVInfo fovInfo)
        {
            // [최적화] transform.position을 루프 전 1회 캐싱 (native 접근 최소화)
            Vector3 selfPos = transform.position;

            // [최적화] OverlapSphereNonAlloc은 [0, cnt)만 채우므로 Array.Clear 불필요
            int cnt = Physics.OverlapSphereNonAlloc(selfPos, fovInfo.viewRadius, enemiesInView, fovInfo._enemyMask);

            // [최적화] FovDirection 프로퍼티, halfAngle 루프 밖으로 캐싱
            Vector3 fovDir = FovDirection;
            float halfAngle = fovInfo.viewAngle * 0.5f;

            for (int i = 0; i < cnt; i++)
            {
                Transform enemy = enemiesInView[i].transform;

                // [최적화] O(n) List.Contains → O(1) HashSet.Contains
                if (_visibleTargetsSet.Contains(enemy))
                    continue;

                Vector3 diff = enemy.position - selfPos;
                diff.y = 0f;

                float distSqr = diff.sqrMagnitude;
                if (distSqr <= 0.0001f)
                    continue;

                // [최적화] magnitude를 1회만 계산해 normalized 방향 및 레이캐스트 거리 모두 재사용
                float dist = Mathf.Sqrt(distSqr);
                Vector3 dirToEnemy = diff / dist;

                if (Vector3.Angle(fovDir, dirToEnemy) >= halfAngle)
                    continue;

                if (Physics.Raycast(selfPos, dirToEnemy, dist, fovInfo._obstacleMask))
                    continue;

                visibleTargets.Add(enemy);
                _visibleTargetsSet.Add(enemy);

                // [최적화] before.Contains() 이중 호출 제거 — 결과를 변수에 저장해 1회만 확인
                // before는 HashSet이므로 Contains/Remove 모두 O(1)
                if (!before.Contains(enemy))
                {
                    if (enemy.TryGetComponent(out IFindable findable) && ++findable.SightCount == 1)
                        findable.Founded();
                }
                else
                {
                    // 직전 프레임에도 보였고 이번에도 보임 — escaped 집합에서 제거
                    before.Remove(enemy);
                }
            }
        }
    }
}
