using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Code.ETC
{
    public class DecalObject : MonoBehaviour
    {
        [SerializeField] private DecalProjector decalProjector;

        private void Awake()
        {
            SetActive(false);
        }

        public void SetPos(Vector3 pos) => transform.position = pos;
        
        public void SetActive(bool active) => gameObject.SetActive(active);
        public void SetParent(Transform trm) => transform.SetParent(trm);
    }
}