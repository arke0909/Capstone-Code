using Ami.BroAudio;
using Chipmunk.Library.Utility.GameEvents.Local;
using SHS.Scripts.Effects;
using UnityEngine;

namespace Work.Code.Entities
{
    public class FootstepSFX : MonoBehaviour, ILocalEventSubscriber<FootstepEvent>
    {
        [SerializeField] private SoundID footstepSoundID;

        public void OnLocalEvent(FootstepEvent eventData)
        {
            BroAudio.Play(footstepSoundID, eventData.Position);
        }
    }
}
