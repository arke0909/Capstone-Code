using Chipmunk.ComponentContainers;
using Scripts.Combat.Fovs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Entities
{
    public class FindableRenderer : MonoBehaviour, IContainerComponent
    {
        [SerializeField] private bool _hideOnAwake = true;
        [SerializeField] private List<Renderer> _targetRenderers;
        public ComponentContainer ComponentContainer { get; set; }
        private IFindable _findable;
        public void OnInitialize(ComponentContainer componentContainer)
        {
            CacheRenderers();
            _findable = componentContainer.GetSubclassComponent<IFindable>();
            _findable.OnFound.AddListener(ApplyVisibleState);
            ApplyVisibleState(!_hideOnAwake);
        }

        private void CacheRenderers()
        {
            if (_targetRenderers == null || _targetRenderers.Count == 0)
                _targetRenderers = GetComponentsInChildren<Renderer>(true).ToList();

            for (int i = 0; i < _targetRenderers.Count; i++)
            {
                Renderer targetRenderer = _targetRenderers[i];
            }
        }
        private void ApplyVisibleState(bool onFound)
        {
            if (_targetRenderers == null)
                return;

            for (int i = 0; i < _targetRenderers.Count; i++)
            {
                Renderer targetRenderer = _targetRenderers[i];
                if (targetRenderer == null)
                    continue;

                targetRenderer.forceRenderingOff = !onFound;
            }
        }
    }
}
