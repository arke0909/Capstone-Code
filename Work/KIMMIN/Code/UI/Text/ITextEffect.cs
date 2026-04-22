using TMPro;
using UnityEngine;

namespace Work.Code.UI.Misc
{
    public interface ITextEffect
    {
        public void InitText(TextMeshProUGUI text);
        public void PlayEffect(TextMeshProUGUI text);
    }
}