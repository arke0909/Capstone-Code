using System;
using DewmoLib.ObjectPool.RunTime;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Work.Code.UI
{
    public class DamageText : MonoBehaviour, IPoolable
    {
        [SerializeField] private TextMeshPro damageText;
        [SerializeField] private float animDuration = 0.5f;
        [SerializeField] private float animScale = 1f;
        
        private Camera _cam;
        private Pool _pool;
        
        [field: SerializeField] public PoolItemSO PoolItem { get; private set; }
        public GameObject GameObject => gameObject;
        
        public void InitText(float damage, Vector3 position)
        {
            damageText.text = Mathf.Round(damage).ToString();
            transform.position = position + Vector3.up;
            PlayAnim();
        }
        
        private void PlayAnim()
        {
            transform.DOKill();

            float angle = Random.Range(-30f, 30f);
            Vector3 up = _cam.transform.up;
            Vector3 right = _cam.transform.right;
            Vector3 dir = (up + right * Mathf.Tan(angle * Mathf.Deg2Rad)).normalized;
            Vector3 targetPos = transform.position + dir * animScale;

            transform.rotation = Quaternion.LookRotation(_cam.transform.forward);
            transform.localScale = Vector3.zero;

            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
            seq.Join(transform.DOMove(targetPos, animDuration).SetEase(Ease.OutExpo));
            seq.Append(transform.DOScale(0f, 0.25f).SetEase(Ease.InBack));
            seq.OnComplete(() => _pool.Push(this));
        }

        private void LateUpdate()
        {
            if (_cam == null) return;
            transform.forward = _cam.transform.forward;
        }

        public void SetUpPool(Pool pool)
        {
            _pool = pool;
            _cam = Camera.main;
            
            var renderer = damageText.GetComponent<MeshRenderer>();
            renderer.sortingLayerName = "UI";
            renderer.sortingOrder = 999;
        }

        public void ResetItem() { }
    }
}