using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Work.Code.UI.Misc
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class DynamicText : MonoBehaviour
    {
        public TextMeshProUGUI Text { get; private set; }
        private ITextEffect[] _effects;
        
        private void Awake()
        {
            Text = GetComponent<TextMeshProUGUI>();
            _effects = GetComponentsInChildren<ITextEffect>();

            foreach (var effect in _effects)
            {
                effect.InitText(Text);
            }
        }

        public void SetText(string text, bool nonEffect = false)
        {
            if (Text.text == text) return;
            Text.text = text;
            
            if (nonEffect) return;
            foreach (var effect in _effects)
            {
                effect.PlayEffect(Text);
            }
        }

        public void PlayEffect()
        {
            foreach (var effect in _effects)
            {
                effect.PlayEffect(Text);
            }
        }
    }
}