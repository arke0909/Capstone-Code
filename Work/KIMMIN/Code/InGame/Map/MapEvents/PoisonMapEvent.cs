using System.Collections.Generic;
using Code.StatusEffectSystem;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Work.Code.MapEvents
{
    public class PoisonMapEvent : MapEvent
    {
        [SerializeField] private BuffSO poisonEffect;
        [Inject] private PoolManagerMono _poolManager;
        
        private List<GameObject> _maps = new();
        private Dictionary<int, BoxBuffCaster> _buffs = new();

        private int _index;
        private float _time;
        private bool _isActive;

        protected override void Awake()
        {
            base.Awake();
            
            for (int i = 0; i < transform.childCount; i++)
            {
                _maps.Add(transform.Find($"Area{i + 1}").gameObject);    
            }

            for (int i = 0; i < _maps.Count; i++)
            {
                var buff = _maps[i].GetComponentInChildren<BoxBuffCaster>();
                _buffs.Add(i, buff);
            }
        }

        private void Update()
        {
            if (_isActive && Time.time - _time >= MapEventSO.duration)
            {
                StopPoision();
            }
        }

        protected override void StartEvent()
        {
            _index = Random.Range(0, _maps.Count);
            EventName = $"{_index + 1}지역 독 활성화";
            PlayPoision();
        }

        private void PlayPoision()
        {
            var caster = _buffs[_index];
            caster.CastBuff(caster.transform.position, poisonEffect.GetStatusEffectInfo());
            _isActive = true;
            _time = Time.time;
        }

        private void StopPoision()
        {
            _isActive = false;
        }
    }
}