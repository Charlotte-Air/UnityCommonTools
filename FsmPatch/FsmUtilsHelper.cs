using System;
using YooAsset;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 资源文件查询服务类
/// </summary>
public class GameQueryServices : IBuildinQueryServices
{
    /// <summary>
    /// 查询内置文件的时候，是否比对文件哈希值
    /// </summary>
    public static bool CompareFileCRC = false;

    /// <summary>
    /// 内置资源查询
    /// </summary>
    /// <param name="packageName"></param>
    /// <param name="fileName">注意：fileName包含文件格式</param>
    /// <param name="fileCRC"></param>
    /// <returns></returns>
    public bool Query(string packageName, string fileName, string fileCRC)
    {
        return StreamingAssetsHelper.FileExists(packageName, fileName, fileCRC);
    }
}


/// <summary>
/// 内置资源清单
/// </summary>
public class BuildinFileManifest : ScriptableObject
{
    [Serializable]
    public class Element
    {
        public string PackageName;
        public string FileName;
        public string FileCRC32;
    }

    public List<Element> BuildinFiles = new List<Element>();
}


/// <summary>
/// 内置资源清单管理器
/// </summary>
public sealed class StreamingAssetsHelper
{
    private class PackageQuery
    {
        public readonly Dictionary<string, BuildinFileManifest.Element> Elements = new Dictionary<string, BuildinFileManifest.Element>(1000);
    }
    
    private static bool _isInit = false;
    private static readonly Dictionary<string, PackageQuery> _packages = new Dictionary<string, PackageQuery>(10);

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        if (_isInit)
            return;
        
        _isInit = true;
        var manifest = Resources.Load<BuildinFileManifest>("BuildinFileManifest");
        if (manifest == null)
            return;
        foreach (var element in manifest.BuildinFiles)
        {
            if (_packages.TryGetValue(element.PackageName, out var package) == false)
            {
                package = new PackageQuery();
                _packages.Add(element.PackageName, package);
            }
            
            package.Elements.Add(element.FileName, element);
        }
    }

    /// <summary>
    /// 内置文件查询方法
    /// </summary>
    public static bool FileExists(string packageName, string fileName, string fileCRC32)
    {
        if (_isInit == false)
            Init();

        if (_packages.TryGetValue(packageName, out var package) == false)
            return false;

        if (package.Elements.TryGetValue(fileName, out var element) == false)
            return false;

        if (GameQueryServices.CompareFileCRC)
            return element.FileCRC32 == fileCRC32;
        else
            return true;
    }
}


/// <summary>
/// 服务器资源地址查询服务类
/// </summary>
public class RemoteServices : IRemoteServices
{
    private readonly string _defaultHostServer;
    private readonly string _fallbackHostServer;

    public RemoteServices(string defaultHostServer, string fallbackHostServer)
    {
        _defaultHostServer = defaultHostServer;
        _fallbackHostServer = fallbackHostServer;
    }

    string IRemoteServices.GetRemoteMainURL(string fileName)
    {
        return $"{_defaultHostServer}/{fileName}";
    }

    string IRemoteServices.GetRemoteFallbackURL(string fileName)
    {
        return $"{_fallbackHostServer}/{fileName}";
    }
}


/// <summary>
/// 资源文件构建流加载解密类
/// </summary>
public class FileStreamDecryption : IDecryptionServices
{
    /// <summary>
    /// 同步方式获取解密的资源包对象
    /// 注意：加载流对象在资源包对象释放的时候会自动释放
    /// </summary>
    AssetBundle IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
    {
        var bundleStream = new BundleStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        managedStream = bundleStream;
        return AssetBundle.LoadFromStream(bundleStream, fileInfo.ConentCRC, GetManagedReadBufferSize());
    }

    /// <summary>
    /// 异步方式获取解密的资源包对象
    /// 注意：加载流对象在资源包对象释放的时候会自动释放
    /// </summary>
    AssetBundleCreateRequest IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo, out Stream managedStream)
    {
        var bundleStream = new BundleStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        managedStream = bundleStream;
        return AssetBundle.LoadFromStreamAsync(bundleStream, fileInfo.ConentCRC, GetManagedReadBufferSize());
    }

    private static uint GetManagedReadBufferSize()
    {
        return 1024;
    }
}


/// <summary>
/// 资源文件构建偏移加载解密类
/// </summary>
public class FileOffsetDecryption : IDecryptionServices
{
    /// <summary>
    /// 同步方式获取解密的资源包对象
    /// 注意：加载流对象在资源包对象释放的时候会自动释放
    /// </summary>
    AssetBundle IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
    {
        managedStream = null;
        return AssetBundle.LoadFromFile(fileInfo.FileLoadPath, fileInfo.ConentCRC, GetFileOffset());
    }

    /// <summary>
    /// 异步方式获取解密的资源包对象
    /// 注意：加载流对象在资源包对象释放的时候会自动释放
    /// </summary>
    AssetBundleCreateRequest IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo, out Stream managedStream)
    {
        managedStream = null;
        return AssetBundle.LoadFromFileAsync(fileInfo.FileLoadPath, fileInfo.ConentCRC, GetFileOffset());
    }

    private static ulong GetFileOffset()
    {
        return 32;
    }
}


/// <summary>
/// 资源文件构建解密流
/// </summary>
public class BundleStream : FileStream
{
    public const byte KEY = 64;
    
    public BundleStream(string path, FileMode mode, FileAccess access, FileShare share) : base(path, mode, access, share)
    {
        
    }
    
    public BundleStream(string path, FileMode mode) : base(path, mode)
    {
        
    }

    public override int Read(byte[] array, int offset, int count)
    {
        var index = base.Read(array, offset, count);
        for (var i = 0; i < array.Length; i++)
        {
            array[i] ^= KEY;
        }
        return index;
    }
}