using Chipmunk.ComponentContainers;
using Code.ETC;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Scripts.Effects
{
    public class TrajectoryDrawer : MonoBehaviour, IContainerComponent
    {
        [SerializeField] private LineRenderer line;
        [SerializeField] private int segments = 40;
        [SerializeField] private float maxTime = 2.0f;
        [SerializeField] private LayerMask hitMask = ~0;
        private IAimProvider _aimProvider;
        public ComponentContainer ComponentContainer { get; set; }
        public void OnInitialize(ComponentContainer componentContainer)
        {
            _aimProvider = componentContainer.GetSubclassComponent<IAimProvider>();
        }
        private void Update()
        {
            Draw(transform.position, _aimProvider.GetAimPosition());
        }
        public void Draw(Vector3 start, Vector3 v0, float gravity = 9.81f)
        {
            if (!line) return;

            line.positionCount = segments + 1;
            line.SetPosition(0, start);

            Vector3 prev = start;
            float dt = maxTime / segments;

            for (int i = 1; i <= segments; i++)
            {
                float t = dt * i;
                Vector3 p = start + v0 * t + Vector3.down * 0.5f * gravity * t * t;

                // 충돌하면 그 지점에서 라인 끊기
                if (Physics.Linecast(prev, p, out var hit, hitMask, QueryTriggerInteraction.Ignore))
                {
                    line.positionCount = i + 1;
                    line.SetPosition(i, hit.point);
                    return;
                }

                line.SetPosition(i, p);
                prev = p;
            }
        }


    }
}
