using UnityEngine;

namespace Work.Code.Core.Extension
{
    public static class VectorExtension
    {
        public static Vector3 SetScale(this Vector3 _, float scale)
        { 
            return Vector3.one * scale;
        }
        
        public static Vector3 GetRandomInsideRadius(this Vector3 origin, float radius)
        {
            return origin + UnityEngine.Random.insideUnitSphere * radius;
        }
        
        public static Vector3 GetRandomInsideCircle(this Vector3 origin, float radius)
        {
            Vector2 circle = UnityEngine.Random.insideUnitCircle * radius;
            return origin + new Vector3(circle.x, 0f, circle.y);
        }
        
        public static Vector3 GetRandomInsideUnitCircle(this Vector3 origin, float minRadius, float maxRadius)
        {
            Vector2 circle = UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(minRadius, maxRadius);
            return origin + new Vector3(circle.x, 0f, circle.y);
        }
    }
}