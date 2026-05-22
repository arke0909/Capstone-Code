using Scripts.Entities;
using UnityEngine;
using UnityEngine.Playables;
using Code.ItemContainers;
using Scripts.GameSystem;

namespace Work.Code.Map
{
    public class TutorialHelicopter : InteractableStructure, IInteractable
    {
        [SerializeField] private PlayableDirector helicopterCutscene;

        public override void Interact(Entity interactor)
        {
            helicopterCutscene?.Play();
        }
    }
}