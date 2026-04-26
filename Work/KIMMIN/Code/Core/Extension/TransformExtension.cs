using UnityEngine;

namespace Work.Code.Core.Extension
{
    public static class TransformExtensions
    {
        public static void SetLocalScale(this Transform t, float scale)
        {
            t.localScale = Vector3.one * scale;
        }
    }
}