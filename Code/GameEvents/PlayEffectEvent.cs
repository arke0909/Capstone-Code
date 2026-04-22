using Chipmunk.GameEvents;
using DewmoLib.ObjectPool.RunTime;
using UnityEngine;

namespace Code.GameEvents
{
    public struct PlayEffectEvent : IEvent
    {
        public PoolItemSO PoolItemSO { get; private set; }
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
        public float Scale { get; private set; }
        
        public PlayEffectEvent(PoolItemSO poolItemSO, Vector3 position, Quaternion rotation, float scale = 1)
        {
            PoolItemSO = poolItemSO;
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }
}