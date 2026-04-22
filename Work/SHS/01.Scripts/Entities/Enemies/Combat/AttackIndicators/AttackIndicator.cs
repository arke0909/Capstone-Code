using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace Code.SHS.Entities.Enemies.Combat.Indicators
{
    public enum CompleteAction
    {
        None,
        Destroy,
        Deactivate
    }

    [RequireComponent(typeof(DecalProjector))]
    public class AttackIndicator : MonoBehaviour
    {
        private int progressID = Shader.PropertyToID("_Progress");
        [Header("Settings")] [SerializeField] private CompleteAction defaultCompleteAction = CompleteAction.None;
        private DecalProjector _decalProjector;
        protected DecalProjector DecalProjector => _decalProjector;
        private Material _material;
        protected Material Material => _material;

        private float duration = 1f;
        private float timer = 0f;

        private void Awake()
        {
            _decalProjector = GetComponent<DecalProjector>();
            _material = _decalProjector.material;
            gameObject.SetActive(false);
        }

        public void Initialize(float duration)
        {
            this.duration = duration;
            timer = 0f;
            gameObject.SetActive(true);
            OnInitialized();
            OnUpdate(0);
        }

        private void OnInitialized()
        {
        }

        private void Update()
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            OnUpdate(progress);

            if (progress >= 1f)
            {
                Complete(defaultCompleteAction);
            }
        }

        protected virtual void OnUpdate(float progress)
        {
            Material.SetFloat(progressID, progress);
        }

        public void Complete(CompleteAction action = CompleteAction.None)
        {
            timer = duration;
            switch (action)
            {
                case CompleteAction.None:
                    OnUpdate(1f);
                    break;
                case CompleteAction.Destroy:
                    Destroy(gameObject);
                    break;
                case CompleteAction.Deactivate:
                    gameObject.SetActive(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}