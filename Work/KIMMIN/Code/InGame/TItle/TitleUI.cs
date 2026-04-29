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
        [SerializeField] private Button tutorialButton;
        [SerializeField] private TransitionSettings transition;

        private void Awake()
        {
            playbutton.onClick.AddListener(() => HandlePlay(SceneDefine.MAP_SCENE));
            tutorialButton.onClick.AddListener(() => HandlePlay(SceneDefine.TUTORIAL_SCENE));
            exitButton.onClick.AddListener(() => Application.Quit());
        }

        private void OnDestroy()
        {
            playbutton.onClick.RemoveAllListeners();
            tutorialButton.onClick.RemoveAllListeners();
            exitButton.onClick.RemoveAllListeners();
        }

        private void HandlePlay(string sceneName)
        {
            TransitionManager.Instance().Transition(sceneName, transition, 0f);
        }
    }
}