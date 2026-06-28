using Chipmunk.GameEvents;
using Code.AirDrop;
using Code.ItemContainers;
using Code.TimeSystem;
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
        [SerializeField] private SupplyRewardTableSO supplyRewardTable;
        [SerializeField] private bool logRewardGeneration = true;
        
        private bool _isDropping = false;
        private readonly SupplyRewardGenerator _rewardGenerator = new();

        private Pool _pool;
        
        [field: SerializeField] public PoolItemSO PoolItem { get; private set; }
        public ItemContainerInventory Inventory { get; private set; }
        public GameObject GameObject => gameObject;
        
        private event Action<Vector3> LandingCallback;
        private string _iconId;
        private void Awake()
        {
            Inventory = GetComponent<ItemContainerInventory>();
        }

        public void StartDrop(Vector3 position, Action<Vector3> landingCallback = null)
        {
            SetUpContainer();
            Spawn(position);
            
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
            if (supplyRewardTable == null)
            {
                Debug.LogWarning($"[{nameof(Airdrop)}] SupplyRewardTableSO is missing.", this);
                Inventory.ClearInventory();
                return;
            }

            int currentDay = TimeController.Instance != null ? Mathf.Max(1, TimeController.Instance.CurrentDay) : 1;
            List<SupplyReward> rewards = _rewardGenerator.Generate(currentDay, supplyRewardTable);

            if (rewards.Count == 0)
            {
                Debug.LogWarning($"[{nameof(Airdrop)}] No supply rewards generated for day {currentDay}.", this);
                Inventory.ClearInventory();
                return;
            }

            if (logRewardGeneration)
            {
                foreach (SupplyReward reward in rewards)
                {
                    string itemName = reward.ItemData != null ? reward.ItemData.itemName : "null";
                    Debug.Log($"[{nameof(Airdrop)}] Reward generated: {itemName} x{reward.Stack}", this);
                }
            }

            Inventory.SetUpRewards(rewards);
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
