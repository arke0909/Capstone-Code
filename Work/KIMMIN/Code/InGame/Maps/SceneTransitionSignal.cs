using EasyTransition;
using UnityEngine;
using Work.Code.Core;

namespace Work.Code.Map
{
    public class SceneTransitionSignal : MonoBehaviour
    {
        [SerializeField] private TransitionSettings transition;

        public void OnReceive()
        {
            TransitionManager.Instance().Transition(SceneDefine.MAP_SCENE, transition, 0f);
        }
    }
}