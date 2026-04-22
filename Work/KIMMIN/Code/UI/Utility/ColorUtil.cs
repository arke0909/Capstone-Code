using UnityEngine;

namespace Work.Code.UI.Utility
{
    public static class ColorUtil
    {
        public static string Format(StatSetting setting, float value)
        {
            float v = value;
            Color color;
            bool isBad = v >= setting.badMin && v <= setting.badMax;

            if (isBad)
            {
                float t = Mathf.InverseLerp(setting.badMin, setting.badMax, v);
                t = Mathf.Clamp01(t);

                color = Color.Lerp(
                    new Color(1f, 0.8f, 0.8f),
                    new Color(1f, 0.4f, 0.4f), t);
            }
            else
            {
                float t;
                if (setting.invert)
                    t = Mathf.InverseLerp(setting.goodMax, setting.goodMin, v);
                else
                    t = Mathf.InverseLerp(setting.goodMin, setting.goodMax, v);

                t = Mathf.Clamp01(t);
                color = Color.Lerp(
                    new Color(0.9f, 1f, 0.9f),
                    new Color(0.4f, 1f, 0.4f), t);
            }

            string hex = ColorUtility.ToHtmlStringRGB(color);
            var prefix = string.IsNullOrEmpty(setting.prefix) ? ":" : setting.prefix;
            return $"{setting.label} {prefix} <color=#{hex}>{value:0.##}{setting.suffix}</color>";
        }
    }
}