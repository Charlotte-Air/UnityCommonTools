using System;
using System.IO;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;

#nullable disable
internal class LogWriter
{
  private static LogWriter __instance;
  private Thread thread;
  private Queue<string> logQueue;
  private Queue<DateTime> timeQueue;
  private Queue<LogType> typeQueue;
  private object rwlock = new object();
  private volatile bool runing;
  private FileStream fout;

  public static LogWriter instance
  {
    get
    {
      if (LogWriter.__instance == null)
        LogWriter.__instance = new LogWriter();
      return LogWriter.__instance;
    }
  }

  private LogWriter()
  {
  }

  public void Setup(string filename)
  {
    if (this.logQueue == null)
      this.logQueue = new Queue<string>();
    if (this.timeQueue == null)
      this.timeQueue = new Queue<DateTime>();
    if (this.typeQueue == null)
      this.typeQueue = new Queue<LogType>();
    if (this.fout == null)
      this.fout = new FileStream(filename, FileMode.Create);
    if (this.thread == null)
      this.thread = new Thread(new ThreadStart(this.DoWorkLoop));
    this.runing = true;
    this.thread.Start();
  }

  public void Shutdown()
  {
    this.runing = false;
    if (!this.thread.Join(1000))
      this.thread.Abort();
    this.thread = (Thread) null;
    this.fout = (FileStream) null;
    this.logQueue.Clear();
    this.typeQueue.Clear();
    this.timeQueue.Clear();
  }

  public void AppendLog(string log, LogType logType)
  {
    if (!this.runing)
      return;
    lock (this.rwlock)
    {
      this.logQueue.Enqueue(log);
      this.timeQueue.Enqueue(DateTime.Now);
      this.typeQueue.Enqueue(logType);
    }
  }

  private void DoWorkLoop()
  {
    StringBuilder stringBuilder = new StringBuilder();
    StreamWriter streamWriter = new StreamWriter((Stream) this.fout);
    int num1 = 16;
    int num2 = 100;
    int num3 = 20;
    int num4 = 0;
    DateTime dateTime = DateTime.Now;
    LogType logType = LogType.Log;
    while (this.runing)
    {
      long num5 = DateTime.Now.Ticks / 10000L;
      string str = (string) null;
      lock (this.rwlock)
      {
        num4 = this.logQueue.Count;
        if (num4 != 0)
        {
          str = this.logQueue.Dequeue();
          dateTime = this.timeQueue.Dequeue();
          logType = this.typeQueue.Dequeue();
        }
      }
      if (!string.IsNullOrEmpty(str))
      {
        stringBuilder.Append(dateTime.ToString());
        stringBuilder.Append("\t");
        switch (logType)
        {
          case LogType.Error:
            stringBuilder.Append("E");
            break;
          case LogType.Assert:
            stringBuilder.Append("A");
            break;
          case LogType.Warning:
            stringBuilder.Append("W");
            break;
          case LogType.Log:
            stringBuilder.Append("D");
            break;
          case LogType.Exception:
            stringBuilder.Append("X");
            break;
        }
        stringBuilder.Append("\t");
        stringBuilder.Append(str);
        stringBuilder.Append("\n");
        streamWriter.Write(stringBuilder.ToString());
        streamWriter.Flush();
        stringBuilder.Remove(0, stringBuilder.Length);
      }
      else
      {
        int num6 = (int) (DateTime.Now.Ticks / 10000L - num5);
        if (num6 < num1)
        {
          int num7 = num1 - num6;
          Thread.Sleep(num7 + (num2 - num7 - (int) ((double) (num2 - num7) / (double) num3 * (double) num4)));
          continue;
        }
      }
      Thread.Sleep(1);
    }
    if (stringBuilder.Length > 0)
    {
      streamWriter.Write(stringBuilder.ToString());
      streamWriter.Flush();
    }
    streamWriter.Close();
  }
}