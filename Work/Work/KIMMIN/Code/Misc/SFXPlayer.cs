using System;
using Ami.BroAudio;
using UnityEngine;

namespace Work.Code.Misc
{
    public class SFXPlayer : MonoBehaviour
    {
        public SoundID soundID;

        private void Awake()
        {
            BroAudio.Play(soundID, transform.position);
        }
    }
}