using UnityEngine;

namespace Charlotte.Client.Framework
{
    [ExecuteInEditMode]
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _applicatinIsQuiting;
        private bool _isRelease = false;

        public static bool HasInstance => (Object)SingletonMonoBehaviour<T>._instance != (Object)null;

        public bool IsRelease() => this._isRelease;

        public static string InstanceName
        {
            get
            {
                string str = typeof(T).ToString();
                int num = str.LastIndexOf(".");
                if (num > 0)
                    str = str.Substring(num + 1);
                return string.Format("!_{0}", (object)str);
            }
        }

        public static T Instance
        {
            get
            {
                if (SingletonMonoBehaviour<T>._applicatinIsQuiting)
                {
                    Debug.LogWarning((object)string.Format("[Singleton] Instance {0} already destroyed on application quit.But you still try to accece it.", (object)typeof(T)));
                    return default(T);
                }

                if ((Object)SingletonMonoBehaviour<T>._instance == (Object)null)
                {
                    GameObject gameObject = GameObjectCreator.New();
                    string str = typeof(T).ToString();
                    int num = str.LastIndexOf(".");
                    if (num > 0)
                        str = str.Substring(num + 1);
                    gameObject.name = string.Format("!_{0}", (object)str);
                    Transform transform = gameObject.transform;
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                    transform.localScale = Vector3.one;
                    gameObject.AddComponent<T>();
                    if (Application.isEditor)
                        Debug.Log((object)string.Format("<color=green> Create Instance {0} </color>", (object)str), (Object)SingletonMonoBehaviour<T>._instance);
                }

                if ((Object)SingletonMonoBehaviour<T>._instance != (Object)null && Application.isPlaying)
                    Object.DontDestroyOnLoad((Object)SingletonMonoBehaviour<T>._instance.gameObject);
                return SingletonMonoBehaviour<T>._instance;
            }
        }

        private void Awake()
        {
            if (!((Object)SingletonMonoBehaviour<T>._instance == (Object)null))
                return;
            SingletonMonoBehaviour<T>._instance = this as T;
            this.OnSingletonInit();
        }

        protected abstract void OnSingletonInit();

        protected abstract void OnSingletonRelease();

        public void Release()
        {
            this.OnSingletonRelease();
            if ((Object)SingletonMonoBehaviour<T>._instance != (Object)null && (Object)this.gameObject != (Object)null)
            {
                Object.Destroy((Object)this.gameObject);
                SingletonMonoBehaviour<T>._instance = default(T);
            }

            this._isRelease = true;
        }

        private void OnApplicationQuit()
        {
        }

        public void OnDestroy()
        {
            if (!this._isRelease && Application.isEditor && !Application.isPlaying)
                Debug.LogError((object)string.Format("<color=yellow>{0} is destory but not call Release</color>",
                    (object)((object)this).GetType().ToString()));
            if (!((Object)SingletonMonoBehaviour<T>._instance == (Object)this))
                return;
            SingletonMonoBehaviour<T>._instance = default(T);
        }
    }
}