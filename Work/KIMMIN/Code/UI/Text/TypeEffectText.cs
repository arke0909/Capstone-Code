using System.Collections;
using TMPro;
using UnityEngine;

namespace Work.Code.UI.Misc
{
    public class TypeEffectText : DynamicText
    {
        [SerializeField] TextMeshProUGUI textComponent;
        public readonly float _delay = 0.05f;

        private Coroutine currentCoroutine;

        public override void SetText(string text, bool hasEffect = false)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            if (hasEffect)
                textComponent.text = text;
            else
                currentCoroutine = StartCoroutine(TypeText(text));
        }

        public void Skip(string text)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            textComponent.text = text;
        }

        IEnumerator TypeText(string text)
        {
            textComponent.text = string.Empty;

            foreach (char c in text)
            {
                textComponent.text += c;
                yield return new WaitForSeconds(_delay);
            }
        }
    }
}