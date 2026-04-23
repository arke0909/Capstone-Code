using System;
using DG.Tweening;
using Scripts.Entities;
using UnityEngine;

namespace Scripts.Feedbacks
{
    public class HitBlinkFeedback : Feedback
    {
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private float glowDuration;
        private static readonly int _glowAmount = Shader.PropertyToID("_GlowAmount");


        public override void CreateFeedback()
        {
            foreach (var renderer in renderers)
            {
                if (renderer)
                {
                    renderer.material.DOKill();
                    renderer.material.SetFloat(_glowAmount, 1);
                    renderer.material.DOFloat(0, _glowAmount, glowDuration);
                }
            }
        }

        private void Reset()
        {
            renderers = GetRenderers();
        }

        private Renderer[] GetRenderers()
        {
            Transform targetParent = transform.parent;
            while (targetParent.parent != null)
            {
                targetParent = targetParent.parent;
            }

            Renderer[] findRenderers = targetParent.GetComponentsInChildren<Renderer>();
            return findRenderers;
        }
    }
}

