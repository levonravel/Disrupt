using UnityEngine;

namespace RavelTek.Disrupt
{
    public abstract class Singleton<T> : NetHelper where T : Singleton<T>
    {
        protected static T instance;
        bool initialized = false;
        public static T Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    Debug.LogError("DisruptManager missing in scene..");
                    return null;
                }
                return instance;
            }
        }
        static T Create()
        {
            var singleton = new GameObject(typeof(T).Name);
            singleton.AddComponent<T>();
            return singleton as T;
        }
        void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                if (initialized) return;
                initialized = true;
                DontDestroyOnLoad(gameObject);
                OnAwake();
            }
            else if (instance != this) Destroy(gameObject);
        }
        void OnDestroy()
        {
            if (instance == this as T) instance = null;
            OnDestroyed();
        }
        protected virtual void OnAwake() { }
        protected virtual void OnDestroyed() { }
    }
}