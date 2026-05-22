using System.Collections.Generic;
using Ami.BroAudio;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Effects;
using Scripts.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.ETC.MapObjects
{
    public class Explosive : HittableObject
    {
        private const float SmokeEffectHpThreshold = 0.4f;
        private const float DestroyDelay = 0.04f;

        [Header("Reference")]
        [SerializeField] private LayerMask whatIsBullet;
        [SerializeField] private PoolItemSO explosiveItem;
        [SerializeField] private PoolManagerSO poolManager;
        [SerializeField] private OverlapDamageCaster damageCaster;
        [SerializeField] private MovementDataSO movementData;
        [SerializeField] private SoundID explosiveSoundID;
        [FormerlySerializedAs("smokeEffects")] [SerializeField] private GameObject smokeEffect;

        [Header("Setting")]
        [SerializeField] private float damage;
        [SerializeField] private List<Explosive> nearbyExplosives;

        private DamageData _damageData;
        private bool _isExplosion = false;

        private void Start()
        {
            if(damageCaster != null)
                damageCaster.InitCaster(null);

            _damageData = new DamageData
            {
                damage = damage,
                damageType = DamageType.MAGIC,
                defPierceLevel = 1
            };
        }


        protected override void OnDeath()
        {
            Explode();
        }

        public void Explode()
        {
            if(_isExplosion) return;

            _isExplosion = true;
            damageCaster.CastDamage(_damageData, transform.position, Vector3.forward, movementData);

            PoolingEffect effect = poolManager.Pop(explosiveItem) as PoolingEffect;
            if (effect != null) effect.PlayVFX(transform.position, quaternion.identity);

            foreach (Explosive exp in nearbyExplosives)
            {
                exp.Kill();
            }

            BroAudio.Play(explosiveSoundID, transform.position);
            Destroy(gameObject, DestroyDelay);
        }

        public override void ApplyDamage(DamageData damageData, Entity dealer = null)
        {
            base.ApplyDamage(damageData, dealer);

            if (CurrentHp <= MaxHp * SmokeEffectHpThreshold)
            {
                smokeEffect.SetActive(true);
            }
        }
    }
}