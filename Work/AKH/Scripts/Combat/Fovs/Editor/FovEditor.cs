using Scripts.Combat.Fovs;
using UnityEditor;
using UnityEngine;

namespace Scripts.Fovs.Editor
{
    [CustomEditor(typeof(FovCompo))]
    public class FOVEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var pFOV = (FovCompo)target;
            var pos = pFOV.transform.position;
            foreach (var item in pFOV.fovInfos)
            {

                Handles.color = Color.white;
                Handles.DrawWireArc(pos, Vector3.up, Vector3.forward, 360f, item.viewRadius);
                Vector3 viewAngleA = pFOV.DirFromAngle(-item.viewAngle * 0.5f, false);
                Vector3 viewAngleB = pFOV.DirFromAngle(item.viewAngle * 0.5f, false);

                Handles.DrawLine(pos, pos + viewAngleA * item.viewRadius);
                Handles.DrawLine(pos, pos + viewAngleB * item.viewRadius);
                Handles.color = Color.red;

                foreach (var trm in pFOV.visibleTargets)
                {
                    Vector3 targetPos = trm.position;
                    targetPos.y = pos.y;
                    Handles.DrawLine(pos, targetPos);
                }
            }

        }
    }

}
