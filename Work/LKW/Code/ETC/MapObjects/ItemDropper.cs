using DewmoLib.ObjectPool.RunTime;
using DG.Tweening;
using UnityEngine;
using Code.Items;
using Code.Items.ItemInfo;
using Chipmunk.Library.Utility.GameEvents.Local;
using Chipmunk.ComponentContainers;

namespace Code.ETC.MapObjects
{
    public abstract class ItemDropper : MonoBehaviour,IContainerComponent, ILocalEventSubscriber<ItemDropEvent>
    {
        [SerializeField] private PoolManagerSO poolManager;
        [SerializeField] private PoolItemSO previewItemPool;

        [Header("Drop Settings")]
        [SerializeField] private float arcHeight = 1f;
        [SerializeField] private float dropDuration = 0.4f;

        public ComponentContainer ComponentContainer { get; set; }

        public abstract PreviewItem Drop(Vector3 from, Vector3 to);

        public void OnInitialize(ComponentContainer componentContainer)
        {
        }

        public void OnLocalEvent(ItemDropEvent eventData)
        {
            Drop(eventData.From, eventData.To);
        }

        protected PreviewItem SpawnItem(Vector3 from, Vector3 to, ItemCreateData createData)
        {
            if (createData.Item == null || poolManager == null || previewItemPool == null)
                return null;

            var item = poolManager.Pop(previewItemPool) as PreviewItem;
            if (item == null)
                return null;

            item.Discard(from, createData.Item, createData.Stack);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(item.transform.DOMoveX(to.x, dropDuration).SetEase(Ease.Linear));
            sequence.Join(item.transform.DOMoveZ(to.z, dropDuration).SetEase(Ease.Linear));
            sequence.Join(item.transform.DOMoveY(from.y + arcHeight, dropDuration * 0.4f).SetEase(Ease.OutQuad));
            sequence.Insert(dropDuration * 0.4f,
                item.transform.DOMoveY(to.y, dropDuration * 0.6f).SetEase(Ease.InQuad));
            sequence.OnComplete(() =>
                item.transform.DOPunchScale(Vector3.one * 0.15f, 0.15f, 1, 0f));

            return item;
        }
    }
}
