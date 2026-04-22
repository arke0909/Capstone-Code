using Code.SkillSystem.Upgrade;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Core
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class UILineRenderer : MaskableGraphic
    {
        public Vector2[] points = new Vector2[2];
        public float thickness = 3f;
        public Color lineColor;
        public SkillUpgradeSO data;

        private bool _isActive;

        public new bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                lineColor = _isActive ? Color.green : Color.white;
            }
        }

        public void FadeLine(float duration, bool isFadeOut)
        {
            UIUtility.FadeUI(gameObject, duration, isFadeOut);
        }
        
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (points.Length < 2) return;

            for (int i = 0; i < points.Length - 1; i++)
            {
                CreateLineSegment(points[i], points[i + 1], vh);

                int index = vh.currentVertCount - 5;

                vh.AddTriangle(index, index + 1, index + 3);
                vh.AddTriangle(index, index + 3, index + 2);

                if (i != 0)
                {
                    vh.AddTriangle(index, index - 1, index - 3);
                    vh.AddTriangle(index + 1, index - 1, index - 2);

                    CreateRoundJoin(
                        points[i], (points[i] - points[i - 1]).normalized, 
                        (points[i + 1] - points[i]).normalized, vh);
                }
            }
        }

        private void CreateLineSegment(Vector3 point1, Vector3 point2, VertexHelper vh)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = lineColor;
            
            Quaternion point1Rotation = Quaternion.Euler(0, 0, RotatePointToward(point1, point2) + 90f);
            vertex.position = point1Rotation * new Vector3(-thickness * 0.5f, 0f);
            vertex.position += point1;
            vh.AddVert(vertex);
            vertex.position = point1Rotation * new Vector3(thickness * 0.5f, 0f);
            vertex.position += point1;
            vh.AddVert(vertex);
            
            
            Quaternion point2Rotation = Quaternion.Euler(0, 0, RotatePointToward(point2, point1) - 90f);
            vertex.position = point2Rotation * new Vector3(-thickness * 0.5f, 0f);
            vertex.position += point2;
            vh.AddVert(vertex);
            vertex.position = point2Rotation * new Vector3(thickness * 0.5f, 0f);
            vertex.position += point2;
            vh.AddVert(vertex);
            
            vertex.position = point2;
            vh.AddVert(vertex); 
        }
        
        void CreateRoundJoin(Vector2 center, Vector2 fromDir, Vector2 toDir, VertexHelper vh, int segments = 6)
        {
            float angle = Vector2.SignedAngle(fromDir, toDir);
            float step = angle / segments;
            Vector2 startNormal = Vector2.Perpendicular(fromDir).normalized;
            int startIndex = vh.currentVertCount;

            UIVertex v = UIVertex.simpleVert;
            v.color = lineColor;
            v.position = center;
            vh.AddVert(v);

            for (int i = 0; i <= segments; i++)
            {
                float a = step * i;
                Vector2 dir = Quaternion.Euler(0, 0, a) * startNormal;
                v.position = center + dir * thickness * 0.5f;
                vh.AddVert(v);

                if (i > 0)
                    vh.AddTriangle(startIndex, startIndex + i, startIndex + i + 1);
            }
        }

        private float RotatePointToward(Vector2 point1, Vector2 point2)
            => Mathf.Atan2(point2.y - point1.y, point2.x - point1.x) * Mathf.Rad2Deg;
    }
}