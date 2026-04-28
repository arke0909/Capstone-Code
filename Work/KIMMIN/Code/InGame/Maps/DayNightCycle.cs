using Code.TimeSystem;
using DewmoLib.Dependencies;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Work.Code.Map
{
    public class DayNightCycle : MonoBehaviour
    {
        [SerializeField] private Volume volume;
        [SerializeField] private AnimationCurve exposureCurve;
        [SerializeField] private AnimationCurve temperatureCurve;
        [SerializeField] private Gradient colorFilter;
        //[SerializeField] private float updateInterval = 0.5f;
        
        [Inject] private TimeController _timeController;

        private ColorAdjustments _color;
        private WhiteBalance _whiteBalance;
        private LiftGammaGain _liftGammaGain;
        private float _time;

        private void Awake()
        {
            volume.profile.TryGet(out _color);
            volume.profile.TryGet(out _whiteBalance);
            volume.profile.TryGet(out _liftGammaGain);
        }

        private void Update()
        {
            /*if (Time.time - _time > updateInterval)
            {
                _time = Time.time;
                UpdateEnvironment();
            }*/
        }

        private void UpdateEnvironment()
        {
            float t = _timeController.DayTime / _timeController.SecondsPerDay;

            float exposure = exposureCurve.Evaluate(t);
            _color.postExposure.value = exposure;

            float temperature = temperatureCurve.Evaluate(t);
            _whiteBalance.temperature.value = temperature;

            _color.colorFilter.value = colorFilter.Evaluate(t);

            float contrast = Mathf.Lerp(10f, 30f, Mathf.Abs(t - 0.5f) * 2f);
            _color.contrast.value = contrast;

            float lift = Mathf.Lerp(0f, -0.1f, Mathf.Abs(t - 0.5f) * 2f);
            _liftGammaGain.lift.value = new Vector4(lift, lift, lift, 0f);
        }
    }
}