using Ami.BroAudio;
using UnityEngine;

namespace Code.ETC.Sound
{
    public class BGMPlayer : MonoBehaviour
    {
        private static IAudioPlayer _currentBGMPlayer;
        private static bool _isTrackingInitialized;

        [SerializeField] private SoundID bgmID;
        [SerializeField] private float fadeTime = 2f;
        [SerializeField] private bool playOnStart = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetTracking()
        {
            _currentBGMPlayer = null;
            _isTrackingInitialized = false;
        }

        private static void EnsureTrackingInitialized()
        {
            if (_isTrackingInitialized)
                return;

            BroAudio.OnBGMChanged += HandleBGMChanged;
            _isTrackingInitialized = true;
        }

        private static void HandleBGMChanged(IAudioPlayer player)
        {
            _currentBGMPlayer = player;
        }

        private static bool TryGetCurrentBGMPlayer(out IAudioPlayer player)
        {
            if (_currentBGMPlayer is not Object unityObject || unityObject == null)
            {
                _currentBGMPlayer = null;
                player = null;
                return false;
            }

            player = _currentBGMPlayer;
            return true;
        }

        private void Awake()
        {
            EnsureTrackingInitialized();
        }

        private void Start()
        {
            if (playOnStart)
                PlayBGM();
        }

        private void OnDestroy()
        {
            StopBGM();
        }

        public void PlayBGM()
        {
            EnsureTrackingInitialized();

            if (!bgmID.IsValid())
                return;

            if (TryGetCurrentBGMPlayer(out IAudioPlayer currentPlayer) &&
                currentPlayer.IsPlaying &&
                currentPlayer.ID.Equals(bgmID))
            {
                return;
            }

            BroAudio.Play(bgmID)
                .AsBGM()
                .SetTransition(Transition.CrossFade, fadeTime);
        }

        public void StopBGM()
        {
            if (bgmID.IsValid())
            {
                BroAudio.Stop(bgmID, fadeTime);

                if (TryGetCurrentBGMPlayer(out IAudioPlayer currentPlayer) &&
                    currentPlayer.ID.Equals(bgmID))
                {
                    _currentBGMPlayer = null;
                }
            }
        }
    }
}
