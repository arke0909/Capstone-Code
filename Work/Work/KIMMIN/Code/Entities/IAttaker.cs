using Scripts.Entities;
using UnityEngine;

namespace Work.Code.Entities
{
    public interface IDamageDelaer
    {
        GameObject Dealer { get; }
        Entity Owner { get; }
    }
}