using Chipmunk.GameEvents;
using Code.Events;
using Unity.Cinemachine;
using UnityEngine;

namespace Code.ETC.CameraZone
{
    public class CameraZoneTrigger : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera enterCam;
        [SerializeField] private CinemachineCamera exitCam;
        [SerializeField] private float blendTime = 1.5f;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            Debug.Log("Enter");
            Bus.Raise(new CameraSwitchEvent(new CameraSwitchData(enterCam, blendTime)));
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            Debug.Log("Exit");
            Bus.Raise(new CameraSwitchEvent(new CameraSwitchData(exitCam, blendTime)));
        }
        
    }
}