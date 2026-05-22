using Chipmunk.GameEvents;
using Code.ItemContainers;
using Code.Items.ItemInfo;
using Code.UI.Minimap.Core;
using DewmoLib.ObjectPool.RunTime;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Work.Code.Core.Extension;

namespace Work.Code.MapEvents.Elements
{
    public class Airdrop : MonoBehaviour, IPoolable, ISpawnableStructure
    {
        [SerializeField] private float dropSpeed = 5f;
        [SerializeField] private float groundDetectSize = 1f;
        
        [SerializeField] private GameObject parachute;
        [SerializeField] private ParticleSystem fogEffect;
        [SerializeField] private LayerMask whatIsGround; 
        [SerializeField] private List<ItemDataSO> airDropItems;
        
        private bool _isDropping = false;
        private ItemContainerInventory _Inventory;

        private Pool _pool;
        
        [field: SerializeField] public PoolItemSO PoolItem { get; private set; }
        public GameObject GameObject => gameObject;
        
        private event Action<Vector3> LandingCallback;
        private string _iconId;
        private void Awake()
        {
            _Inventory = GetComponent<ItemContainerInventory>();
        }

        public void StartDrop(Vector3 position, Action<Vector3> landingCallback = null)
        {
            Spawn(position);
            SetUpContainer();
            
            LandingCallback = landingCallback;
            _isDropping = true;
            _iconId = MinimapUtil.AddToMinimap(this, ElementType.SupplyIcon, null, false, position);
        }

        public void Spawn(Vector3 targetPos)
        {
            parachute.SetActive(true);
            parachute.transform.SetLocalScale(0.7f);
            transform.position = targetPos;
        }
        public void Despawn()
        {
            fogEffect?.Stop();
            fogEffect?.Clear();
            LandingCallback = null;
            _isDropping = false;
            MinimapUtil.RemoveFromMinimap(_iconId);
            _pool.Push(this);
        }
        private void SetUpContainer()
        {
            int index = UnityEngine.Random.Range(0, airDropItems.Count);
            _Inventory.SetUpItem(airDropItems[index]);
        }

        private void Update()
        {
            if (_isDropping)
            {
                transform.position += Vector3.down * (dropSpeed * Time.deltaTime);
                CheckLanding();
            }
        }

        private void CheckLanding()
        {
            if(Physics.Raycast(transform.position + Vector3.up, Vector3.down, 
                groundDetectSize, whatIsGround))
            {
                OnLanding();
            }
        }

        private void OnLanding()
        {
            _isDropping = false;

            parachute.transform.DOScaleY(0.2f, 0.8f).OnComplete(() =>
            {
                parachute.gameObject.SetActive(false);
                fogEffect?.Play();
            });
            
            LandingCallback?.Invoke(transform.position);
            LandingCallback = null;
        }

        public void SetUpPool(Pool pool)
        {
            _pool = pool;
        }

        public void ResetItem() { }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position + Vector3.up, Vector3.down * groundDetectSize);
        }


    }
}
