using System;
using YooAsset;
using HybridCLR;
using UnityEngine;
using Framework.Utils;
using Newtonsoft.Json;
using System.Reflection;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Framework.Manager
{
    public class ResourceManager : SingletonInstance<ResourceManager>, ISingleton
    {
        private class ResourceCache
        {
            class CacheNode
            {
                public string key;
                public UnityEngine.Object asset;
                public LinkedListNode<CacheNode> nodeInCacheList;
            }
            
            private int mSize;
            private LinkedList<CacheNode> mCacheList;
            private Dictionary<string, CacheNode> mCacheMap;

            public ResourceCache(int size)
            {
                mSize = size;
                mCacheList = new LinkedList<CacheNode>();
                mCacheMap = new Dictionary<string, CacheNode>();
            }

            public UnityEngine.Object Get(string path)
            {
                CacheNode cacheNode;
                var inCache = mCacheMap.TryGetValue(path, out cacheNode);
                if (!inCache)
                    return null;
                mCacheList.Remove(cacheNode.nodeInCacheList);
                mCacheList.AddFirst(cacheNode.nodeInCacheList);
                return cacheNode.asset;
            }

            public void Insert(string path, UnityEngine.Object asset)
            {
                if (mCacheList.Count >= mSize)
                {
                    var nodeKickOut = mCacheList.Last;
                    mCacheMap.Remove(nodeKickOut.Value.key);
                    mCacheList.Remove(nodeKickOut);
                }
                
                var cacheNode = new CacheNode();
                var nodeInList = new LinkedListNode<CacheNode>(cacheNode);
                cacheNode.key = path;
                cacheNode.asset = asset;
                cacheNode.nodeInCacheList = nodeInList;

                mCacheMap[cacheNode.key] = cacheNode;
                mCacheList.AddFirst(nodeInList);
            }

            public void Clear()
            {
                mCacheMap.Clear();
                mCacheList.Clear();
            }
        }
        
        
        /// <summary>
        /// 资源缓存
        /// </summary>
        private ResourceCache mCacheResources;
        
        /// <summary>
        /// AssetHandle缓存
        /// </summary>
        private Dictionary<string, AssetHandle> m_AssetHandleCache;
        
        void ISingleton.OnCreate(object createParam)
        {
            mCacheResources = new ResourceCache(100);
            m_AssetHandleCache = new Dictionary<string, AssetHandle>();
        }
        
        void ISingleton.OnDestroy()
        {
            
        }
        
        void ISingleton.OnUpdate()
        {
            
        }
        
        
        /// <summary>
        /// 从缓存中取出预制体
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private async UniTask<GameObject> GetPrefabCache(string key)
        {
            if (!m_AssetHandleCache.ContainsKey(key))
            {
                return null;
            }
            var cacheHandle = m_AssetHandleCache[key];
            m_AssetHandleCache.Remove(key);
            await cacheHandle.InstantiateAsync();
            var prefab = cacheHandle.InstantiateAsync();
            await prefab.ToUniTask();
            return prefab.Result;
        }
        
        
        /// <summary>
        /// 从Resource加载
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public UnityEngine.Object ResourceLoad(string path)
        {
            var asset = mCacheResources.Get(path);
            if (asset == null)
            {
                asset = Resources.Load(path);
                if (asset != null)
                    mCacheResources.Insert(path, asset);
                else
                    Debug.LogWarning($"ResourceLoad: {path} Error!");
            }
            return asset;
        }
        
        
        /// <summary>
        /// 清空资AssetHandle缓存 & Resource缓存
        /// </summary>
        public void ClearCache()
        {
            foreach (var assetHandle in m_AssetHandleCache)
            {
                assetHandle.Value.Dispose();
            }
            m_AssetHandleCache.Clear();
            YooAssets.TryGetPackage(GameConfig.GameHotUpdatePackageName)?.UnloadUnusedAssets();
            mCacheResources.Clear();
            Resources.UnloadUnusedAssets();
        }

        
        /// <summary>
        /// 从热更包中加载AOT元数据DLL
        /// </summary>
        /// <param name="hotUpdatePackageName"></param>
        public async UniTask LoadMetadataForAOTAssemblies(string hotUpdatePackageName)
        {
            Debug.Log($"LoadMetadataForAOTAssemblies PackName:{hotUpdatePackageName}");
            var package = YooAssets.TryGetPackage(hotUpdatePackageName);
            if (package == null)
            {
                Debug.LogError("LoadMetadataForAOTAssemblies Get Package Error!");
            }
            else
            {
                var handle = package.LoadAssetAsync("AOTDLLList");
                await handle.ToUniTask();
                var asset = handle.AssetObject as TextAsset;
                if (asset == null)
                {
                    Debug.LogError("LoadMetadataForAOTAssemblies AOTDLLList Get Error!");
                }
                else
                {
                    const HomologousImageMode mode = HomologousImageMode.SuperSet;
                    var dllNames = JsonConvert.DeserializeObject<List<string>>(asset.text);
                    foreach (var name in dllNames)
                    {
                        var dataHandle = package.LoadAssetAsync(name);
                        await dataHandle.ToUniTask();
                        var dllAsset = dataHandle.AssetObject as TextAsset;
                        if (dllAsset == null)
                        {
                            Debug.LogError($"LoadMetadataForAOTAssemblies DLL {name} Load Error!");
                            continue;
                        }
                        var resultCode = RuntimeApi.LoadMetadataForAOTAssembly(dllAsset.bytes, mode);
                        Debug.Log($"LoadMetadataForAOTAssemblies Load:{name} Mode:{mode} ResultCode:{resultCode}");
                        dataHandle.Dispose();
                    }
                    
                    handle.Dispose();
                    package.UnloadUnusedAssets();
                }
            }
        }

        
        /// <summary>
        /// 从热更包中加载热更DLL
        /// </summary>
        /// <param name="hotUpdatePackageName"></param>
        public async UniTask LoadHotUpdateAssemblies(string hotUpdatePackageName)
        {
            Debug.Log($"LoadHotUpdateAssemblies PackName:{hotUpdatePackageName}");
            var package = YooAssets.TryGetPackage(hotUpdatePackageName);
            if (package == null)
            {
                Debug.LogError("LoadHotUpdateAssemblies Get Package Error!");
            }
            else
            {
                var handle = package.LoadAssetAsync("HotUpdateDLLList");
                await handle.ToUniTask();
                var asset = handle.AssetObject as TextAsset;
                if (asset == null)
                {
                    Debug.LogError("LoadHotUpdateAssemblies HotUpdateDLLList Get Error!");
                }
                else
                {
                    var dllNames = JsonConvert.DeserializeObject<List<string>>(asset.text);
                    foreach (var dllName in dllNames)
                    {
                        var dataHandle = package.LoadAssetAsync(dllName);
                        await dataHandle.ToUniTask();
                        var dllAsset = dataHandle.AssetObject as TextAsset;
                        if (dllAsset == null)
                        {
                            Debug.LogError($"LoadHotUpdateAssemblies Get {dllName} DLL Data Error!");
                            continue;
                        }
                        Assembly.Load(dllAsset.bytes);
                        Debug.Log($"LoadHotUpdateAssemblies LoadHotUpdate DLL:{dllName}");
                        dataHandle.Dispose();
                    }
                    
                    handle.Dispose();
                    package.UnloadUnusedAssets();
                }
            }
        }
        
        
        /// <summary>
        /// 从热更包中异步加载资源
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="onFinishCallback"></param>
        /// <returns></returns>
        public async UniTaskVoid LoadAssetsAsync(string assetPath, Action<AssetHandle> onFinishCallback = null)
        {
            Debug.Log($"LoadAssetsAsync AssetPath:{assetPath}");
            var package = YooAssets.TryGetPackage(GameConfig.GameHotUpdatePackageName);
            if (package == null)
            {
                Debug.LogError($"LoadAssetsAsync Get PackageName:{GameConfig.GameHotUpdatePackageName} Error!");
                return;
            }
            var loadHandle = YooAssets.LoadAssetAsync(assetPath);
            await loadHandle.ToUniTask();
            if (loadHandle.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"LoadAssetsAsync: {assetPath} Error Status:{loadHandle.Status}");
                return;
            }
            onFinishCallback?.Invoke(loadHandle);
        }
        
        
        /// <summary>
        /// 异步加载Json
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="onFinishCallback"></param>
        public void LoadJsonAsync(string filename, Action<JSONObject> onFinishCallback = null)
        {
            LoadAssetsAsync($"{GameConfig.PackJsonTextPath}/{filename}", (AssetHandle loadHandle) =>
            {
                if (loadHandle.Status != EOperationStatus.Succeed)
                {
                    Debug.LogError($"LoadJsonTextAsync: {filename} Load Error Status:{loadHandle.Status}");
                    return;
                }
                var textAsset = loadHandle.GetAssetObject<TextAsset>();
                if (textAsset == null)
                {
                    Debug.LogError($"LoadJsonTextAsync: {filename} TextAsset Null Error!");
                    return;
                }
                onFinishCallback?.Invoke(new JSONObject(textAsset.text));
            });
        }

        
        /// <summary>
        /// 异步加载文本
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="onFinishCallback"></param>
        public void LoadJsonTextAsync(string filename, Action<string> onFinishCallback = null)
        {
            LoadAssetsAsync($"{GameConfig.PackDataTextPath}/{filename}", (AssetHandle loadHandle) =>
            {
                if (loadHandle.Status != EOperationStatus.Succeed)
                {
                    Debug.LogError($"LoadJsonTextAsync: {filename} Load Error Status:{loadHandle.Status}");
                    return;
                }
                var textAsset = loadHandle.GetAssetObject<TextAsset>();
                onFinishCallback?.Invoke(textAsset == null ? string.Empty : textAsset.text);
            });
        }

        
        /// <summary>
        /// 异步加载全部配置表文件
        /// </summary>
        /// <param name="onFinishCallback"></param>
        public void LoadAllDataFilesAsync(Action<string[]> onFinishCallback = null)
        {
            LoadAssetsAsync($"{GameConfig.PackDataTextPath}/AllDataList.txt", (AssetHandle loadHandle) =>
            {
                if (loadHandle.Status != EOperationStatus.Succeed)
                {
                    Debug.LogError($"LoadAllDataFilesAsync Load Error Status:{loadHandle.Status}");
                    return;
                }
                var textAsset = loadHandle.GetAssetObject<TextAsset>();
                if (textAsset == null)
                {
                    Debug.LogError($"LoadAllDataFilesAsync Load TextAsset Null Error!");
                    return;
                }
                var strs = textAsset.text.Split(new string[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
                onFinishCallback?.Invoke(strs);
            });
        }

        
        /// <summary>
        /// 异步加载配置表文件
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="onFinishCallback"></param>
        public void LoadDataFileAsync(string filename, Action<string[]> onFinishCallback = null)
        {
            LoadAssetsAsync($"{GameConfig.PackDataTextPath}/{filename}", (AssetHandle loadHandle) =>
            {
                if (loadHandle.Status != EOperationStatus.Succeed)
                {
                    Debug.LogError($"LoadAllTblNameAsync Load Error Status:{loadHandle.Status}");
                    return;
                }
                var textAsset = loadHandle.GetAssetObject<TextAsset>();
                if (textAsset == null)
                {
                    Debug.LogError($"LoadAllTblNameAsync Load TextAsset Null Error!");
                    return;
                }
                var strs = textAsset.text.Split(new string[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
                onFinishCallback?.Invoke(strs);
            });
        }

        
        /// <summary>
        /// 异步预加载预制体
        /// </summary>
        /// <param name="prefabName"></param>
        /// <param name="onFinishCallback"></param>
        public async UniTask PreLoadUGUIPrefabAsync(string prefabName, Action onFinishCallback = null)
        {
            var loadPath = $"{GameConfig.PackUguiPrefabPath}/{prefabName}";
            Debug.Log($"LoadUGUIPrefabAsync AssetPath:{loadPath}");
            if (m_AssetHandleCache.ContainsKey(loadPath))
            {
                onFinishCallback?.Invoke();
                return;
            }
            var package = YooAssets.TryGetPackage(GameConfig.GameHotUpdatePackageName);
            if (package == null)
            {
                Debug.LogError($"LoadAssetsAsync Get PackageName:{GameConfig.GameHotUpdatePackageName} Error!");
                return;
            }
            var loadHandle = YooAssets.LoadAssetAsync(loadPath);
            await loadHandle.ToUniTask();
            if (loadHandle.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"LoadAssetsAsync: {loadPath} Error Status:{loadHandle.Status}");
                return;
            }
            if (!m_AssetHandleCache.ContainsKey(loadPath))
            {
                m_AssetHandleCache.Add(loadPath, loadHandle);
            }
            Debug.LogError($"PreLoadUGUIPrefabAsync ToUniTask OK {loadPath}");
            onFinishCallback?.Invoke();
        }
        
        
        /// <summary>
        /// 异步加载预制体
        /// </summary>
        /// <param name="prefabName"></param>
        /// <param name="onFinishCallback"></param>
        public async UniTask LoadUGUIPrefabAsync(string prefabName, Action<GameObject> onFinishCallback = null)
        {
            var loadPath = $"{GameConfig.PackUguiPrefabPath}/{prefabName}";
            Debug.Log($"LoadUGUIPrefabAsync AssetPath:{loadPath}");
            var cachePre = await GetPrefabCache(loadPath);
            if (cachePre != null)
            {
                onFinishCallback?.Invoke(cachePre);
                return;
            }
            var package = YooAssets.TryGetPackage(GameConfig.GameHotUpdatePackageName);
            if (package == null)
            {
                Debug.LogError($"LoadAssetsAsync Get PackageName:{GameConfig.GameHotUpdatePackageName} Error!");
                return;
            }
            var loadHandle = YooAssets.LoadAssetAsync(loadPath);
            await loadHandle.ToUniTask();
            if (loadHandle.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"LoadAssetsAsync: {loadPath} Error Status:{loadHandle.Status}");
                return;
            }
            if (!m_AssetHandleCache.ContainsKey(loadPath))
            {
                m_AssetHandleCache.Add(loadPath, loadHandle);
            }
            var prefab = loadHandle.InstantiateSync();
            if (prefab == null)
            {
                Debug.LogError($"LoadUGUIPrefabAsync InstantiateSync {loadPath} Error!");
            }
            onFinishCallback?.Invoke(prefab);
        }

        
        /// <summary>
        /// 异步加载Sprite
        /// </summary>
        /// <param name="spriteName"></param>
        /// <param name="onFinishCallback"></param>
        public void LoadSpriteAsync(string spriteName, Action<Sprite> onFinishCallback = null)
        {
            LoadAssetsAsync(spriteName, (AssetHandle loadHandle) =>
            {
                if (loadHandle.Status != EOperationStatus.Succeed)
                {
                    Debug.LogError($"LoadSpriteAsync: {spriteName} Error Status:{loadHandle.Status}");
                    return;
                }
                var sp = loadHandle.AssetObject as Sprite;
                if (sp == null)
                {
                    var texture = loadHandle.AssetObject as Texture2D;
                    if (texture != null)
                        sp = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                }
                onFinishCallback?.Invoke(sp);
            });
        }
        
        
        /// <summary>
        /// 异步加载视频文件
        /// </summary>
        /// <param name="vcName"></param>
        /// <param name="onFinishCallback"></param>
        public void LoadVideoClipAsync(string vcName, Action<UnityEngine.Video.VideoClip> onFinishCallback = null)
        {
            LoadAssetsAsync(vcName, (AssetHandle loadHandle) =>
            {
                if (loadHandle.Status != EOperationStatus.Succeed)
                {
                    Debug.LogError($"LoadVideoClipAsync: {vcName} Error Status:{loadHandle.Status}");
                    return;
                }
                var videoClip = loadHandle.GetAssetObject<UnityEngine.Video.VideoClip>();
                onFinishCallback?.Invoke(videoClip);
            });
        }

        
        /// <summary>
        /// 异步加载Sprite (初始化锚点在中心)
        /// </summary>
        /// <param name="spriteName"></param>
        /// <param name="onFinishCallback"></param>
        public void LoadSpriteAsyncWithPivot(string spriteName, Action<Sprite> onFinishCallback = null)
        {
            LoadAssetsAsync(spriteName, (AssetHandle loadHandle) =>
            {
                if (loadHandle.Status != EOperationStatus.Succeed)
                {
                    Debug.LogError($"LoadSpriteAsyncWithPivot: {spriteName} Error Status:{loadHandle.Status}");
                    return;
                }
                var texture = loadHandle.GetAssetObject<Texture2D>();
                if (texture == null)
                {
                    Debug.LogError($"LoadSpriteAsyncWithPivot: {spriteName} Texture2D Null Error!");
                    return;
                }
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),new Vector2(0.5f,0.5f));
                onFinishCallback?.Invoke(sprite);
            });
        }

        
        /// <summary>
        /// 用于加载包含border信息的sprite
        /// </summary>
        /// <param name="spriteName"></param>
        /// <param name="border"></param>
        /// <param name="onFinishCallback"></param>
        public void LoadSpriteWithBorderAsync(string spriteName, Vector4 border, Action<Sprite> onFinishCallback = null)
        {
            LoadAssetsAsync(spriteName, (AssetHandle loadHandle) =>
            {
                if (loadHandle.Status != EOperationStatus.Succeed)
                {
                    Debug.LogError($"LoadSpriteWithBorderAsync: {spriteName} Error Status:{loadHandle.Status}");
                    return;
                }
                var texture = loadHandle.GetAssetObject<Texture2D>();
                if (texture == null)
                {
                    Debug.LogError($"LoadSpriteWithBorderAsync: {spriteName} Texture2D Null Error!");
                    return;
                }
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 100, 1, SpriteMeshType.Tight, border);
                onFinishCallback?.Invoke(sprite);
            });
        }

        
        /// <summary>
        /// 异步加载Texture
        /// </summary>
        /// <param name="spriteName"></param>
        /// <param name="onFinishCallback"></param>
        public void LoadTextureAsync(string spriteName, Action<Texture> onFinishCallback = null)
        {
            LoadAssetsAsync(spriteName, (AssetHandle loadHandle) =>
            {
                if (loadHandle.Status != EOperationStatus.Succeed)
                {
                    Debug.LogError($"LoadTextureAsync: {spriteName} Error Status:{loadHandle.Status}");
                    return;
                }
                var texture = loadHandle.GetAssetObject<Texture2D>();
                onFinishCallback?.Invoke(texture);
            });
        }

        
        /// <summary>
        /// 异步加载Sprite图集
        /// </summary>
        /// <param name="spriteName"></param>
        /// <param name="onFinishCallback"></param>
        public void LoadSpriteAtlasAsync(string spriteName, Action<UnityEngine.U2D.SpriteAtlas> onFinishCallback = null)
        {
            LoadAssetsAsync(spriteName, (AssetHandle loadHandle) =>
            {
                if (loadHandle.Status != EOperationStatus.Succeed)
                {
                    Debug.LogError($"LoadSpriteAtlasAsync: {spriteName} Error Status:{loadHandle.Status}");
                    return;
                }
                var spriteAtlas = loadHandle.GetAssetObject<UnityEngine.U2D.SpriteAtlas>();
                onFinishCallback?.Invoke(spriteAtlas);
            });
        }
        
        
        /// <summary>
        /// 异步加载Material (UI的材质专用加载)
        /// </summary>
        /// <param name="materialName"></param>
        /// <param name="onFinishCallback"></param>
        public void LoadMaterialAsync(string materialName, Action<Material> onFinishCallback = null)
        {
            LoadAssetsAsync(materialName, (AssetHandle loadHandle) =>
            {
                if (loadHandle.Status != EOperationStatus.Succeed)
                {
                    Debug.LogError($"LoadMaterialAsync: {materialName} Error Status:{loadHandle.Status}");
                    return;
                }
                var material = loadHandle.GetAssetObject<Material>();
                onFinishCallback?.Invoke(material);
            });
        }
    }
}