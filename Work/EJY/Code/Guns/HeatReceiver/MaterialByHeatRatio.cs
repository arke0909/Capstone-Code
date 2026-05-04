using UnityEngine;

namespace Work.EJY.Code.Guns.HeatReceiver
{
    public class MaterialByHeatRatio : MonoBehaviour, IHeatRatioReceiver
    {
        [SerializeField] private Transform visualTrm;
        
        private readonly int _heatRatio = Shader.PropertyToID("_HeatRatio");

        private MeshRenderer[] _meshRenderers;
        
        private void Awake()
        {
            _meshRenderers = visualTrm.GetComponentsInChildren<MeshRenderer>();
        }

        public void SetHeatRatio(float ratio)
        {
            foreach (var meshRenderer in _meshRenderers)
            {
                meshRenderer.material.SetFloat(_heatRatio, ratio);
            }
        }

        public void ResetRatio()
        {
            foreach (var meshRenderer in _meshRenderers)
            {
                meshRenderer.material.SetFloat(_heatRatio, 0);
            }
        }
    }
}