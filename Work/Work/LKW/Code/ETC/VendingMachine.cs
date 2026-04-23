using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using DG.Tweening;
using Scripts.Effects;
using Unity.Mathematics;
using UnityEngine;
using Work.LKW.Code.Items;
using Work.LKW.Code.Items.ItemInfo;
using Random = UnityEngine.Random;

namespace Code.ETC
{
    public class VendingMachine : MonoBehaviour
    {
        [Inject]
        private PoolManagerMono _poolManagerMono;
        
        [Header("Reference")]
        [SerializeField] private PoolItemSO explosiveItem;
        [SerializeField] private LayerMask whatIsBullet;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private ItemDataBaseSO itemDB;
        [SerializeField] private PoolItemSO previewItem;
        [SerializeField] private Transform discardPoint;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float offDuration = 1f;
        [SerializeField] private float discardRange = 0.5f;
        
        [SerializeField] private List<ItemType> allowedTypes;

        private bool _isOff = false;
        private int _spawnCount = 0;
        private int _count = 0;
        private readonly int MAX_SPAWNCOUNT = 5;

        private Material _material;
        
        private void Awake()
        {
            _material = meshRenderer.material;
        }

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            _material.EnableKeyword("_EMISSION");
            _material.SetColor("_EmissionColor", Color.white * 10.0f);
            _spawnCount = Random.Range(1, MAX_SPAWNCOUNT);
            _count = 0;
            _isOff = false;
        }

        private void OnTriggerEnter(Collider collision)
        {
            if (((1 << collision.gameObject.layer) & whatIsBullet) != 0)
            {
                TakeHit();
            }
        }

        public void TakeHit()
        {
            if (_isOff) return;

            if (_count > 0)
            {
                float explodeChance = (float)_count / MAX_SPAWNCOUNT;
                if (Random.value < explodeChance)
                {
                    Explode();
                    return;
                }
            }

            if (_count < _spawnCount)
            {
                SpawnItem();
                _count++;
            }
            else if (_count == _spawnCount)
            {
                SpawnItem();
                _isOff = true;
                StartCoroutine(MachineOffCoroutine(offDuration));
            }
        }

        private void Explode()
        {
            _isOff = true;
            Debug.Log("Vending Machine Exploded!");
            StartCoroutine(MachineOffCoroutine(offDuration));
            
            PoolingEffect effect = _poolManagerMono.Pop<PoolingEffect>(explosiveItem);
            if (effect != null) effect.PlayVFX(transform.position, quaternion.identity);
        }

        private void SpawnItem()
        {
            if (allowedTypes == null || allowedTypes.Count == 0) return;

            ItemType type = allowedTypes[Random.Range(0, allowedTypes.Count)];
            var targetItemData = itemDB.GetRandomItems(type, 1).FirstOrDefault();
            
            if (targetItemData == null) return;

            ItemCreateData createData = targetItemData.CreateItem();
            var spawnPreviewItem = _poolManagerMono.Pop<PreviewItem>(previewItem);

            Vector3 discardPos = discardPoint.position;
            discardPos.x += Random.Range(-discardRange, discardRange);
            discardPos.z += Random.Range(-discardRange, discardRange);
            discardPos.y += 0.2f;

            spawnPreviewItem.Discard(spawnPoint.position, createData.Item, createData.Stack);
            spawnPreviewItem.transform.DOMove(discardPos, 0.25f).SetEase(Ease.InCubic);
        }
        
        private IEnumerator MachineOffCoroutine(float duration)
        {
            Color startColor = _material.GetColor("_EmissionColor");
            Color endColor = Color.white * 0f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                Color currentColor = Color.Lerp(startColor, endColor, t);
                _material.SetColor("_EmissionColor", currentColor);
                yield return null;
            }

            _material.SetColor("_EmissionColor", endColor);
            _material.DisableKeyword("_EMISSION");
        }
    }
}