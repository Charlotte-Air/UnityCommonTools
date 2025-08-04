using System;
using UnityEngine;
using System.Reflection;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using Framework.Net;
using Framework.Utils;
using System.Collections.Generic;
using System.Collections.Concurrent;

public class NetInfo
{
    public ValueCallBack<Message>  Customer = new ValueCallBack<Message>();
}

public class Networkmanager : SingletonInstance<Networkmanager>, ISingleton
{
    public int Port { private set; get; }
    public string Host { private set; get; }
    public float HandMsgTime { get; private set; } // 上次接收消失时间
    public int ResCode { get; private set; } //收到的消息计数 和服务器对不上则应该断线重连
    
    private NetChannel channel { get; set; }
    private NetInfo m_mainNetInfo = new NetInfo();
    private List<int> ignoreCodeList = new List<int>();
    private ConcurrentQueue<Message> msgQueue = new ConcurrentQueue<Message>();
    private float m_UpdateMaxTime = 0.06f; //每一帧最大的派发事件时间，超过这个时间则停止派发，等到下一帧再派发 
    
    void ISingleton.OnCreate(object createParam)
    {

    }
    
    void ISingleton.OnDestroy()
    {

    }
    
    void ISingleton.OnUpdate()
    {
        var curTime = Time.realtimeSinceStartup;
        var endTime = curTime + m_UpdateMaxTime;
        while (curTime < endTime)
        {
            if (msgQueue.IsEmpty)
                return;
            if (!msgQueue.TryDequeue(out var msg))
                return;
            if (msg == null)
                return;
            
            try
            {
                m_mainNetInfo.Customer.Dispatch(msg.MsgId, msg);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            
            curTime = Time.realtimeSinceStartup;
            if (!ignoreCodeList.Contains(msg.MsgId))
                ResCode++;
        }
    }
    
    public void AddCustomNetListener(int netCustomId, ValueCallBack<Message>.FnCallBack callback, object context = null)
    {
        m_mainNetInfo.Customer.RegistListener(netCustomId, callback, context);
    }
    public void RemoveCustomNetListener(int netCustomId, ValueCallBack<Message>.FnCallBack callback)
    {
        m_mainNetInfo.Customer.RemoveListener(netCustomId, callback);
    }
    
    public int UniId { private set; get; } = 200;
    
    public void SendTestMsg(Message msg)
    {
        msg.UniId = UniId++;
        Send(msg);
    }
    
    public void Send(Message msg)
    {
        channel?.Write(msg);
    }

    public async UniTask<bool> Connect(string host, int port, int timeOut = 5000)
    {
        Host = host;
        Port = port;
        try
        {
            ClearAllMsg();
            var ipType = AddressFamily.InterNetwork;
            (ipType, host) = NetUtils.GetIPv6Address(host, port);
            var socket = new TcpClient(ipType);
            try
            {
                await socket.ConnectAsync(host, port);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
            
            if (!socket.Connected)
                return false;


            OnConnected();
            channel = new NetChannel(socket, OnRevice, OnDisConnected);
            _ = channel.StartAsync();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            return false;
        }
    }

    private void OnConnected()
    {
        
    }

    private Message m_NetDisConnectMessage = null;
    public void OnDisConnected()
    {
        if (m_NetDisConnectMessage == null)
        {
            m_NetDisConnectMessage = Activator.CreateInstance(Assembly.GetExecutingAssembly().GetType("NetDisConnectMessage")) as Message;
        }
        msgQueue.Enqueue(m_NetDisConnectMessage);
    }

    public void OnRevice(Message msg)
    {
        msgQueue.Enqueue(msg);
    }

    public void Close()
    {
        channel?.Close();
        channel = null;
        ClearAllMsg();
    }

    public void ClearAllMsg()
    {
        msgQueue = new ConcurrentQueue<Message>();
    }
    
    
    public void ResetResCode(int code = 0)
    {
        ResCode = code;
    }

    /// <summary>
    /// 心跳等无关逻辑的消息可忽略
    /// </summary>
    public void AddIgnoreCode(int msgId)
    {
        if (!ignoreCodeList.Contains(msgId))
            ignoreCodeList.Add(msgId);
    }
}