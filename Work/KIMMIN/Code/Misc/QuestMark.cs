using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Work.Code.Misc
{
    public class QuestMark : MonoBehaviour
    {
        [SerializeField] private TextMeshPro markText;
        
        private Camera _cam;
        private Vector3 _originPos;

        private void Awake()
        {
            _cam = Camera.main;
            markText.text = string.Empty;
            _originPos = markText.transform.localPosition;
        }

        public void SetMark(string mark, Color color, float duration = 3f)
        {
            EnableMark(mark, color);
    
            markText.transform.DOKill();
            markText.transform.localScale = Vector3.zero;
            markText.transform.localPosition = _originPos;
            markText.transform.DOScale(1f, 0.15f).SetEase(Ease.OutBack);
            markText.transform.DOLocalMoveY(_originPos.y + 1f, 0.15f).SetEase(Ease.OutBack);

            StartCoroutine(QuestMarkRoutine(duration));
        }
        
        private void LateUpdate()
        {
            if (!_cam) return;
            markText.transform.forward = _cam.transform.forward;
        }

        private void EnableMark(string mark, Color color)
        {
            markText.text = mark;
            markText.color = color;
        }

        private void DisableMark()
        {
            markText.transform.DOKill();
            markText.transform.DOScale(0f, 0.15f).SetEase(Ease.InQuad);
        }

        private IEnumerator QuestMarkRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            DisableMark();
        }
    }
}