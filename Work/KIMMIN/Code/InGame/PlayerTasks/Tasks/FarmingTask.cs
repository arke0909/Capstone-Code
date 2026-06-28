using Code.ItemContainers;
using DewmoLib.Dependencies;
using UnityEngine;
using Work.Code.Misc;
using Work.Code.Tutorials;

namespace Work.Code.PlayerTasks
{
    public class FarmingTask : PlayerTask
    {
        private const float PlayerArrowHeight = 1f;

        [SerializeField] private TutorialMarking marking;
        [SerializeField] private ItemContainer container;
        [SerializeField] private GameObject arrow;
        [SerializeField] private string containerName;
        
        [Inject] private ArrowLineManager _arrowLineManager;
        private ArrowLine _arrowLine;
        private bool _hasRemainItem;

        public override void StartTask()
        {
            base.StartTask();

            if (container == null)
            {
                Debug.LogError($"[{nameof(FarmingTask)}] Container is missing.");
                return;
            }
             
            _hasRemainItem = container.Inventory.GetRemainItems() > 0;
            container.Inventory.InventoryChanged += HandleInventoryChanged;
            SetElements(true);

            if (_arrowLineManager == null)
                return;

            _arrowLine = _arrowLineManager.CreateLine(_player.gameObject, container.gameObject);
            _arrowLine.SetOffset(Vector3.up * PlayerArrowHeight, Vector3.zero);
        }
        
        private void HandleInventoryChanged()
        {
            if (container == null)
                return;

            int remainCount = container.Inventory.GetRemainItems();

            if (remainCount > 0)
            {
                _hasRemainItem = true;
                return;
            }

            if (_hasRemainItem)
            {
                SetElements(false);
                CompleteTask();
            }
        }

        protected override void StopTask()
        {
            if (container != null)
                container.Inventory.InventoryChanged -= HandleInventoryChanged;

            if (_arrowLineManager != null)
                _arrowLineManager.RemoveLine(_arrowLine);
        }

        private void SetElements(bool isEnable)
        {
            if (arrow != null)
                arrow.SetActive(isEnable);
            if (marking != null)
                marking.SetVisual(isEnable);
        }

        protected override string GetTaskText()
        {
            return $"{containerName}상자를 열어 아이템을 모두 수집하세요";
        }
    }
}
