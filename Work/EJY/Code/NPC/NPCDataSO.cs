using UnityEngine;

namespace Code.NPC
{
    [CreateAssetMenu(fileName = "NPC Data", menuName = "SO/NPC/Data", order = 0)]
    public class NPCDataSO : ScriptableObject
    {
        public string npcName;
        public bool isOneTime;
    }
}