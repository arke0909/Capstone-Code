using Ami.BroAudio;
using Chipmunk.ComponentContainers;
using Scripts.Entities;
using UnityEngine;

namespace SHS.Scripts.Effects
{
    public class CraftEffect : MonoBehaviour, IContainerComponent
    {
        [SerializeField] private GameObject toolboxModel;
        [SerializeField] private ParticleSystem craftParticle;
        [SerializeField] private SoundID craftID;
        private EntityAnimatorTrigger _animatorTrigger;
        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _animatorTrigger = componentContainer.Get<EntityAnimatorTrigger>();
            StopCrafting();
        }

        public void StartCrafting()
        {
            BroAudio.Play(craftID, transform.position);
            gameObject.SetActive(true);
            _animatorTrigger.OnAnimationImpactTrigger += PlayCraftingEffect;
        }

        public void StopCrafting()
        {
            gameObject.SetActive(false);
            _animatorTrigger.OnAnimationImpactTrigger -= PlayCraftingEffect;
        }

        public void PlayCraftingEffect()
        {
            craftParticle.Play();
        }
    }
}