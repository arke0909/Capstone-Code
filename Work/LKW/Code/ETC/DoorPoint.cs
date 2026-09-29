using Ami.BroAudio;
using Chipmunk.GameEvents;
using Code.TimeSystem;
using Code.UI.Controller;
using Code.UI.Core;
using Code.UI.Popup;
using Scripts.Entities;
using Scripts.GameSystem;
using Scripts.Players;
using Scripts.Players.States;
using UnityEngine;
using UnityEngine.Events;
using Work.Code.GameEvents;

namespace Code.ETC
{
    public class DoorPoint : InteractableStructure
    {
        [SerializeField] private SoundID crossDoorSound;
        [SerializeField] private Transform targetPoint;
        [SerializeField] private bool isBossRoomDoor;
        [SerializeField] private int bossRoomEnterableDay = 2;
        [SerializeField] private bool disableTargetObjectOnDayReset;
        [SerializeField] private GameObject targetObjectToDisableOnDayReset;
        [SerializeField] private UnityEvent<Entity> onInteract;

        private int _lastBossRoomEnterDay;

        private void OnEnable()
        {
            Bus.Subscribe<DayChangeEvent>(HandleDayChange);
        }

        private void OnDisable()
        {
            Bus.Unsubscribe<DayChangeEvent>(HandleDayChange);
        }

        public override void Interact(Entity interactor)
        {
            if (!CanEnter())
            {
                OverlayUIManager.Instance.ShowPopup(new RoomEnterFailedData());
                return;
            }
            
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
                context.onComplete += () => OnCompleteTeleport(interactorPlayer);
                interactorPlayer.ChangeState(PlayerStateEnum.Teleport);
                
                MarkEntered();
            }
        }
        
        private void OnCompleteTeleport(Entity interactor)
        {
            onInteract?.Invoke(interactor);
            BroAudio.Play(crossDoorSound);
        }
        private void HandleDayChange(DayChangeEvent evt)
        {
            if (!isBossRoomDoor)
                return;

            _lastBossRoomEnterDay = 0;

            if (disableTargetObjectOnDayReset)
                GetTargetObjectToDisable()?.SetActive(false);
        }

        private bool CanEnter()
        {
            if (!isBossRoomDoor)
                return true;

            int currentDay = GetCurrentDay();
            return currentDay >= bossRoomEnterableDay && _lastBossRoomEnterDay != currentDay;
        }

        private void MarkEntered()
        {
            if (!isBossRoomDoor)
                return;

            _lastBossRoomEnterDay = GetCurrentDay();
        }

        private int GetCurrentDay()
        {
            return TimeController.Instance != null
                ? Mathf.Max(1, TimeController.Instance.CurrentDay)
                : 1;
        }

        private GameObject GetTargetObjectToDisable()
        {
            if (targetObjectToDisableOnDayReset != null)
                return targetObjectToDisableOnDayReset;

            return targetPoint != null ? targetPoint.gameObject : null;
        }
    }
}
