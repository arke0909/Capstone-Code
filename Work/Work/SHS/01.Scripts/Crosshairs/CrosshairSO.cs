using UnityEngine;

namespace SHS.Scripts.Crosshairs
{
    [CreateAssetMenu(fileName = "CrosshairSO", menuName = "SO/CrosshairSO", order = 1)]
    public class CrosshairSO : ScriptableObject
    {
        public GameObject crosshairPrefab;

        private void OnValidate()
        {
            if (crosshairPrefab == null || crosshairPrefab.GetComponent<VirtualCrosshair>() == null)
            {
                Debug.LogError("Crosshair prefab must have a VirtualCrosshair component.", this);
                crosshairPrefab = null;
            }
        }
    }
}