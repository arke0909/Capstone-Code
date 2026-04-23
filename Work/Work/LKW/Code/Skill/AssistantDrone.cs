using System;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat.Projectiles;
using UnityEngine;

namespace Work.LKW.Skill
{
    public class AssistantDrone : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private LayerMask whatIsEnemy;
        [SerializeField] private float detactRadius = 5f;

        [Header("Attack")] 
        [SerializeField] private float attackCoolDown;
        
        private float _lastAttackTime = 0f;

        [SerializeField] private PoolManagerSO poolManager;
        [SerializeField] private PoolItemSO projectileItem;
        
        private Transform _target = null;
        
        private void Update()
        {
            TryAttack();
        }

        private void TryAttack()
        {
            if(_target == null) return;
            ;
            if(Time.time > _lastAttackTime + attackCoolDown)
            {
                Attack();
                _lastAttackTime = Time.time;
            }
        }

        private void Attack()
        {
            Bullet bullet = poolManager.Pop(projectileItem) as Bullet;
            //bullet.InitBullet();
        }


        public void DetectEnemy()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, detactRadius, whatIsEnemy);

            if (hits.Length > 0)
            {
                _target = hits[0].transform;
            }
        }
        
    }
}