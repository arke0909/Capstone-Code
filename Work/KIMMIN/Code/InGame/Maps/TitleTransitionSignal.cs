using EasyTransition;
using UnityEngine;
using Work.Code.Core;

namespace Work.Code.Map
{
    public class TitleTransitionSignal : MonoBehaviour
    {
        [SerializeField] private TransitionSettings transition;

        public void OnReceive()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            TransitionManager.Instance().Transition(SceneDefine.TITLE_SCENE, transition, 0f);
        }
    }
}
