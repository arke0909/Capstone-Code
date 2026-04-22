using UnityEngine;

namespace SHS.Scripts.Entities.Players
{
    public interface IGrabableObject
    {
        public Transform LeftGrabPoint { get; }
        public Transform RightGrabPoint { get; }
    }
}