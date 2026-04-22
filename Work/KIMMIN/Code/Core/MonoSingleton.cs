using UnityEngine;

namespace Work.Code.Core
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour 
    {
        private static T _instance;
        public static bool HasInstance => _instance != null;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<T>();
                }

                Debug.Assert(_instance != null, "FindAnyObjectByType<T>() != null");
                return _instance;
            }
        }
    }
}