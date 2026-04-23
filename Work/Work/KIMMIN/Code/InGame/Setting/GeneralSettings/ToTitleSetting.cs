using System;
using EasyTransition;
using UnityEngine;
using UnityEngine.UI;
using Work.Code.Core;

namespace Work.Code.Setting.GeneralSetting
{
    public class ToTitleSetting : MonoBehaviour
    {
        [SerializeField] private Button titleButton;
        [SerializeField] private TransitionSettings transition;

        private void Awake()
        {
            titleButton.onClick.AddListener(HandleToTitle);
        }
        
        private void OnDestroy()
        {
            titleButton.onClick.RemoveListener(HandleToTitle);
        }

        private void HandleToTitle()
        {
            TransitionManager.Instance().Transition(SceneDefine.TITLE_SCENE, transition, 0f);
        }
    }
}