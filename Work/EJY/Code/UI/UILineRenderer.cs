using Code.SkillSystem.Upgrade;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class UILineRenderer : MaskableGraphic
    {
        public Vector2[] points = new Vector2[2];
        public float thickness = 2f;
        public bool center = false;
        public Color lineColor = Color.white;
        public SkillUpgradeSO data;
        
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if(points.Length < 2) return;
            
            for(int i = 0; i < points.Length - 1; i++)
            {
                CreateLineSegment(points[i], points[i + 1], vh);
                int index = i * 5;
                vh.AddTriangle(index, index + 1, index + 3);
                vh.AddTriangle(index, index + 3, index + 2);

                if (i != 0)
                {
                    vh.AddTriangle(index, index - 1, index - 3);
                    vh.AddTriangle(index + 1, index - 1, index - 2);
                }
            }
            
        }

        private void CreateLineSegment(Vector3 point1, Vector3 point2, VertexHelper vh)
        {
             Vector3 offset = center ? (rectTransform.sizeDelta * 0.5f) : Vector3.zero;
             
             UIVertex vertex = UIVertex.simpleVert;
             vertex.color = lineColor;

             Quaternion point1Rotation = Quaternion.Euler(0, 0, RotationPointToward(point1, point2) + 90f);
             vertex.position = point1Rotation * new Vector3(-thickness * 0.5f, 0f);
             vertex.position += point1 + offset;
             vh.AddVert(vertex);
             vertex.position = point1Rotation * new Vector3(thickness * 0.5f, 0f);
             vertex.position += point1 + offset;
             vh.AddVert(vertex);
             
             Quaternion point2Rotation = Quaternion.Euler(0, 0, RotationPointToward(point2, point1) - 90f);
             vertex.position = point2Rotation * new Vector3(-thickness * 0.5f, 0f);
             vertex.position += point2 + offset;
             vh.AddVert(vertex);
             vertex.position = point2Rotation * new Vector3(thickness * 0.5f, 0f);
             vertex.position += point2 + offset;
             vh.AddVert(vertex);

             vertex.position = point2 + offset;
             vh.AddVert(vertex);
        }

        private float RotationPointToward(Vector2 point1, Vector2 point2)
        {
            return Mathf.Atan2(point2.y - point1.y, point2.x - point1.x) * Mathf.Rad2Deg;
        }
    }
}