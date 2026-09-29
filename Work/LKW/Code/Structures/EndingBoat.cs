using Chipmunk.ComponentContainers;
using Code.InventorySystems.Items;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.Items.ItemInfo;
using Code.Players;
using Scripts.Entities;
using Scripts.GameSystem;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace Code.Structures
{
    public class EndingBoat : InteractableStructure
    {
        [Header("Key")]
        [SerializeField] private ItemDataSO boatKey;
        [SerializeField] private BoatKeyInsertUI keyInsertUI;

        [Header("Timeline")]
        [SerializeField] private PlayableDirector boatTimeline;
        [SerializeField] private CinemachineBrain cinemachineBrain;
        [SerializeField] private Camera cutsceneCamera;
        [SerializeField] private GameObject fovObject;

        [Header("Events")]
        [FormerlySerializedAs("onKeyInserted")]
        [SerializeField] private UnityEvent onDepart;

        private PlayerInventory _playerInventory;
        private bool _isKeyInserted;
        private bool _hasDeparted;

        public bool IsKeyInserted => _isKeyInserted;
        public bool HasDeparted => _hasDeparted;
        public ItemDataSO BoatKey => boatKey;

        public override void Interact(Entity interactor)
        {
            if (_hasDeparted || interactor == null)
                return;

            _playerInventory = interactor.Get<PlayerInventory>();
            if (_playerInventory == null)
                return;

            if (keyInsertUI == null)
            {
                Debug.LogError($"[{nameof(EndingBoat)}] Key Insert UI가 연결되지 않았습니다.", this);
                return;
            }

            EventBus.Raise(new OpenPlayerUIEvent(false));
            keyInsertUI.Open(this);
        }

        public bool TryInsertKey()
        {
            if (!CanInsertKey())
                return false;

            if (_playerInventory.GetItemCount(boatKey) <= 0)
            {
                keyInsertUI.ShowMessage("보트 열쇠가 필요하다.");
                return false;
            }

            if (!_playerInventory.RemoveItemByData(boatKey, 1))
                return false;

            CompleteKeyInsertion();
            return true;
        }

        public bool TryInsertKey(ItemSlot sourceSlot)
        {
            if (!CanInsertKey())
                return false;

            if (sourceSlot?.Item?.ItemData != boatKey)
            {
                keyInsertUI.ShowMessage("이 열쇠로는 시동을 걸 수 없다.");
                return false;
            }

            if (sourceSlot.OwnerInventory != _playerInventory ||
                !_playerInventory.IsActiveSlot(sourceSlot))
            {
                keyInsertUI.ShowMessage("플레이어 인벤토리의 열쇠만 사용할 수 있다.");
                return false;
            }

            // 드래그한 정확한 슬롯에서 한 개만 제거한다.
            if (!_playerInventory.RemoveItem(sourceSlot.Item, 1, false))
                return false;

            CompleteKeyInsertion();
            return true;
        }

        private bool CanInsertKey()
        {
            if (_isKeyInserted || _playerInventory == null || boatKey == null || keyInsertUI == null)
                return false;

            return true;
        }

        private void CompleteKeyInsertion()
        {
            _isKeyInserted = true;
            keyInsertUI.ShowInsertedKey(boatKey.itemImage);
        }

        public void Depart()
        {
            if (!_isKeyInserted || _hasDeparted)
                return;

            _hasDeparted = true;
            keyInsertUI.Close();
            fovObject.SetActive(false);
            SetCutsceneFog();
            SetCutsceneCameraBlend();
            boatTimeline.time = 0f;
            boatTimeline.Evaluate();
            boatTimeline.Play();
            onDepart?.Invoke();
        }

        private void SetCutsceneCameraBlend()
        {
            var blend = cinemachineBrain.DefaultBlend;
            blend.Time = 0f;
            cinemachineBrain.DefaultBlend = blend;
        }

        private void SetCutsceneFog()
        {
            Color fogColor = new Color(0.5f, 0.7f, 0.8f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogStartDistance = 15f;
            RenderSettings.fogEndDistance = 200f;

            cutsceneCamera.clearFlags = CameraClearFlags.SolidColor;
            cutsceneCamera.backgroundColor = fogColor;
        }

        private void OnValidate()
        {
            if (boatKey != null && boatKey.maxStack != 1)
                Debug.LogWarning($"[{nameof(EndingBoat)}] 보트 열쇠의 maxStack을 1로 설정하는 것을 권장합니다.", this);
        }
    }
}
