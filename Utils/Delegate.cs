using Sirenix.OdinInspector;
using System.Collections.Generic;

/// <summary>
/// 用户自定义代理管理类
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public class UserDelegate
{
    /// <summary>
    /// 绑定函数
    /// </summary>
    /// <param name="args"></param>
    public delegate void OnCallBack(params object[] args);
    /// <summary>
    /// 回调列表
    /// </summary>
    //[ShowInInspector, LabelText("回调列表")]
    private List<OnCallBack> mCalls = new List<OnCallBack>();
    /// <summary>
    /// 待移除的列表
    /// </summary>
    //[ShowInInspector, LabelText("待移除的列表")]
    private List<OnCallBack> removeCalls = new List<OnCallBack>();
    /// <summary>
    /// 是否在分发事件
    /// </summary>
    private bool bExecute = false;
    /// <summary>
    /// 添加绑定回调
    /// </summary>
    /// <param name="t"></param>
    public void AppendCalls(OnCallBack t)
    {
        if (t == null)
            return;

        if (!mCalls.Contains(t))
        {
            mCalls.Add(t);
        }
    }
    /// <summary>
    /// 去除指定的绑定回调
    /// </summary>
    /// <param name="t"></param>
    public void RemoveCalls(OnCallBack t)
    {
        if (t == null)
            return;

        if (mCalls.Contains(t))
        {
            if (bExecute)
            {
                removeCalls.Add(t);
            }
            else
            {
                mCalls.Remove(t);
            }
        }
    }

    /// <summary>
    /// 执行绑定回调
    /// </summary>
    /// <param name="args"></param>
    public void ExecuteCalls(params object[] args)
    {
        bExecute = true;
        int iCount = mCalls.Count;
        if (iCount > 0)
        {
            for (int i = 0; i < iCount; i++)
            {
                try
                {
				    if (mCalls[i] != null)	/** && AsyncTrigger.IsTargetValid(mCalls[i].Target)) **/
                    {
                        mCalls[i](args);
                    }
                }
                catch(System.Exception ex)
                {
                    LogHelper.Exception(ex);
                }
            }
        }
        bExecute = false;
        int rCount = removeCalls.Count;
        if (rCount > 0)
        {
            for (int i = 0; i < rCount; ++i)
            {
                if (removeCalls[i] != null)
                {
                    mCalls.Remove(removeCalls[i]);
                }
            }
            removeCalls.Clear();
        }
    }


    /// <summary>
    /// 清空绑定回调
    /// </summary>
    /// <param name="t"></param>
    public void ClearCalls()
    {
        mCalls.Clear();
    }

}
/// <summary>
/// 用户代理触发器原型
/// </summary>
/// <param name="triggle">触发器自己</param>
/// <param name="args">触发参数表</param>
public delegate void UserDelegateFun(UserDelegateTriggle triggle, object[] args);
/// <summary>
/// 用户代理触发连接器触发函数原型
/// </summary>
/// <param name="triggleLinker"></param>
/// <param name="args"></param>
public delegate void UserDelegateLinkerFun(UserDelegateTriggleLinker triggleLinker, object[] args);
/// <summary>
/// 用户代理函数触发连接器
/// </summary>
public class UserDelegateTriggleLinker
{
    /// <summary>
    /// 触发器对象表
    /// </summary>
    public object[] mTriggles;
    /// <summary>
    /// 连接器需要的聚集的参数表
    /// </summary>
    public object[] mUserArgs;
    /// <summary>
    /// 连接器触发函数
    /// </summary>
    public UserDelegateLinkerFun mLinkerFun;
    public UserDelegateTriggleLinker()
    {
    }
    ~UserDelegateTriggleLinker()
    {
        mTriggles = null;
        mLinkerFun = null;
        mUserArgs = null;
    }
    /// <summary>
    /// 设置连接器参数个数，以及触发函数
    /// </summary>
    /// <param name="iArgsCount"></param>
    /// <param name="func"></param>
    public void SetLinker(int iArgsCount, UserDelegateLinkerFun func)
    {
        mUserArgs = new object[iArgsCount];
        mLinkerFun = func;
    }
    /// <summary>
    /// 设置连接器参数，检查是否触发
    /// </summary>
    /// <param name="iIndex"></param>
    /// <param name="o"></param>
    public void SetLinkerArgs(int iIndex, object o)
    {
        if (iIndex >= 0 && iIndex < mUserArgs.Length)
        {
            mUserArgs[iIndex] = o;
        }
        ///检查是否触发
        if (CheckArgsFull())
        {
            try
            {
				if (mLinkerFun != null)	/** && AsyncTrigger.IsTargetValid(mLinkerFun.Target)) **/
                {
                    mLinkerFun(this, mUserArgs);
                }
            }
            catch (System.Exception ex)
            {
				LogHelper.Exception(ex);
            }
         
            ///回收所有触发器
            UserDelegateTriggle.CollectTriggle(mTriggles);
        }
    }
    /// <summary>
    /// 检查连接器触发函数是否聚齐
    /// </summary>
    /// <returns></returns>
    bool CheckArgsFull()
    {
        for (int i = 0; i < mUserArgs.Length; i++)
        {
            if (mUserArgs[i] == null)
            {
                return false;
            }
        }
        return true;
    }
    /// <summary>
    /// 设置触发器连接对象
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static UserDelegateTriggleLinker SetTriggleLinker(params object[] args)
    {
        UserDelegateTriggleLinker triggleLinker = new UserDelegateTriggleLinker();
        triggleLinker.mTriggles = args;
        for (int i = 0; i < args.Length; i++)
        {
            UserDelegateTriggle triggle = args[i] as UserDelegateTriggle;
            if (triggle != null)
            {
                ///设置触发器的连接者
                triggle.SetTriggleLinker(triggleLinker);
            }
        }
        return triggleLinker;
    }
}
/// <summary>
/// 用户代理触发器
/// </summary>
public class UserDelegateTriggle
{
    /// <summary>
    /// 触发器参数表
    /// </summary>
    public object[] mContextArgs;
    /// <summary>
    /// 触发器触发函数
    /// </summary>
    private UserDelegateFun mactionud;
    /// <summary>
    /// 触发器的链接器
    /// </summary>
    UserDelegateTriggleLinker mTriggleLinker;
    public UserDelegateTriggle()
    {
    }
    ~UserDelegateTriggle()
    {
    }
    public void SetTriggleLinker(UserDelegateTriggleLinker triggleLinker)
    {
        mTriggleLinker = triggleLinker;
    }
    public UserDelegateTriggleLinker GetTriggleLinker()
    {
        return mTriggleLinker;
    }
    public void SetDelegateFun(UserDelegateFun ud)
    {
        mactionud = ud;
    }
    public object[] GetContexts()
    {
        return mContextArgs;
    }
    public void SetContext(params object[] o)
    {
        mContextArgs = o;
    }
    public bool Triggle(params object[] args)
    {
        if (mactionud == null)
            return true;

        try
        {
            mactionud(this, args);
        }
        catch (System.Exception ex)
        {	
			LogHelper.Exception(ex);
        }
       
        if (mTriggleLinker != null)
        {
            return false;
        }
        return true;
    }
   /// <summary>
   /// 清空触发器的所有信息
   /// </summary>
    public void Clear()
    {
        mactionud = null;
        mactionud = null;
        mTriggleLinker = null;
    }
    /// <summary>
    /// 异步调用触发器
    /// </summary>
    static Queue<UserDelegateTriggle> mTrigglePools = new Queue<UserDelegateTriggle>();

    /// <summary>
    /// 获取当前可用的异步触发器，提供触发类型
    /// </summary>
    /// <param name="ud"></param>
    /// <returns></returns>
    public static UserDelegateTriggle GetTriggle(UserDelegateFun ud, params object[] args)
    {
        UserDelegateTriggle triggle;
        if (mTrigglePools.Count > 0)
        {
            triggle = mTrigglePools.Dequeue();
            triggle.SetDelegateFun(ud);
            triggle.SetContext(args);
        }
        else
        {
            triggle = new UserDelegateTriggle();
            triggle.SetDelegateFun(ud);
            triggle.SetContext(args);
        }
        return triggle;
    }
    /// <summary>
    /// 调用一个异步触发器
    /// </summary>
    /// <param name="triggle"></param>
    /// <param name="bFree"></param>
    public static void Triggle(UserDelegateTriggle triggle, params object[] args)
    {
        if (triggle != null)
        {
            ///触发返回是否回收
            if (triggle.Triggle(args))
            {
                triggle.Clear();
                ///没有触发连接器的，直接回收
                mTrigglePools.Enqueue(triggle);
            }
        }
    }
    /// <summary>
    /// 回收所有触发器，用于连接器的回收
    /// </summary>
    /// <param name="args"></param>
    public static void CollectTriggle(object[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            UserDelegateTriggle triggle = args[i] as UserDelegateTriggle;
            if (triggle != null)
            {
                triggle.Clear();
                mTrigglePools.Enqueue(triggle);
            }
        }
    }
}
