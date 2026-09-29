using DG.Tweening;
using UnityEngine;

namespace SHS.Scripts
{
    public static class DOTweenSetup
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()

        {
            DOTween.SetTweensCapacity(500, 100);
        }
    }
}