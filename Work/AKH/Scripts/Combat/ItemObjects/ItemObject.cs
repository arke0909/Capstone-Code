using Code.Items;
using Scripts.Entities;
using SHS.Scripts.Entities.Players;
using UnityEngine;

namespace Scripts.Combat.ItemObjects
{
    public abstract class ItemObject : MonoBehaviour
    {
        [field: SerializeField] public GrabableObjectBehavior GrabableObjectBehavior { get; private set; }

        protected Entity _owner;
        protected EquipableItem _item;
        private Renderer[] _targetRenderers;
        private FindableRenderer _findableRenderer;

        public virtual void InitObject(Entity owner, EquipableItem item)
        {
            UnregisterRenderers();
            _owner = owner;
            _item = item;
            _targetRenderers = GetComponentsInChildren<Renderer>(true);

            if (_owner != null && _owner.ComponentContainer != null &&
                _owner.ComponentContainer.TryGetComponent(out _findableRenderer))
                _findableRenderer.AddRenderers(_targetRenderers);
        }

        protected virtual void OnDestroy()
        {
            UnregisterRenderers();
        }

        private void UnregisterRenderers()
        {
            if (_findableRenderer == null)
                return;

            _findableRenderer.RemoveRenderers(_targetRenderers);
            _findableRenderer = null;
        }
    }
}
