using System;
using System.IO;
using UnityEngine;
using System.Text;
using System.IO.Compression;
using System.Collections.Generic;
using System.Security.Cryptography;

public class Uploader
{
  public void HttpPut() { }
  public volatile byte threadedFlag;
  public virtual int UploadFileThreaded(string file) => 0;
  public static long GetUnixTimeStamp() => (DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000L;
  public static string Encode_HMAC_SHA1(string input, byte[] key)
  {
    byte[] hash = new HMACSHA1(key).ComputeHash((Stream) new MemoryStream(Encoding.ASCII.GetBytes(input)));
    StringBuilder stringBuilder = new StringBuilder();
    foreach (byte num in hash)
      stringBuilder.AppendFormat("{0:x2}", (object) num);
    return stringBuilder.ToString();
  }
}

internal class LogUploader
{
  public class UploadTask
  {
    public bool finalDelete;
    public string filename;
    public Uploader uploader;
  }
  
  private static object taskLock = new object();
  private static Dictionary<string, LogUploader.UploadTask> tasks;
  private delegate void DoUploadTaskFunc(LogUploader.UploadTask task);

  public static void Upload(string filename, Uploader uploader, bool finalDelete = true)
  {
    LogUploader.DoUploadTaskFunc doUploadTaskFunc = new LogUploader.DoUploadTaskFunc(LogUploader.DoUploadTaskPipe);
    LogUploader.UploadTask task = new LogUploader.UploadTask();
    task.filename = filename;
    task.uploader = uploader;
    task.finalDelete = finalDelete;
    lock (LogUploader.taskLock)
    {
      if (LogUploader.tasks == null)
        LogUploader.tasks = new Dictionary<string, LogUploader.UploadTask>();
      LogUploader.tasks.Add(filename, task);
    }
    doUploadTaskFunc.BeginInvoke(task, (AsyncCallback) (ar => { }), (object) null);
  }

  public static void StopAll()
  {
    lock (LogUploader.taskLock)
    {
      if (LogUploader.tasks == null || LogUploader.tasks.Count == 0)
        return;
      foreach (LogUploader.UploadTask uploadTask in LogUploader.tasks.Values)
        uploadTask.uploader.threadedFlag = (byte) 0;
    }
  }

  private static void DoUploadTaskPipe(LogUploader.UploadTask task)
  {
    string str = Path.Combine(Path.GetDirectoryName(task.filename), Path.GetFileName(task.filename) + ".gz");
    if (LogUploader.CreateZip(task.filename, str))
    {
      if (task.uploader.UploadFileThreaded(str) == 0 || task.finalDelete)
        File.Delete(task.filename);
      File.Delete(str);
    }
    lock (LogUploader.taskLock)
      LogUploader.tasks.Remove(task.filename);
  }

  private static bool CreateZip(string filename_in, string filename_out)
  {
    try
    {
      using (FileStream fileStream1 = File.OpenRead(filename_in))
      {
        if (fileStream1.Length > (long) LogHelper.MaxLogBytesToUpload)
          fileStream1.Seek((long) -LogHelper.MaxLogBytesToUpload, SeekOrigin.End);
        using (FileStream fileStream2 = File.Create(filename_out))
        {
          using (GZipStream gzipStream = new GZipStream((Stream) fileStream2, CompressionMode.Compress))
          {
            byte[] buffer = new byte[4096];
            int count;
            while ((count = fileStream1.Read(buffer, 0, 4096)) > 0)
              gzipStream.Write(buffer, 0, count);
          }
        }
      }
    }
    catch (Exception ex)
    {
      Debug.LogException(ex);
      return false;
    }
    return true;
  }
  
}