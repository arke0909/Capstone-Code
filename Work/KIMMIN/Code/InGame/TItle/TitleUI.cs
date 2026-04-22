using System;
using EasyTransition;
using Ricimi;
using UnityEngine;
using UnityEngine.UI;
using Work.Code.Core;

namespace Work.Code.Setting
{
    public class TitleUI : MonoBehaviour
    {
        [SerializeField] private Button playbutton;
        [SerializeField] private Button exitButton;
        [SerializeField] private TransitionSettings transition;

        private void Awake()
        {
            playbutton.onClick.AddListener(HandlePlay);
            exitButton.onClick.AddListener(() => Application.Quit());
        }

        private void OnDestroy()
        {
            playbutton.onClick.RemoveListener(HandlePlay);
            exitButton.onClick.RemoveAllListeners();
        }

        private void HandlePlay()
        {
            TransitionManager.Instance().Transition(SceneDefine.MAP_SCENE, transition, 0f);
        }
    }
}