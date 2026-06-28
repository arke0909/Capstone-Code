using System.Collections.Generic;
using UnityEngine;

namespace Code.ETC.Sound
{
    [RequireComponent(typeof(Collider))]
    public class BGMChangeZone : MonoBehaviour
    {
        [SerializeField] private BGMPlayer targetBGMPlayer;
        [SerializeField] private LayerMask targetLayers = ~0;
        [SerializeField] private bool replayOnReEnter = true;

        private readonly Dictionary<int, int> _occupantCounts = new();
        private bool _hasPlayed;

        private void Reset()
        {
            TryGetComponent(out targetBGMPlayer);
            EnsureTriggerCollider();
        }

        private void Awake()
        {
            if (targetBGMPlayer == null)
                TryGetComponent(out targetBGMPlayer);
        }

        private void OnValidate()
        {
            EnsureTriggerCollider();
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleEnter(ResolveTarget(other));
        }

        private void OnTriggerExit(Collider other)
        {
            HandleExit(ResolveTarget(other));
        }

        private void HandleEnter(GameObject target)
        {
            if (target == null)
                return;

            int key = target.GetInstanceID();
            _occupantCounts.TryGetValue(key, out int count);
            _occupantCounts[key] = count + 1;

            if (count > 0)
                return;

            if (!_hasPlayed || replayOnReEnter)
            {
                targetBGMPlayer?.PlayBGM();
                _hasPlayed = true;
            }
        }

        private void HandleExit(GameObject target)
        {
            if (target == null)
                return;

            int key = target.GetInstanceID();
            if (!_occupantCounts.TryGetValue(key, out int count))
                return;

            if (count <= 1)
                _occupantCounts.Remove(key);
            else
                _occupantCounts[key] = count - 1;
        }

        private GameObject ResolveTarget(Collider other)
        {
            GameObject target = other.attachedRigidbody != null
                ? other.attachedRigidbody.gameObject
                : other.transform.root.gameObject;

            return IsValidTarget(target) ? target : null;
        }

        private bool IsValidTarget(GameObject target)
        {
            if (((1 << target.layer) & targetLayers.value) == 0)
                return false;

            return true;
        }

        private void EnsureTriggerCollider()
        {
            if (TryGetComponent(out Collider zoneCollider))
                zoneCollider.isTrigger = true;
        }
    }
}
