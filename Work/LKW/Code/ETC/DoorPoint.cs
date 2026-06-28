using Ami.BroAudio;
using Scripts.Entities;
using Scripts.GameSystem;
using Scripts.Players;
using Scripts.Players.States;
using UnityEngine;

namespace Code.ETC
{
    public class DoorPoint : InteractableStructure
    {
        [SerializeField] private SoundID crossDoorSound;
        [SerializeField] private Transform targetPoint;
        
        public override void Interact(Entity interactor)
        {
                if (interactor is Player interactorPlayer)
                {
                    var context = interactorPlayer.Blackboard.GetOrDefault<TeleportContext>("TeleportContext");
                    if (context == null)
                    {
                        context = new TeleportContext();
                        interactorPlayer.Blackboard.Set("TeleportContext", context);
                    }
                    context.duration = 2;
                    context.targetPosition = targetPoint.position;
                    context.onComplete += () => BroAudio.Play(crossDoorSound);
                    interactorPlayer.ChangeState(PlayerStateEnum.Teleport);
                }
        }
    }
}