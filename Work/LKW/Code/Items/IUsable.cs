using Scripts.Entities;

namespace Code.Items
{
    public interface IUsable
    {
        public void Use(Entity user);
    }
}