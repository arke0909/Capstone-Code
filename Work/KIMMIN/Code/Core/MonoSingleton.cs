using UnityEngine;

namespace Work.Code.Core
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour 
    {
        private static T _instance;
        private static readonly object _lock = new();

        public static bool HasInstance => _instance != null;

        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;

                lock (_lock)
                {
                    if (_instance != null) return _instance;

                    _instance = FindAnyObjectByType<T>();

                    if (_instance == null)
                    {
                        Debug.LogError($"[MonoSingleton] No instance of {typeof(T)} found in scene.");
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[MonoSingleton] Duplicate instance of {typeof(T)} detected. Destroying {gameObject.name}.");
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _instance = null;
        }
    }
}