using System;
using System.Collections.Generic;
using Chipmunk.ComponentContainers;
using DewmoLib.Dependencies;
using Scripts.Players;
using UnityEngine;
using Work.Code.Core;

namespace Work.Code.Entities.Experiencement
{
    public class ExpManager : MonoSingleton<ExpManager>
    {
        [SerializeField] private float magneticSpeed;
        [SerializeField] private float collectDistance;
        [SerializeField] private float expPerOrb;
        
        [Inject] private Player _player;
        
        private readonly List<ExpOrb> _orbs = new();
        private readonly Queue<ExpOrb> _deleteQueue = new();
        private ExpCompo _expCompo;

        private void Start()
        {
            _expCompo = _player.Get<ExpCompo>();
        }

        public void RegisterOrb(ExpOrb orb)
        {
            orb.SubscribeOnCollect(HandleCollect);
            _orbs.Add(orb);
        }

        private void HandleCollect()
        {
            _expCompo.CurrentValue += expPerOrb;
        }

        private void Update()
        {
            foreach (var orb in _orbs)
            {
                if(!orb.CanCollect) continue;
                
                orb.transform.position = Vector3.Lerp(orb.transform.position, 
                    _player.transform.position, Time.deltaTime * magneticSpeed);
                
                float distance = (_player.transform.position - orb.transform.position).magnitude;
                if (distance < collectDistance)
                {
                    orb.CollectingOrb();
                    _deleteQueue.Enqueue(orb);
                }
            }

            if (_deleteQueue.Count > 0)
            {
                foreach (var orb in _deleteQueue)
                {
                    _orbs.Remove(orb);
                }
                
                _deleteQueue.Clear();
            }
        }
    }
}