using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Work.Code.UI.Dropdown
{
    public class DropdownTemplate : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI optionText;
        [field: SerializeField] public Button Button { get; private set; }
        
        public void EnableFor(string option)
        {
            gameObject.SetActive(true);
            optionText.text = option;
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }
    }
}