using Chipmunk.ComponentContainers;
using Scripts.Entities;
using UnityEngine;

namespace SHS.Scripts.NoiseSystems
{
    public struct NoiseData
    {
        public Entity Source { get; }
        public Vector3 Position { get; }
        public float Radius { get; }

        public NoiseData(Entity source, Vector3 position, float radius)
        {
            Source = source;
            Position = position;
            Radius = radius;
        }
    }

    public class NoiseGenerator : MonoBehaviour
    {
        [SerializeField] private LayerMask _layerMask;
        private Collider[] _colliders = new Collider[50];

        public void GenerateNoise(Entity entity, float radius)
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, radius, _colliders, _layerMask);
            for (int i = 0; i < count; i++)
            {
                if (_colliders[i].TryGetComponent(out Entity target) &&
                    target.TryGetSubclassComponent(out INoiseListener noiseReceiver))
                {
                    NoiseData noiseData = new NoiseData(entity, transform.position, radius);
                    noiseReceiver.OnNoiseListen(noiseData);
                }
            }
        }
    }
}