using Code.UI.Core;
using Code.UI.Minimap.Core;
using DewmoLib.Dependencies;
using Scripts.Entities;
using Scripts.Players;
using Scripts.Players.States;
using UnityEngine;
using Work.Code.GameEvents;

namespace Scripts.GameSystem.Structures
{
    public class TeleportStructure : InteractableStructure
    {
        [SerializeField] private Sprite teleportIcon;
        [SerializeField] private Transform targetTrm;
        [Inject]private Player _interactor;
        private UIPanel _viewOnlyMinimapUI;
        
        protected override void Start()
        {
            base.Start();
            _viewOnlyMinimapUI = UIManager.Instance.GetPanel<UIPanel>("TeleportMinimap");
            MinimapUtil.AddToMinimap(this, ElementType.LockedMarker, teleportIcon, true, transform.position);
            MinimapUtil.AddToMinimap(this, ElementType.Teleport, null, false, transform.position);
        }

        public override void Interact(Entity interactor)
        {
            if (_interactor.StateMachine.CurrentStateEnum == PlayerStateEnum.Teleport)
                return;
            _viewOnlyMinimapUI.ToggleUI(true);
        }
        public void Teleport()
        {
            _viewOnlyMinimapUI.ToggleUI(true);
            var context = _interactor.Blackboard.GetOrDefault<TeleportContext>("TeleportContext");
            if (context == null)
            {
                context = new TeleportContext();
                _interactor.Blackboard.Set("TeleportContext", context);
            }
            context.duration = 3;
            context.targetPosition = targetTrm.position;
            _interactor.ChangeState(PlayerStateEnum.Teleport);
            _interactor.LocalEventBus.Raise(new PlayerTeleportEvent());
        }
    }
}
