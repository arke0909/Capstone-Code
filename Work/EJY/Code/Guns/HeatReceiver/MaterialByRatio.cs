using Code.ETC;
using UnityEngine;

namespace Code.Guns.HeatReceiver
{
    public class MaterialByRatio : MonoBehaviour, IRatioReceiver
    {
        [SerializeField] private Transform visualTrm;
        
        private readonly int _heatRatio = Shader.PropertyToID("_HeatRatio");

        private MeshRenderer[] _meshRenderers;
        
        private void Awake()
        {
            _meshRenderers = visualTrm.GetComponentsInChildren<MeshRenderer>();
        }

        public void SetRatio(float ratio)
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