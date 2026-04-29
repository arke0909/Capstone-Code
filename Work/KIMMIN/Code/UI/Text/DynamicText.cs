using TMPro;
using UnityEngine;

namespace Work.Code.UI.Misc
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class DynamicText : MonoBehaviour
    {
        public TextMeshProUGUI Text { get; private set; }
        private ITextEffect[] _textEffects;
        
        private void Awake()
        {
            Text = GetComponent<TextMeshProUGUI>();
            _textEffects = GetComponentsInChildren<ITextEffect>();

            foreach (var effect in _textEffects)
            {
                effect.InitText(Text);
            }
        }

        public virtual void SetText(string text, bool nonEffect = false)
        {
            if (Text.text == text) return;
            Text.text = text;
            
            if (nonEffect) return;
            foreach (var effect in _textEffects)
            {
                effect.PlayEffect(Text);
            }
        }

        public void PlayEffect()
        {
            foreach (var effect in _textEffects)
            {
                effect.PlayEffect(Text);
            }
        }
    }
}