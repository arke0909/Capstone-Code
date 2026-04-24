using Code.StatusEffectSystem;
using DewmoLib.ObjectPool.RunTime;
using UnityEngine;
using System.Collections.Generic;
using Chipmunk.ComponentContainers;
using Scripts.Entities;
using Scripts.Players;

namespace Code.SkillSystem.Skills.Drones
{
    public struct Receiver : System.IEquatable<Receiver>
    {
        public EntityStatusEffect StatusEffect;
        public CharacterMovement Movement;

        public bool Equals(Receiver other)
        {
            return StatusEffect == other.StatusEffect && Movement == other.Movement;
        }

        public override int GetHashCode() => (StatusEffect, Movement).GetHashCode();
    }
    public class SpeedNode : MonoBehaviour, IPoolable
    {
        [SerializeField] private PoolItemSO speedNodeItem;
        [SerializeField] private BuffSO speedBuffData; 
        
        [Header("Settings")]
        [Range(0, 1f)] 
        [SerializeField] private float lookThreshold = 0.5f;

        private Pool _myPool;
        private List<Receiver> _receiversInRange = new List<Receiver>();

        public PoolItemSO PoolItem => speedNodeItem;
        public GameObject GameObject => gameObject;

        private void Update()
        {
            if (_receiversInRange.Count == 0) return;

            Vector3 nodeForward = transform.forward;

            for (int i = _receiversInRange.Count - 1; i >= 0; i--)
            {
                var receiver = _receiversInRange[i];
        
                if (receiver.StatusEffect == null || receiver.Movement == null || !receiver.StatusEffect.gameObject.activeInHierarchy)
                {
                    _receiversInRange.RemoveAt(i);
                    continue;
                }

                Vector3 moveDirection = receiver.Movement.Direction; 
                float dot = Vector3.Dot(nodeForward, moveDirection.normalized);

                if (dot >= lookThreshold)
                {
                    receiver.StatusEffect.AddStatusEffect(speedBuffData.GetStatusEffectInfo());
                }
            }
        }

        

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Entity entity))
            {
                if (entity.TryGet(out EntityStatusEffect statusEffect) && entity.TryGet(out CharacterMovement movement))
                {
                    if (!_receiversInRange.Exists(r => r.StatusEffect == statusEffect))
                    {
                        _receiversInRange.Add(new Receiver { StatusEffect = statusEffect, Movement = movement });
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out Entity entity))
            {
                if (entity.TryGet(out EntityStatusEffect statusEffect))
                {
                    _receiversInRange.RemoveAll(r => r.StatusEffect == statusEffect);
                }
            }
        }

        private void ForceReleaseAll()
        {
            for (int i = _receiversInRange.Count - 1; i >= 0; i--)
            {
                var receiver = _receiversInRange[i];
                if (receiver.StatusEffect != null)
                {
                    receiver.StatusEffect.RemoveStatusEffect(speedBuffData);
                }
            }
            _receiversInRange.Clear();
        }
        
        private void OnDisable()
        {
            ForceReleaseAll();
        }

        public void SetUpPool(Pool pool) => _myPool = pool;

        public void ResetItem()
        {
            _receiversInRange.Clear();
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        public void ReturnToPool() => _myPool.Push(this);
    }
}