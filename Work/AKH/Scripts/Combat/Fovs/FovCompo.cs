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
        private int _lastRenderPoseFrame = -1;

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

        private void HandleCameraPreCull(Camera camera)
        {
            HandleCameraPreRender(camera);
        }

        private void HandleBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            HandleCameraPreRender(camera);
        }

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

            Vector3 direction = GetCurrentAimPosition() - transform.position;
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : GetFallbackDirection();
        }

        private Vector3 GetCurrentAimPosition()
            => aimProvider.GetAimPosition();

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

            int resolveIterations = Mathf.Max(_edgeResolveIterations, MinEdgeResolveIterations);
            for (int i = 0; i < resolveIterations; i++)
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

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            // uv.x stores visibility for the render texture mask: 1 inside FOV, 0 on the soft outer band.
            int centerIndex = AddVertex(Vector3.zero, new Vector2(1f, 0f), vertices, uvs);
            List<int> boundaryIndices = new List<int>(localBoundaryPoints.Length);
            for (int i = 0; i < localBoundaryPoints.Length; i++)
                boundaryIndices.Add(AddVertex(localBoundaryPoints[i].point, new Vector2(1f, 0f), vertices, uvs));

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

            List<Vector3> innerPoints = new List<Vector3>(polygonPointCount);
            List<float> widthScales = new List<float>(polygonPointCount);
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

            List<Vector3> edgeOutwards = BuildEdgeOutwards(innerPoints, signedArea);
            if (edgeOutwards.Count != polygonPointCount)
                return;

            List<int> innerIndices = new List<int>(polygonPointCount);
            List<int> outerIndices = new List<int>(polygonPointCount);
            for (int i = 0; i < polygonPointCount; i++)
            {
                Vector3 outward = GetMiterOutward(edgeOutwards, i, out float miterScale);
                float inset = Mathf.Max(_edgeSoftnessWorld * Mathf.Max(widthScales[i], 0.01f), 0.01f);
                Vector3 outerPoint = innerPoints[i] + outward * inset * miterScale;

                innerIndices.Add(AddVertex(innerPoints[i], new Vector2(1f, 0f), vertices, uvs));
                outerIndices.Add(AddVertex(outerPoint, new Vector2(0f, 0f), vertices, uvs));
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
            for (int i = 0; i < points.Count; i++)
            {
                Vector3 current = points[i];
                Vector3 next = points[(i + 1) % points.Count];
                area += current.x * next.z - next.x * current.z;
            }

            return area * 0.5f;
        }

        private static List<Vector3> BuildEdgeOutwards(List<Vector3> points, float signedArea)
        {
            List<Vector3> outwards = new List<Vector3>(points.Count);
            float outwardSign = signedArea >= 0f ? 1f : -1f;
            for (int i = 0; i < points.Count; i++)
            {
                Vector3 edge = points[(i + 1) % points.Count] - points[i];
                if (edge.sqrMagnitude <= 0.000001f)
                {
                    outwards.Add(Vector3.zero);
                    continue;
                }

                outwards.Add(Vector3.Cross(Vector3.up, edge).normalized * outwardSign);
            }

            return outwards;
        }

        private static Vector3 GetMiterOutward(List<Vector3> edgeOutwards, int index, out float miterScale)
        {
            Vector3 previousOutward = edgeOutwards[(index - 1 + edgeOutwards.Count) % edgeOutwards.Count];
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
            Vector3 previousOutward = edgeOutwards[(pointIndex - 1 + edgeOutwards.Count) % edgeOutwards.Count];
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

            int inner = AddVertex(innerPoint, new Vector2(1f, 0f), vertices, uvs);
            int previousCap = AddVertex(previousCapPoint, new Vector2(0f, 0f), vertices, uvs);
            int miterCap = AddVertex(miterCapPoint, new Vector2(0f, 0f), vertices, uvs);
            int nextCap = AddVertex(nextCapPoint, new Vector2(0f, 0f), vertices, uvs);

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
