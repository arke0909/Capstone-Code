using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.UI.Bar;
using Scripts.Entities.VisibleStates;
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
        public bool IsVisible => _visibleStateMap[_currentVisibleState].IsVisible;
        private LocalEventBus _localEventBus;
        private EntityVisibleManagement _visibleManagement;
        private readonly Dictionary<VisibleState, IVisibleModule> _visibleStateMap = new();
        private readonly Dictionary<Renderer, Material[]> _previousMaterialsByRenderer = new();
        private VisibleState _currentVisibleState;

        public void OnInitialize(ComponentContainer componentContainer)
        {
            CacheRenderers();
            InitializeVisibleStates();
            _localEventBus = componentContainer.Get<LocalEventBus>();
            _visibleManagement = componentContainer.Get<EntityVisibleManagement>();
            _visibleManagement.OnVisibleStateChanged += ApplyVisibleState;
            _currentVisibleState = _hideOnAwake ? VisibleState.OutFOV : VisibleState.InFOV;
            ApplyCurrentState();
        }

        private void OnDestroy()
        {
            if (_visibleManagement != null)
                _visibleManagement.OnVisibleStateChanged -= ApplyVisibleState;
        }
        private void InitializeVisibleStates()
        {
            _visibleStateMap[VisibleState.InFOV] = new DefaultInFOV();
            _visibleStateMap[VisibleState.OutFOV] = new DefaultOutFOV();
        }

        public void SetRenderState(VisibleState visibleState, IVisibleModule renderState)
        {
            if (renderState == null)
                return;

            _visibleStateMap[visibleState] = renderState;
            if (_currentVisibleState == visibleState)
                ApplyCurrentState();
        }

        public void ResetRenderState(VisibleState visibleState)
        {
            _visibleStateMap[visibleState] = CreateDefaultState(visibleState);
            if (_currentVisibleState == visibleState)
                ApplyCurrentState();
        }
        public void SetVisible(bool isVisible)
        {
            if (_targetRenderers == null)
                return;

            for (int i = 0; i < _targetRenderers.Count; i++)
            {
                Renderer targetRenderer = _targetRenderers[i];
                if (targetRenderer == null)
                    continue;

                targetRenderer.forceRenderingOff = !isVisible;
            }
        }

        private void ApplyVisibleState(VisibleState prev, VisibleState next, bool onFound)
        {
            _currentVisibleState = next;
            ApplyCurrentState();
        }
        private void ApplyCurrentState()
        {
            if (_targetRenderers == null)
                return;

            if (_visibleStateMap.TryGetValue(_currentVisibleState, out IVisibleModule renderState))
            {
                renderState.Apply(this);
                _localEventBus.Raise(new VisibleStateChangeEvent(_currentVisibleState, renderState.IsVisible, _visibleManagement.IsFound));
            }

        }

        private IVisibleModule CreateDefaultState(VisibleState visibleState)
        {
            return visibleState == VisibleState.InFOV
                ? new DefaultInFOV()
                : new DefaultOutFOV();
        }

        #region Renderers
        private void CacheRenderers()
        {
            if (_targetRenderers == null || _targetRenderers.Count == 0)
                _targetRenderers = GetComponentsInChildren<Renderer>(true).ToList();
        }

        public void AddRenderers(IEnumerable<Renderer> renderers)
        {
            if (renderers == null)
                return;

            foreach (Renderer targetRenderer in renderers)
            {
                AddRenderer(targetRenderer);
            }
        }

        public void AddRenderer(Renderer targetRenderer)
        {
            if (targetRenderer == null)
                return;

            _targetRenderers ??= new List<Renderer>();
            if (_targetRenderers.Contains(targetRenderer))
                return;

            _targetRenderers.Add(targetRenderer);
            ApplyCurrentState();
        }

        public void RemoveRenderers(IEnumerable<Renderer> renderers)
        {
            if (renderers == null)
                return;

            foreach (Renderer targetRenderer in renderers)
            {
                RemoveRenderer(targetRenderer);
            }
        }

        public void RemoveRenderer(Renderer targetRenderer)
        {
            if (targetRenderer == null || _targetRenderers == null)
                return;

            RestorePreviousMaterials(targetRenderer);
            _targetRenderers.Remove(targetRenderer);
        }
        #endregion

        #region Materials
        public void ApplyMaterial(Material material)
        {
            if (_targetRenderers == null || material == null)
                return;

            for (int i = 0; i < _targetRenderers.Count; i++)
            {
                Renderer targetRenderer = _targetRenderers[i];
                if (targetRenderer == null)
                    continue;

                ApplyMaterial(targetRenderer, material);
            }
        }
        public void RestorePreviousMaterials()
        {
            foreach (KeyValuePair<Renderer, Material[]> pair in _previousMaterialsByRenderer)
            {
                if (pair.Key == null)
                    continue;

                pair.Key.sharedMaterials = pair.Value;
            }

            _previousMaterialsByRenderer.Clear();
        }

        private void ApplyMaterial(Renderer targetRenderer, Material material)
        {
            if (!_previousMaterialsByRenderer.ContainsKey(targetRenderer))
                _previousMaterialsByRenderer[targetRenderer] = targetRenderer.sharedMaterials;

            Material[] currentMaterials = targetRenderer.sharedMaterials;
            Material[] changedMaterials = new Material[currentMaterials.Length];
            for (int i = 0; i < changedMaterials.Length; i++)
            {
                changedMaterials[i] = material;
            }

            targetRenderer.sharedMaterials = changedMaterials;
        }

        private void RestorePreviousMaterials(Renderer targetRenderer)
        {
            if (!_previousMaterialsByRenderer.TryGetValue(targetRenderer, out Material[] previousMaterials))
                return;

            targetRenderer.sharedMaterials = previousMaterials;
            _previousMaterialsByRenderer.Remove(targetRenderer);
        }
        #endregion
    }
}
