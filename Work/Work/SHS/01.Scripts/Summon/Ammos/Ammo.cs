using System;
using Chipmunk.ComponentContainers;
using Scripts.Entities;
using UnityEngine;

namespace SHS.Scripts.Summon.Ammos
{
    public class Ammo : MonoBehaviour, ISummonable
    {
        [SerializeField] private float radius = 0.5f;
        [SerializeField] private LayerMask layerMask;
        private Collider[] colliders = new Collider[10];

        public void Awake()
        {
        }

        private void FixedUpdate()
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, radius, colliders, layerMask);
            for (int i = 0; i < count; i++)
            {
                if (colliders[i].TryGetComponent(out ComponentContainer componentContainer))
                {
                    // 총알 무한 버프를 만들고 버프 주기.
                }
            }
        }
    }
}