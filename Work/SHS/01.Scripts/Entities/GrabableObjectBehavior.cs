using UnityEngine;

namespace SHS.Scripts.Entities.Players
{
    public class GrabableObjectBehavior : MonoBehaviour, IGrabableObject
    {
        [field: SerializeField] public Transform LeftGrabPoint { get; private set; }
        [field: SerializeField] public Transform RightGrabPoint { get; private set; }
    }
}