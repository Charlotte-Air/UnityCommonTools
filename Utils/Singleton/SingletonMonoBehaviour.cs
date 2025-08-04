using System;
using UnityEngine;

namespace Framework.Utils
{
    [ExecuteInEditMode]
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private bool _isRelease;
        private static T _instance;
        private static bool _applicatinIsQuiting;

        public static bool HasInstance => _instance != null;

        public bool IsRelease() => _isRelease;

        public static string InstanceName
        {
            get
            {
                var str = typeof(T).ToString();
                var num = str.LastIndexOf(".", StringComparison.Ordinal);
                if (num > 0)
                    str = str.Substring(num + 1);
                return string.Format("!_{0}", str);
            }
        }

        public static T Instance
        {
            get
            {
                if (_applicatinIsQuiting)
                {
                    Debug.LogWarning(string.Format("[Singleton] Instance {0} Already Destroyed On Application Quit.But You still Try To Accece it.", typeof(T)));
                    return default(T);
                }
                if (_instance == null)
                {
                    var gameObject = GameObjectCreator.New();
                    var str = typeof(T).ToString();
                    var num = str.LastIndexOf(".", StringComparison.Ordinal);
                    if (num > 0)
                        str = str.Substring(num + 1);
                    gameObject.name = string.Format("!_{0}", str);
                    var transform = gameObject.transform;
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                    transform.localScale = Vector3.one;
                    gameObject.AddComponent<T>();
                    if (Application.isEditor)
                        Debug.Log(string.Format("<color=green> Create Instance {0} </color>", str), _instance);
                }
                if (_instance != null && Application.isPlaying)
                    DontDestroyOnLoad(_instance.gameObject);
                return _instance;
            }
        }

        private void Awake()
        {
            if (!(_instance == null))
                return;
            _instance = this as T;
            OnSingletonInit();
        }

        protected abstract void OnSingletonInit();

        protected abstract void OnSingletonRelease();

        public void Release()
        {
            OnSingletonRelease();
            if (_instance != null && gameObject != null)
            {
                Destroy(gameObject);
                _instance = default(T);
            }
            _isRelease = true;
        }

        private void OnApplicationQuit()
        {
            
        }

        public void OnDestroy()
        {
            if (!_isRelease && Application.isEditor && !Application.isPlaying)
                Debug.Log(string.Format("<color=yellow>{0} is Destory But Not Call Release</color>", GetType()));
            if (!(_instance == this))
                return;
            _instance = default(T);
        }
    }
}