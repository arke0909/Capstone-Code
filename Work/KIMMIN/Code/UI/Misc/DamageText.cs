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
            damageText.alpha = 1f;
            transform.position = position + Vector3.up * 1.2f
                                          + _cam.transform.right * Random.Range(-0.2f, 0.2f);
            PlayAnim();
        }

        private void PlayAnim()
        {
            transform.DOKill();

            float angle = Random.Range(-30f, 30f);
            Vector3 up = _cam.transform.up;
            Vector3 right = _cam.transform.right;
            Vector3 dir = (up + right * Mathf.Tan(angle * Mathf.Deg2Rad)).normalized;
            Vector3 targetPos = transform.position + dir * ((Random.value + 1) * 0.3f);

            transform.rotation = Quaternion.LookRotation(_cam.transform.forward, _cam.transform.up);
            transform.localScale = Vector3.one * 2.8f;

            transform.DOPunchRotation(
                Vector3.forward * (Random.Range(-12f, 12f) * 5), 0.15f, 3, 0.6f).SetEase(Ease.OutBack);
            Sequence moveSequence = DOTween.Sequence();
            moveSequence.Append(transform.DOMove(targetPos, animDuration).SetEase(Ease.OutQuad));
            moveSequence.Play();
            
            Sequence scaleSequence = DOTween.Sequence();
            scaleSequence.AppendInterval(0.03f);
            scaleSequence.Append(transform.DOScale(0.9f, 0.3f).SetEase(Ease.OutExpo));
            scaleSequence.Append(transform.DOScale(1f, 0.06f).SetEase(Ease.OutQuad));
            scaleSequence.AppendInterval(0.04f);
            scaleSequence.Join(damageText.DOFade(0f, animDuration).SetEase(Ease.InQuad));
            scaleSequence.OnComplete(() => _pool.Push(this));
            scaleSequence.Play();
        }

        public void SetUpPool(Pool pool)
        {
            _pool = pool;
            _cam = Camera.main;

            var renderer = damageText.GetComponent<MeshRenderer>();
            renderer.sortingLayerName = "UI";
            renderer.sortingOrder = 999;
        }

        public void ResetItem()
        {
        }
    }
}