using System;
using UnityEngine;
using System.Collections;
using Object = System.Object;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Framework.Utils
{
	internal class SingletonDriver : MonoBehaviour
	{
		void Update()
		{
			Singleton.Update();
		}
	}
	
	public abstract class SingletonInstance<T> where T : class, ISingleton
	{
		private static T _instance;
		public static T Instance
		{
			get
			{
				if (_instance == null)
					Debug.LogError($"{typeof(T)} Is Not Create Use {nameof(Singleton)}.{nameof(Singleton.CreateSingleton)} Create.");
				return _instance;
			}
		}

		protected SingletonInstance()
		{
			if (_instance != null)
				throw new Exception($"{typeof(T)} Instance Already Created.");
			_instance = this as T;
		}
		
		protected void DestroyInstance()
		{
			_instance = null;
		}
	}
	
	public static class Singleton
	{
		private class Wrapper
		{
			public int Priority { private set; get; }
			public ISingleton Singleton { private set; get; }

			public Wrapper(ISingleton module, int priority)
			{
				Singleton = module;
				Priority = priority;
			}
		}

		private static bool _isDirty;
		private static bool _isInitialize;
		private static GameObject _driver;
		private static MonoBehaviour _behaviour;
		private static readonly List<Wrapper> _wrappers = new List<Wrapper>(100);

		/// <summary>
		/// 初始化单例系统
		/// </summary>
		public static void Initialize()
		{
			if (_isInitialize)
				throw new Exception($"{nameof(Singleton)} is Initialized !");

			if (_isInitialize == false)
			{
				// 创建驱动器
				_isInitialize = true;
				_driver = new GameObject($"[{nameof(Singleton)}]");
				_behaviour = _driver.AddComponent<SingletonDriver>();
				UnityEngine.Object.DontDestroyOnLoad(_driver);
				Debug.Log($"{nameof(Singleton)} initalize !");
			}
		}

		/// <summary>
		/// 销毁单例系统
		/// </summary>
		public static void Destroy()
		{
			if (!_isInitialize)
				return;
			
			DestroyAll();
			_isInitialize = false;
			if (_driver != null)
			{
				GameObject.Destroy(_driver);
			}
			Debug.Log($"{nameof(Singleton)} Destroy ALL!");
		}

		/// <summary>
		/// 更新单例系统
		/// </summary>
		internal static void Update()
		{
			// 如果需要重新排序
			if (_isDirty)
			{
				_isDirty = false;
				_wrappers.Sort((left, right) =>
				{
					if (left.Priority > right.Priority)
						return -1;
					if (left.Priority == right.Priority)
						return 0;
					return 1;
				});
			}
			
			// 轮询所有模块
			for (var i = 0; i < _wrappers.Count; i++)
			{
				_wrappers[i].Singleton.OnUpdate();
			}
		}

		/// <summary>
		/// 获取单例
		/// </summary>
		public static T GetSingleton<T>() where T : class, ISingleton
		{
			Type type = typeof(T);
			for (int i = 0; i < _wrappers.Count; i++)
			{
				if (_wrappers[i].Singleton.GetType() == type)
					return _wrappers[i].Singleton as T;
			}
			Debug.LogError($"Not found manager : {type}");
			return null;
		}

		/// <summary>
		/// 查询单例是否存在
		/// </summary>
		public static bool Contains<T>() where T : class, ISingleton
		{
			Type type = typeof(T);
			for (int i = 0; i < _wrappers.Count; i++)
			{
				if (_wrappers[i].Singleton.GetType() == type)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 创建单例
		/// </summary>
		/// <param name="priority">运行时的优先级，优先级越大越早执行。如果没有设置优先级，那么会按照添加顺序执行</param>
		public static T CreateSingleton<T>(int priority = 0) where T : class, ISingleton
		{
			return CreateSingleton<T>(null, priority);
		}

		/// <summary>
		/// 创建单例
		/// </summary>
		/// <param name="createParam">附加参数</param>
		/// <param name="priority">运行时的优先级，优先级越大越早执行。如果没有设置优先级，那么会按照添加顺序执行</param>
		public static T CreateSingleton<T>(Object createParam, int priority = 0) where T : class, ISingleton
		{
			if (priority < 0)
				throw new Exception("The priority can not be negative");

			if (Contains<T>())
				throw new Exception($"Module is already existed : {typeof(T)}");

			// 如果没有设置优先级
			if (priority == 0)
			{
				var minPriority = GetMinPriority();
				priority = --minPriority;
			}

			var module = Activator.CreateInstance<T>();
			var wrapper = new Wrapper(module, priority);
			wrapper.Singleton.OnCreate(createParam);
			_wrappers.Add(wrapper);
			_isDirty = true;
			return module;
		}

		/// <summary>
		/// 销毁单例
		/// </summary>
		public static bool DestroySingleton<T>() where T : class, ISingleton
		{
			var type = typeof(T);
			for (int i = 0; i < _wrappers.Count; i++)
			{
				if (_wrappers[i].Singleton.GetType() == type)
				{
					_wrappers[i].Singleton.OnDestroy();
					_wrappers.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 开启一个协程
		/// </summary>
		public static Coroutine StartCoroutine(IEnumerator coroutine)
		{
			return _behaviour.StartCoroutine(coroutine);
		}
		
		
		/// <summary>
		/// 启动一个协程
		/// </summary>
		/// <param name="methodName"></param>
		/// <returns></returns>
		public static Coroutine StartCoroutine(string methodName)
		{
			return _behaviour.StartCoroutine(methodName);
		}

		/// <summary>
		/// 停止一个协程
		/// </summary>
		public static void StopCoroutine(Coroutine coroutine)
		{
			_behaviour.StopCoroutine(coroutine);
		}
		
		/// <summary>
		/// 停止一个协程
		/// </summary>
		/// <param name="methodName"></param>
		public static void StopCoroutine(string methodName)
		{
			_behaviour.StopCoroutine(methodName);
		}

		/// <summary>
		/// 停止所有协程
		/// </summary>
		public static void StopAllCoroutines()
		{
			_behaviour.StopAllCoroutines();
		}

		private static int GetMinPriority()
		{
			var minPriority = 0;
			for (var i = 0; i < _wrappers.Count; i++)
			{
				if (_wrappers[i].Priority < minPriority)
					minPriority = _wrappers[i].Priority;
			}
			return minPriority; //小于等于零
		}
		
		private static void DestroyAll()
		{
			for (var i = 0; i < _wrappers.Count; i++)
			{
				_wrappers[i].Singleton.OnDestroy();
			}
			_wrappers.Clear();
		}
	}
}