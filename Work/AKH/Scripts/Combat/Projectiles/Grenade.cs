using Scripts.Entities;
using UnityEngine;

namespace Scripts.Combat.Projectiles
{
    public class Grenade : Throw
    {
        private void OnCollisionEnter(Collision collision)
        {
            _myPool.Push(this);
        }
    }
}
