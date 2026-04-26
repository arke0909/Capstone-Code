using System;
using DG.Tweening;
using UnityEngine;

namespace Code.UI.Core
{
    public static class UIUtility
    {
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            if(component == null)
                component = go.AddComponent<T>();
            
            return component;
        }

        public static void FadeUI(GameObject go, float duration, bool isFadeOut, 
            Action completeCallback = null)
        {
            CanvasGroup canvas = GetOrAddComponent<CanvasGroup>(go);
            float targetA = isFadeOut ? 0 : 1;

            canvas.DOFade(targetA, duration)
                .OnComplete(() => {
                    canvas.interactable = !isFadeOut;
                    canvas.blocksRaycasts = !isFadeOut;
                    completeCallback?.Invoke();
                }).SetUpdate(true);
        }
        
        public static void FadeUI(CanvasGroup canvas, GameObject go, float duration, bool isFadeOut, 
            Action completeCallback = null)
        {
            float targetA = isFadeOut ? 0 : 1;

            canvas.DOFade(targetA, duration)
                .OnComplete(() => {
                    canvas.interactable = !isFadeOut;
                    canvas.blocksRaycasts = !isFadeOut;
                    completeCallback?.Invoke();
                }).SetUpdate(true);
        }
        
        public static void ShowUI(GameObject go, bool isShow = true)
        {
            CanvasGroup canvas = GetOrAddComponent<CanvasGroup>(go);
            canvas.alpha = isShow ? 1f : 0f;
            canvas.interactable = isShow;
            canvas.blocksRaycasts = isShow;
        }
    }
}