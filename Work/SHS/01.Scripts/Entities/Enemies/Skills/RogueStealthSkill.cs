using Ami.BroAudio;
using Chipmunk.ComponentContainers;
using Cysharp.Threading.Tasks;
using Scripts.Entities;
using Scripts.SkillSystem;
using UnityEngine;
using ChangeMaterialState = Scripts.Entities.VisibleStates.ChangeMaterial;

namespace Code.SHS.Entities.Enemies.Skills
{
    public class RogueStealthSkill : ActiveSkill
    {
        [SerializeField] private SoundID stealthSound;
        [SerializeField, Min(0f)] private float stealthDuration = 5f;
        [SerializeField] private Material stealthMaterial;

        private FindableRenderer _findableRenderer;
        private ChangeMaterialState _changeMaterialState;

        public bool IsStealthed { get; private set; }

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _findableRenderer = container.Get<FindableRenderer>();
            _changeMaterialState = new ChangeMaterialState(stealthMaterial,false);
        }

        public override async void StartSkill()
        {
            if (IsStealthed || _findableRenderer == null)
                return;

            IsStealthed = true;
            _findableRenderer.SetRenderState(VisibleState.InFOV, _changeMaterialState);
            BroAudio.Play(stealthSound, _owner.transform.position);
            
            await UniTask.WaitForSeconds(stealthDuration);
            EndSkill();
        }

        public override void EndSkill()
        {
            if (!IsStealthed)
                return;

            IsStealthed = false;
            _findableRenderer?.ResetRenderState(VisibleState.InFOV);
        }
    }
}
