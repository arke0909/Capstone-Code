using TMPro;
using UnityEngine;

namespace Code.TimeSystem
{
    public class TimeUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI dayText;

        public void SetTime(string time)
        {
            timeText.SetText(time);
        }

        public void SetDay(string day)
        {
            dayText.SetText(day);
        }
    }
}