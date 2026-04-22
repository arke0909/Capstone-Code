using UnityEngine;

namespace Code.Combat
{
    public interface IPullable
    {
        public void Pull(Vector3 pullOffset);
    }
}