using System;
using System.Collections.Generic;
using Chipmunk.GameEvents;
using DewmoLib.ObjectPool.RunTime;
using DG.Tweening;
using UnityEngine;
using Work.Code.Core.Extension;
using Work.LKW.Code.ItemContainers;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.MapEvents.Elements
{
    public class Airdrop : MonoBehaviour, IPoolable
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

        private void Awake()
        {
            _Inventory = GetComponent<ItemContainerInventory>();
        }

        public void StartDrop(Vector3 position, float height, Action<Vector3> landingCallback = null)
        {
            InitAirdrop(position, height);
            SetUpContainer();
            
            LandingCallback = landingCallback;
            _isDropping = true;
        }
        
        private void InitAirdrop(Vector3 position, float height)
        {
            parachute.SetActive(true);
            parachute.transform.SetLocalScale(0.7f);
            transform.position = new Vector3(position.x, height, position.z);
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

        public void TakeAirdrop()
        {
            fogEffect?.Stop();
            fogEffect?.Clear();
            
            _pool.Push(this);
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