using UnityEngine;

namespace Code.NPC
{
    public class NPCVisual : MonoBehaviour
    {
        [field: SerializeField] public NPCDataSO NPCData { get; private set; }

        public void SetVisual(bool isActive) => gameObject.SetActive(isActive);
    }
}