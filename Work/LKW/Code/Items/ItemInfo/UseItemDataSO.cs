using UnityEngine;

namespace Code.Items.ItemInfo
{
    public abstract class UseItemDataSO : HandItemDataSO
    {
        [Min(0.01f)] public float useDuration = 1f;
    }
}
