using Ami.BroAudio;
using UnityEngine;

namespace Work.Code.Entities
{
    public class FootstepSFX : MonoBehaviour
    {
        [SerializeField] private SoundID footstepSoundID;

        public void PlayFootstepSound()
        {
            BroAudio.Play(footstepSoundID, transform.position);
        }
    }
}