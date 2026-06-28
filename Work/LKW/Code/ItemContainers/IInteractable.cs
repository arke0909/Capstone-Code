using EPOOutline;
using Scripts.Entities;

namespace Code.ItemContainers
{
    public interface IInteractable
    {
        public void Select();
        public void DeSelect();
        public void Interact(Entity interactor);

        public Outlinable Outlinable { get; }
    }   
}