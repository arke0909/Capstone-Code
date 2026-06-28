using DewmoLib.ObjectPool.RunTime;
using UnityEngine;

namespace Work.Code.Misc
{
    [RequireComponent(typeof(LineRenderer))]
    public class ArrowLine : MonoBehaviour, IPoolable
    {
        private const float MinTileSize = 0.01f;
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");

        [SerializeField] private GameObject targetA;
        [SerializeField] private GameObject targetB;
        [SerializeField] private Sprite arrowSprite;
        [SerializeField] private float tileSize = 1.25f;
        [SerializeField] private float moveSpeed = 2f;

        private LineRenderer _lineRenderer;
        private Transform _targetATransform;
        private Transform _targetBTransform;
        private Vector3 _targetAOffset;
        private Vector3 _targetBOffset;
        private Material _material;
        private Pool _pool;
        private float _textureOffset;
        private bool _isReleased;

        [field: SerializeField] public PoolItemSO PoolItem { get; private set; }
        public GameObject GameObject => gameObject;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _material = _lineRenderer.material;
            ApplyTexture();
            SetTarget(targetA, targetB);
        }

        private void OnEnable()
        {
            _isReleased = false;
        }

        private void LateUpdate()
        {
            _textureOffset += Time.deltaTime * moveSpeed;
            RefreshLine();
        }

        public void SetTarget(GameObject targetA, GameObject targetB)
        {
            this.targetA = targetA;
            this.targetB = targetB;
            _targetATransform = targetA ? targetA.transform : null;
            _targetBTransform = targetB ? targetB.transform : null;
            RefreshLine();
        }

        public void SetTarget(Transform targetA, Transform targetB)
        {
            this.targetA = targetA ? targetA.gameObject : null;
            this.targetB = targetB ? targetB.gameObject : null;
            _targetATransform = targetA;
            _targetBTransform = targetB;
            RefreshLine();
        }

        public void SetTargetA(GameObject target)
        {
            targetA = target;
            _targetATransform = target ? target.transform : null;
            RefreshLine();
        }

        public void SetTargetB(GameObject target)
        {
            targetB = target;
            _targetBTransform = target ? target.transform : null;
            RefreshLine();
        }

        public void SetArrowSprite(Sprite sprite)
        {
            arrowSprite = sprite;
            ApplyTexture();
            RefreshLine();
        }

        public void SetOffset(Vector3 targetAOffset, Vector3 targetBOffset)
        {
            _targetAOffset = targetAOffset;
            _targetBOffset = targetBOffset;
            RefreshLine();
        }

        public void ClearTarget()
        {
            targetA = null;
            targetB = null;
            _targetATransform = null;
            _targetBTransform = null;
            _targetAOffset = Vector3.zero;
            _targetBOffset = Vector3.zero;
            ChangeLineActive(false);
        }

        public void ReturnToPool()
        {
            if (_isReleased)
                return;

            _isReleased = true;
            ClearTarget();

            if (_pool != null)
                _pool.Push(this);
            else
                gameObject.SetActive(false);
        }

        private void RefreshLine()
        {
            if (!_targetATransform || !_targetBTransform || arrowSprite == null)
            {
                ChangeLineActive(false);
                return;
            }

            Vector3 startPosition = _targetATransform.position + _targetAOffset;
            Vector3 endPosition = _targetBTransform.position + _targetBOffset;
            float distance = Vector3.Distance(startPosition, endPosition);

            if (distance <= Mathf.Epsilon)
            {
                ChangeLineActive(false);
                return;
            }

            ChangeLineActive(true);
            _lineRenderer.SetPosition(0, startPosition);
            _lineRenderer.SetPosition(1, endPosition);
            RefreshTexture(distance);
        }

        private void ApplyTexture()
        {
            if (arrowSprite == null)
                return;

            arrowSprite.texture.wrapMode = TextureWrapMode.Repeat;
            _material.SetTexture(MainTex, arrowSprite.texture);
        }

        private void RefreshTexture(float distance)
        {
            float size = Mathf.Max(MinTileSize, tileSize);
            _material.SetTextureScale(MainTex, new Vector2(distance / size, 1f));
            _material.SetTextureOffset(MainTex, new Vector2(-_textureOffset / size, 0f));
        }

        private void ChangeLineActive(bool isActive)
        {
            _lineRenderer.enabled = isActive;
        }

        public void SetUpPool(Pool pool)
        {
            _pool = pool;
        }

        public void ResetItem()
        {
            _isReleased = false;
            _textureOffset = 0f;
            ClearTarget();
        }
    }
}
