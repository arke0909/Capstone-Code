using System.Collections.Generic;
using Code.Items.ItemInfo;
using DewmoLib.Dependencies;
using Scripts.Players;
using UnityEngine;
using Work.Code.Craft;
using Work.Code.Craft.Installer;
using Work.Code.GameEvents;

namespace Work.Code.PlayerTasks.TaskTrigger
{
    public class CraftPinItemTask : PlayerTask
    {
        [SerializeField] private CraftTreeUI craftTreeUI;
        [SerializeField] private CraftingTask craftingTaskPrefab;
        [SerializeField] private ItemType targetType;
        [SerializeField] private Rarity rarity;

        [Inject] private TaskController _taskRunner;
        [Inject] private CraftPinItemContainer _pinItemContainer;

        private readonly List<CraftingTask> _craftingTasks = new();
        private readonly List<CraftingTask> _waitingRemoveTasks = new();
        private CraftTreeSO _targetTree;
        
        public override bool ShowOnTaskUI => false;

        public override void StartTask()
        {
            base.StartTask();

            ClearCraftingTasks();
            if (!_pinItemContainer.TryGetTree(targetType, rarity, out CraftTreeSO tree))
            {
                CompleteTask();
                return;
            }

            _targetTree = tree;
            CreateCraftingTasks(tree);

            if (_craftingTasks.Count == 0)
            {
                CompleteTask();
                return;
            }

            _player.LocalEventBus.Subscribe<CompleteCraftingEvent>(HandleCompleteCrafting);
            _taskRunner.OnTaskRemoved -= HandleTaskRemoved;
            _taskRunner.OnTaskRemoved += HandleTaskRemoved;
            _taskRunner.AddTasksOnCurrentGroup(_craftingTasks.ToArray());
        }

        protected override void StopTask()
        {
            _player.LocalEventBus.Unsubscribe<CompleteCraftingEvent>(HandleCompleteCrafting);
        }

        private void OnDestroy()
        {
            ClearCraftingTasks();
        }

        private void CreateCraftingTasks(CraftTreeSO tree)
        {
            HashSet<CraftTreeSO> childTrees = new HashSet<CraftTreeSO>();
            int count = tree.isBinary ? 2 : 3;

            for (int i = 1; i <= count && i < tree.nodeList.Count; i++)
            {
                CraftTreeSO childTree = tree.nodeList[i].Tree;
                if (childTree == null || childTree.Item == null || !childTrees.Add(childTree))
                    continue;

                CreateCraftingTask(childTree);
            }
            
            CreateCraftingTask(tree);
        }

        private void HandleCompleteCrafting(CompleteCraftingEvent evt)
        {
            if (_targetTree == null || evt.CraftedItem != _targetTree.Item)
                return;

            _player.LocalEventBus.Unsubscribe<CompleteCraftingEvent>(HandleCompleteCrafting);

            foreach (CraftingTask task in _craftingTasks)
            {
                if (task != null)
                    task.CompleteCraftingTask();
            }

            CompleteTask();
        }

        private void CreateCraftingTask(CraftTreeSO tree)
        {
            CraftingTask task = Instantiate(craftingTaskPrefab, transform);
            task.gameObject.SetActive(true);
            task.name = $"{tree.Item.itemName}_CraftingTask";
            task.InitTask(craftTreeUI, tree.Item, false);
            _craftingTasks.Add(task);
            _waitingRemoveTasks.Add(task);
        }

        private void HandleTaskRemoved(PlayerTask task)
        {
            if (task is not CraftingTask craftingTask || !_waitingRemoveTasks.Remove(craftingTask))
                return;

            if (_waitingRemoveTasks.Count > 0)
                return;

            ClearCraftingTasks();
        }

        private void ClearCraftingTasks()
        {
            if (_player != null)
                _player.LocalEventBus.Unsubscribe<CompleteCraftingEvent>(HandleCompleteCrafting);

            if (_taskRunner != null)
                _taskRunner.OnTaskRemoved -= HandleTaskRemoved;

            _targetTree = null;
            _waitingRemoveTasks.Clear();

            foreach (CraftingTask task in _craftingTasks)
            {
                if (task != null)
                    Destroy(task.gameObject);
            }

            _craftingTasks.Clear();
        }

        protected override string GetTaskText()
        {
            return string.Empty;
        }
    }
}
