using UnityEngine;

namespace Singleton.Base
{
    public abstract class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static object _lock = new object();
        private static bool _quitting = false;
        
        /// <summary>
        /// 子类决定：是否在切换场景时保留
        /// </summary>
        protected virtual bool IsPersistent => false;

        public static T Instance
        {
            get
            {
                if (_quitting)
                {
                    Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();

                        if (_instance == null)
                        {
                            GameObject singletonObj = new GameObject(typeof(T).Name + " (Singleton)");
                            _instance = singletonObj.AddComponent<T>();
                            //DontDestroyOnLoad(singletonObj);
                            if ((_instance as SingletonMono<T>).IsPersistent)
                            {
                                DontDestroyOnLoad(singletonObj);
                            }
                        }
                    }

                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this as T;

            if (IsPersistent)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        
        public static bool TryGetInstance(out T instance)
        {
            instance = _instance;
            return instance != null;
        }


        protected virtual void OnApplicationQuit()
        {
            _quitting = true;
        }
    }
}