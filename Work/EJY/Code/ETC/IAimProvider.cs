using UnityEngine;

namespace Code.ETC
{
    public interface IAimProvider
    {
        Vector3 GetAimPosition();
        Vector3 GetAimPosition(float planeY);
        Vector3 GetWorldAimPosition();
    }
}
