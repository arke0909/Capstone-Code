using Unity.Cinemachine;

namespace Code.ETC.CameraZone
{
    public struct CameraSwitchData
    {
        public CinemachineCamera TargetCamera { get;}
        public float BlendTime { get; }

        public CameraSwitchData(CinemachineCamera targetCamera, float blendTime)
        {
            TargetCamera = targetCamera;
            BlendTime = blendTime;
        }
    }
}