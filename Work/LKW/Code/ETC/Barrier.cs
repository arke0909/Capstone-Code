using System;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;

namespace Code.ETC
{
    public class Barrier : MonoBehaviour
    {
        [SerializeField] private LayerMask whatIsBullet;
        
        private Animator _animator;
        private bool _isBreak = false;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnTriggerEnter(Collider collision)
        {
            if (_isBreak == false && ((1 << collision.gameObject.layer) & whatIsBullet) != 0)
            {
                _isBreak = true;
                _animator.SetTrigger("BREAK");
            }
        }

        private void DestroySelf()
        {
            Destroy(gameObject);
        }
    }
}