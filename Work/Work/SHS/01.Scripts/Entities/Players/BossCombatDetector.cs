using System;
using Chipmunk.GameEvents;
using Code.SHS.Entities.Enemies;
using Code.SHS.Entities.Enemies.Events;
using UnityEngine;

namespace SHS.Scripts.Entities.Players
{
    public class BossCombatDetector : MonoBehaviour
    {
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private float detectionRadius = 15f;

        private Boss CurrentBoss
        {
            get => currentBoss;
            set
            {
                if (currentBoss == value)
                    return;
                currentBoss = value;
                EventBus<BossCombatEnteredEvent>.OnEvent?.Invoke(new BossCombatEnteredEvent(currentBoss));
            }
        }

        private Boss currentBoss;
        private Collider[] detectedTargets = new Collider[15];

        private void FixedUpdate()
        {
            if (CurrentBoss != null &&
                Vector3.Distance(currentBoss.transform.position, transform.position) > detectionRadius)
                CurrentBoss = null;
            DetectTargets();
        }

        private void DetectTargets()
        {
            int targetCount =
                Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, detectedTargets, targetLayer);
            float closestDistance = Mathf.Infinity;
            Boss closestTarget = null;
            for (int i = 0; i < targetCount; i++)
            {
                Collider targetCollider = detectedTargets[i];
                Boss target = targetCollider.GetComponent<Boss>();
                if (target == null)
                    continue;
                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance < closestDistance && target != null)
                {
                    closestDistance = distance;
                    closestTarget = target;
                }
            }

            CurrentBoss = closestTarget;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}