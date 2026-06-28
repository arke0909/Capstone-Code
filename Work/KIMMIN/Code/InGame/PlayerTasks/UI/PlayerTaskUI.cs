using System;
using Code.UI.Bar;
using Code.UI.Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Work.Code.UI.Misc;

namespace Work.Code.PlayerTasks.UI
{
    [RequireComponent(typeof(LayoutElement))]
    public class PlayerTaskUI : UIBase
    {
        [SerializeField] private DynamicText taskText;
        [SerializeField] private BarComponent progressBar;
        [SerializeField] private Image dot;
        [SerializeField] private float moveOffset = 80f;
        [SerializeField] private float tweenDuration = 0.15f;

        private PlayerTask _task;
        private IProgressTask _progressTask;
        private LayoutElement _layoutElement;
        private Sequence _tweenSeq;
        private Vector2 _originPos;

        protected override void Awake()
        {
            base.Awake();
            _originPos = Rect.anchoredPosition;
            _layoutElement = GetComponent<LayoutElement>();
        }

        public void InitTaskUI(PlayerTask task)
        {
            _task = task;
            _progressTask = task as IProgressTask;
            _layoutElement.ignoreLayout = false;

            task.OnTaskTextChanged += HandleTextChanged;
            
            HandleTextChanged(task);
            progressBar?.ResetBarColor();
            RefreshProgress();
        }

        public void ShowTaskUI()
        {
            _tweenSeq?.Kill();
            _originPos = Rect.anchoredPosition;
            Rect.anchoredPosition = _originPos + Vector2.right * moveOffset;
            EnableUI(true);

            _tweenSeq = DOTween.Sequence();
            _tweenSeq.Join(Rect.DOAnchorPosX(_originPos.x, tweenDuration)
                .SetEase(Ease.OutCubic));
            _tweenSeq.SetUpdate(true);
        }

        public void HideTaskUI(Action completeCallback)
        {
            _tweenSeq?.Kill();
            DisableUI(true);

            _tweenSeq = DOTween.Sequence();
            _tweenSeq.Join(Rect.DOAnchorPosX(_originPos.x - moveOffset, tweenDuration)
                .SetEase(Ease.InCubic));
            _tweenSeq.OnComplete(() =>
            {
                ClearTaskUI();
                _layoutElement.ignoreLayout = true;
                completeCallback?.Invoke();
            });
            
            _tweenSeq.SetUpdate(true);
        }

        public void CompleteTaskUI()
        {
            RefreshProgress();
            progressBar?.SetBarColor(UIDefine.GreenColor);
            SetUIColor(UIDefine.GreenColor);
        }

        private void HandleTextChanged(PlayerTask task)
        {
            taskText.SetText(task.TaskText);
        }

        public void DisableTaskUI()
        {
            _tweenSeq?.Kill();
            Rect.anchoredPosition = _originPos;
            ClearTaskUI();
            _layoutElement.ignoreLayout = true;
            DisableUI();
        }

        private void ClearTaskUI()
        {
            if (_task != null)
            {
                _task.OnTaskTextChanged -= HandleTextChanged;
            }

            _task = null;
            _progressTask = null;
            
            if (progressBar != null)
            {
                progressBar.ResetBarColor();
                progressBar.DisableUI();
            }
            
            SetUIColor(Color.white);
            Rect.anchoredPosition = _originPos;
        }

        private void SetUIColor(Color color)
        {
            taskText.Text.color = color;
            dot.color = color;
        }

        private void Update()
        {
            if (_progressTask == null)
                return;

            if (!_progressTask.HasProgress)
            {
                if (progressBar != null)
                    progressBar.DisableUI();

                return;
            }

            RefreshProgress();
        }

        private void RefreshProgress()
        {
            if (progressBar == null)
                return;

            if (_progressTask == null || !_progressTask.HasProgress)
            {
                progressBar.DisableUI();
                return;
            }

            progressBar.EnableUI();
            progressBar.SetBar(Mathf.Clamp01(_progressTask.Progress), 1f);
        }

        protected override void OnDestroy()
        {
            _tweenSeq?.Kill();
            base.OnDestroy();
        }
    }
}
