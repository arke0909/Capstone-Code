using System;
using System.Collections.Generic;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Effects;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace Code.ETC
{
    public class PoliceCar : MonoBehaviour
    {
        public UnityEvent OnDispatch;
        
        [Header("Reference")]
        [SerializeField] private LayerMask whatIsBullet;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private PoliceStation station;

        private bool _isDispatch = false;
        private Material _material;
        
        private void Awake()
        {
            _material = meshRenderer.material;
        }

        private void Start()
        {
            station.OnEndDispatch.AddListener(EndDispatch);
        }
        
        private void OnDestroy()
        {
            station.OnEndDispatch.RemoveListener(EndDispatch);
        }
        
        private void OnTriggerEnter(Collider collision)
        {
            if (((1 << collision.gameObject.layer) & whatIsBullet) != 0)
            {
                Dispatch();
            }
        }

        private void EndDispatch()
        {
            _material.DisableKeyword("_EMISSION");
            _material.SetColor("_EmissionColor", Color.white * 0);
            animator.enabled = false;

        }


        public void Dispatch()
        {
            if(_isDispatch) return;
            
            _isDispatch = true;
            _material.EnableKeyword("_EMISSION");
            _material.SetColor("_EmissionColor", Color.white * 10.0f);
            animator.enabled = true;
            
            OnDispatch?.Invoke();
        }
    }
}