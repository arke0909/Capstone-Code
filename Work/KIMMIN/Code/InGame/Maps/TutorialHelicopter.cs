using System;
using EPOOutline;
using Scripts.Entities;
using UnityEngine;
using UnityEngine.Playables;
using Work.LKW.Code.ItemContainers;

namespace Work.Code.Map
{
    public class TutorialHelicopter : MonoBehaviour, IInteractable
    {
        [SerializeField] private PlayableDirector helicopterCutscene;
        [field: SerializeField] public Outlinable Outlinable { get; private set; }

        private void Awake()
        {
            Outlinable.enabled = false;
        }

        public void Select()
        {
            Outlinable.enabled = true;
        }

        public void DeSelect()
        {
            Outlinable.enabled = false;
        }

        public void Interact(Entity interactor)
        {
            helicopterCutscene?.Play();
        }
    }
}