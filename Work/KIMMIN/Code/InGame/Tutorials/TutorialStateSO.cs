using UnityEngine;

namespace Work.Code.Tutorials
{
    [CreateAssetMenu(fileName = "TutorialStateSO", menuName = "SO/Tutorial/State", order = 0)]
    public class TutorialStateSO : ScriptableObject
    {
        public string dialogue;
        public Sprite stateIcon;
    }
}