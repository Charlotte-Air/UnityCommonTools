using System;
using UnityEngine;
using Framework.Utils;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Framework.Manager
{
    public class TimerManager : SingletonInstance<TimerManager>, ISingleton
    {
        public delegate void TimerManagerHandler(params object[] args);
        public delegate void TimerManagerCountHandler(int count, params object[] args);
        
        private List<string> m_DestroyTimerList = new List<string>();
        private DictionaryExtend<string, Timer> m_TimerList = new DictionaryExtend<string, Timer>();
        private DictionaryExtend<string, Timer> m_AddTimerList = new DictionaryExtend<string, Timer>();
        
        void ISingleton.OnCreate(object createParam)
        {
  
        }
    
        void ISingleton.OnDestroy()
        {
            ClearAllTimer();
        }
    
        void ISingleton.OnUpdate()
        {
            if (m_DestroyTimerList.Count > 0)
            {
                for (var i = 0; i < m_DestroyTimerList.Count; i++)
                {
                    m_TimerList.Remove(m_DestroyTimerList[i]);
                }
                m_DestroyTimerList.Clear();
            }

            if (m_AddTimerList.Count > 0)
            {
                for (int i = 0, imax = m_AddTimerList.mList.Count; i < imax; i++)
                {
                    var value = m_AddTimerList[m_AddTimerList.mList[i]];
                    if (value == null)
                        continue;

                    if (m_TimerList.ContainsKey(m_AddTimerList.mList[i]))
                        m_TimerList[m_AddTimerList.mList[i]] = value;
                    else
                        m_TimerList.Add(m_AddTimerList.mList[i], value);
                }
                m_AddTimerList.Clear();
            }

            if (m_TimerList.Count > 0)
            {
                for (int i = 0, imax = m_TimerList.mList.Count; i < imax; i++)
                {
                    var value = m_TimerList[m_TimerList.mList[i]];
                    if (value == null)
                        return;
                    
                    value.Run();
                    
                    if (m_TimerList.mList.Count == 0)
                        return;
                }
            }
        }
        
        
        /// <summary>
        /// ���Ӷ�ʱ��
        /// </summary>
        /// <param name="key"></param>
        /// <param name="duration"></param>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool AddTimer(string key, float duration, TimerManagerHandler handler, params object[] args)
        {
            return Internal_AddTimer(key, TIMER_MODE.NORMAL, duration, handler, args);
        }


        /// <summary>
        /// �����ظ���ʱ��
        /// </summary>
        /// <param name="key"></param>
        /// <param name="duration"></param>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool AddTimerRepeat(string key, float duration, TimerManagerHandler handler, params object[] args)
        {
            return Internal_AddTimer(key, TIMER_MODE.REPEAT, duration, handler, args);
        }


        /// <summary>
        /// ���Ӷ�ʱ������
        /// </summary>
        /// <param name="key"></param>
        /// <param name="duration"></param>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool AddTimerCount(string key, float duration, TimerManagerCountHandler handler, params object[] args)
        {
            return Internal_AddTimer(key, TIMER_MODE.COUNTTIME, duration, param => { handler((int)param[0], args); }, args);
        }


        /// <summary>
        /// �����ӳٶ�ʱ��
        /// </summary>
        /// <param name="key"></param>
        /// <param name="duration"></param>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool AddDelayTimer(string key, float duration, TimerManagerHandler handler, params object[] args)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            if (duration < 0.0f)
            {
                if (handler != null)
                {
                    handler(args);
                }

                return true;
            }

            Timer timer = new Timer(key, TIMER_MODE.DELAYTIME, Time.realtimeSinceStartup, duration, handler, args);
            if (m_TimerList.ContainsKey(key))
            {
                m_TimerList[key] = timer;
            }
            else
            {
                m_TimerList.Add(key, timer);
            }

            m_DestroyTimerList.Remove(key);
            return true;
        }


        /// <summary>
        /// ����ƥ�䶨ʱ��
        /// </summary>
        /// <param name="prefix"></param>
        public void ClearTimerWithPrefix(string prefix)
        {
            if (m_TimerList != null && m_TimerList.Count > 0)
            {
                foreach (string timerKey in m_TimerList.Keys)
                {
                    if (timerKey.StartsWith(prefix))
                    {
                        Destroy(timerKey);
                    }
                }
            }
        }


        /// <summary>
        /// ���ٶ�ʱ��
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Destroy(string key)
        {
            if (!IsHaveTimer(key))
                return false;

            if (!m_DestroyTimerList.Contains(key))
            {
                m_DestroyTimerList.Add(key);
            }

            m_AddTimerList.Remove(key);

            return true;
        }


        /// <summary>
        /// ��ʱ���Ƿ����
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsHaveTimer(string key)
        {
            if (m_DestroyTimerList.Contains(key))
                return false;
            return m_TimerList.ContainsKey(key) || m_AddTimerList.ContainsKey(key);
        }


        /// <summary>
        /// ��ʱ������״̬
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsRunning(string key)
        {
            return m_TimerList.ContainsKey(key);
        }


        /// <summary>
        /// �ָ���ʱ��
        /// </summary>
        /// <param name="key"></param>
        /// <param name="handler"></param>
        public void RefreshTimer(string key, TimerManagerHandler handler)
        {
            if (!IsRunning(key))
                return;
            Timer timer = null;
            m_TimerList.TryGetValue(key, out timer);
            timer?.ResetTimerEvent(handler);
        }


        /// <summary>
        /// ���ȫ����ʱ��
        /// </summary>
        public void ClearAllTimer()
        {
            m_DestroyTimerList.Clear();
            m_AddTimerList.Clear();
            m_TimerList.Clear();
        }


        /// <summary>
        /// �ڲ�������ʱ��
        /// </summary>
        /// <param name="key"></param>
        /// <param name="mode"></param>
        /// <param name="duration"></param>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool Internal_AddTimer(string key, TIMER_MODE mode, float duration, TimerManagerHandler handler, params object[] args)
        {
            if (string.IsNullOrEmpty(key))
                return false;
            if (duration < 0.0f)
                return false;

            var timer = new Timer(key, mode, Time.realtimeSinceStartup, duration, handler, args);
            if (m_AddTimerList.ContainsKey(key))
            {
                m_AddTimerList[key] = timer;
            }
            else
            {
                m_AddTimerList.Add(key, timer);
            }

            m_DestroyTimerList.Remove(key);
            return true;
        }
        

        private enum TIMER_MODE
        {
            /// <summary>
            /// ����
            /// </summary>
            NORMAL,

            /// <summary>
            /// �ظ�
            /// </summary>
            REPEAT,

            /// <summary>
            /// ��ʱ
            /// </summary>
            COUNTTIME,

            /// <summary>
            /// �ӳ�
            /// </summary>
            DELAYTIME,
        }

        private class Timer
        {
            /// <summary>
            /// ��ʱ��Key
            /// </summary>
            private string m_Name;

            /// <summary>
            /// ��ʱ��ģʽ
            /// </summary>
            private TIMER_MODE m_Mode;

            /// <summary>
            /// ��ʼʱ��
            /// </summary>
            private float m_StartTime;

            /// <summary>
            /// ����ʱ��
            /// </summary>
            private float m_duration;

            /// <summary>
            /// ʱ��
            /// </summary>
            private float m_time = 0;

            /// <summary>
            /// �ص�
            /// </summary>
            private TimerManagerHandler m_TimerEvent;

            /// <summary>
            /// ����
            /// </summary>
            private object[] m_Args = null;

            /// <summary>
            /// ��ʼʱ��
            /// </summary>
            public float StartTime
            {
                get { return m_StartTime; }
                set { m_StartTime = value; }
            }

            /// <summary>
            /// ʣ��ʱ��
            /// </summary>
            public float TimeLeft
            {
                get { return Mathf.Max(0.0f, m_duration - (Time.realtimeSinceStartup - m_StartTime)); }
            }

            /// <summary>
            /// ʵ������ʱ��
            /// </summary>
            /// <param name="name">��ʱ��Key</param>
            /// <param name="mode">��ʱ��ģʽ</param>
            /// <param name="startTime">��ʼʱ��</param>
            /// <param name="duration">����ʱ��</param>
            /// <param name="handler">�ص�</param>
            /// <param name="args">�ص�����</param>
            public Timer(string name, TIMER_MODE mode, float startTime, float duration, TimerManagerHandler handler, params object[] args)
            {
                m_Name = name;
                m_Mode = mode;
                m_Args = args;
                m_StartTime = startTime;
                m_duration = duration;
                m_TimerEvent = handler;
            }

            public void Run()
            {
                if (m_Mode == TIMER_MODE.DELAYTIME)
                {
                    if (Time.realtimeSinceStartup - m_StartTime > m_duration)
                    {
                        if (this.m_TimerEvent != null) /** && AsyncTrigger.IsTargetValid(this.m_TimerEvent.Target))	**/
                        {
                            try
                            {
                                this.m_TimerEvent(m_Args);
                            }
                            catch (System.Exception ex)
                            {
                                TimerManager.Instance.Destroy(this.m_Name);
                                LogHelper.Exception(ex);
                            }
                        }

                        TimerManager.Instance.Destroy(this.m_Name);
                    }

                    return;
                }
                else if (m_Mode == TIMER_MODE.COUNTTIME)
                {
                    float lastTime = Time.realtimeSinceStartup - m_time;
                    if (lastTime > 1.0f)
                    {
                        m_time = Time.realtimeSinceStartup;

                        if (this.TimeLeft < 0f)
                        {
                            TimerManager.Instance.Destroy(this.m_Name);
                        }

                        try
                        {
                            if (this.m_TimerEvent !=
                                null) /** && AsyncTrigger.IsTargetValid(this.m_TimerEvent.Target))	**/
                            {
                                this.m_TimerEvent(Mathf.CeilToInt(this.TimeLeft), m_Args);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            TimerManager.Instance.Destroy(this.m_Name);
                            LogHelper.Exception(ex);
                        }
                    }

                    return;
                }

                if (this.TimeLeft > 0.0f)
                    return;

                if (this.m_TimerEvent != null) /** && AsyncTrigger.IsTargetValid(this.m_TimerEvent.Target))	**/
                {
                    try
                    {
                        this.m_TimerEvent(m_Args);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Exception(ex);
                    }
                }

                if (m_Mode == TIMER_MODE.NORMAL)
                {
                    TimerManager.Instance.Destroy(this.m_Name);
                }
                else
                {
                    m_StartTime = Time.realtimeSinceStartup;
                }
            }

            public void ResetTimerEvent(TimerManagerHandler handler)
            {
                this.m_TimerEvent = handler;
            }
        }
    }
}