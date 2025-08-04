using System;
using UnityEngine;
using System.Text;
using Framework.Manager;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Framework.Utils.Unity
{
    public class TimeUtils
    {
        private static long serverEnterMSec = 0;
        private static long clientEnterMSec = 0;
        private static long serverEnterSec = 0;
        private static long clientEnterSec = 0;
        private static int serverTimeZone = 0;
        public static bool SynTimer = false;
        public static UserDelegate CorrectionTime = new UserDelegate();

        public static void UnInit()
        {
            CorrectionTime.ClearCalls();
        }

        public static long GetLocalUtcSconds()
        {
            DateTime dateTime = new DateTime().AddTicks(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0, 0).Ticks);
            TimeSpan sp = new TimeSpan(dateTime.Ticks);
            return (long)sp.TotalSeconds;
        }

        public static int GetDaysFrom(long timestamp)
        {
            // 指定时间戳至今的天数， 创角当日算1， 过0点加1
            DateTime time = ConvertTimestamp2DateTime(timestamp);
            DateTime _time = new DateTime(time.Year, time.Month, time.Day);
            DateTime realTime = GetServerDataTime();
            DateTime _realTime = new DateTime(realTime.Year, realTime.Month, realTime.Day);
            TimeSpan t = realTime.Subtract(_time);
            return t.Days + 1;
        }


        private static int GetDayZeroTime(int dwTime)
        {
            DateTime stTime = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local)).AddSeconds(dwTime);
            DateTime stZeroTime = new DateTime(stTime.Year, stTime.Month, stTime.Day);
            stZeroTime = TimeZoneInfo.ConvertTime(stZeroTime, TimeZoneInfo.Local);
            TimeSpan cha = stZeroTime - (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local));
            return (int)cha.TotalSeconds;
        }


        /// <summary>
        /// 获取本地时间戳(s)
        /// </summary>
        /// <returns></returns>
        public static long GetLocalTimeStamp()
        {
            TimeSpan cha = DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (long)cha.TotalSeconds;
        }


        /// <summary>
        /// 获取本地时间戳(ms)
        /// </summary>
        /// <returns></returns>
        public static long GetLocalTimeStampMillisecond()
        {
            TimeSpan cha = DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (long)cha.TotalMilliseconds;
        }


        /// <summary>
        /// 根据两者时间戳计算相差时间
        /// </summary>
        /// <param name="dwTimeNew"></param>
        /// <param name="dwTimeOld"></param>
        /// <returns></returns>
        public static int DayPass(int dwTimeNew, int dwTimeOld)
        {
            if (dwTimeNew <= dwTimeOld)
            {
                return 0;
            }

            dwTimeNew = GetDayZeroTime(dwTimeNew);
            dwTimeOld = GetDayZeroTime(dwTimeOld);
            return (int)(dwTimeNew - dwTimeOld) / (24 * 3600);
        }


        /// <summary>
        /// 将秒数转换为 {0}小时{1}分钟{2}秒
        /// </summary>
        public static string GetTimeDown(uint second)
        {
            string str = "";
            int hour = Mathf.FloorToInt(second / 3600);
            int min = Mathf.FloorToInt((second - 3600 * hour) / 60);
            int sec = (int)(second % 60);
            if (hour != 0)
            {
                str += Lang.Get("LANG_COMMON_TIME_H", hour);
            }

            if (min != 0)
            {
                str += Lang.Get("LANG_COMMON_TIME_M", min);
            }

            if (sec != 0)
            {
                str += Lang.Get("LANG_COMMON_TIME_S", sec);
            }

            return str;
        }

        /// <summary>
        /// 倒计时
        /// </summary>
        /// <param name="timerKey">返回的计时器句柄ID,用于重新调用时停止原先的计时器</param>
        /// <param name="time">结束时间,小于maxCutDownNum为倒计时剩余秒数</param>
        /// <param name="label">需要显示的Text, 为null时,需要将sOriginalStr参数置为"",否则将停止计时器</param>
        /// <param name="overCallback">倒计时结束的回调</param>
        /// <param name="sOriginalStr">Text上显示的文本格式,默认{0}:{1}:{2}</param>
        /// <param name="format">显示时间的位数,默认显示时,分,秒</param>
        /// <returns>计时器的句柄ID</returns>
        public static void GetCountDown(string timerKey, long time, UnityEngine.UI.Text label, Action overCallback, string sOriginalStr = "{0}:{1}:{2}", string format = "HH:MM:SS")
        {
            TimerManager.Instance.Destroy(timerKey);
            var nowTime = ServerTimeUtils.getInstance().getCurrentServerTimeStamp();
            var timeLeft = time < 10000000 ? time : time - nowTime;
            int[] t = { 0, 0, 0, 0 };
            Action setLable = () =>
            {
                if (label == null && sOriginalStr == "") return;
                if (label == null)
                {
                    TimerManager.Instance.Destroy(timerKey);
                    return;
                }

                string p = @"{\d}";
                var math = Regex.Matches(sOriginalStr, p);
                if (math.Count == 1) // 默认 SS
                {
                    if (format == "DD")
                        label.text = Lang.GetByString(sOriginalStr, t[0]);
                    else if (format == "HH")
                        label.text = Lang.GetByString(sOriginalStr, t[0] * 24 + t[1]);
                    else if (format == "MM")
                        label.text = GetByString(sOriginalStr, t[0] * 1440 + t[1] * 60 + t[2]);
                    else
                        label.text = GetByString(sOriginalStr, t[0] * 86400 + t[1] * 3600 + t[2] * 60 + t[3]);
                }
                else if (math.Count == 2) // 默认 HH:MM
                {
                    if (format == "DD:HH")
                        label.text = Lang.GetByString(sOriginalStr, t[0], t[1]);
                    else if (format == "HH:MM")
                        label.text = GetByString(sOriginalStr, (t[0] * 24 + t[1]).ToString("D2"),
                            t[2].ToString("D2"));
                    else if (format == "MM:SS")
                        label.text = GetByString(sOriginalStr, (t[0] * 1440 + t[1] * 60 + t[2]).ToString("D2"),
                            t[3].ToString("D2"));
                    else
                        label.text = GetByString(sOriginalStr, (t[0] * 24 + t[1]).ToString("D2"), t[2].ToString("D2"));
                }
                else if (math.Count == 3) // 默认 HH:MM:SS
                {
                    if (format == "DD:HH:MM")
                        label.text = GetByString(sOriginalStr, t[0], t[1].ToString("D2"), t[2].ToString("D2"));
                    else
                        label.text = GetByString(sOriginalStr, (t[0] * 24 + t[1]).ToString("D2"), t[2].ToString("D2"),
                            t[3].ToString("D2"));
                }
                else
                {
                    label.text = GetByString(sOriginalStr, t[0], t[1].ToString("D2"), t[2].ToString("D2"),
                        t[3].ToString("D2"));
                }
            };
        
            Action<long> getLeft = (tl) =>
            {
                var day = tl / 86400;
                var hour = (tl - day * 86400) / 3600;
                var minute = (tl - day * 86400 - hour * 3600) / 60;
                var second = tl % 60;
                t[0] = (int)day;
                t[1] = (int)hour;
                t[2] = (int)minute;
                t[3] = (int)second;
            };
        
            if (timeLeft <= 0)
            {
                setLable();
                overCallback?.Invoke();
            }
            else
            {
                getLeft(timeLeft);
                setLable();
                TimerManager.Instance.AddTimerRepeat(timerKey, 1, args =>
                {
                    timeLeft--;
                    if (timeLeft < 0)
                    {
                        TimerManager.Instance.Destroy(timerKey);
                        overCallback?.Invoke();
                        return;
                    }
                    getLeft(timeLeft);
                    setLable();
                });
            }
        }


        public static string GetByString(string sOriginalStr, params object[] args)
        {
            string tblStr = string.Format(sOriginalStr, args);
            tblStr = CheckString(tblStr);
            return tblStr;
        }


        public static readonly string No_Breaking_Space = "\u00A0";

        /// <summary>
        /// 中文半角空格在多行文本中会自动换行，需要将其替换成不换行空格
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string CheckString(string str)
        {
            if (str == null)
                return "";
            if (str.Contains(" "))
            {
                str = str.Replace(" ", No_Breaking_Space);
            }

            if (str.Contains("\\n"))
            {
                str = str.Replace("\\n", "\n");
            }

            return str;
        }


        // 将给定的时间戳转换为DataTime
        // int[] 数组形式
        public static int[] getDateByTimeStamp(long timeStamp)
        {
            DateTime dt = TimeZoneInfo.ConvertTimeToUtc(new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Utc))
                .AddSeconds(timeStamp);
            int[] time = { dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second };
            return time;
        }


        // 将给定的时间戳转换为"MM-DD HH-MM 至 MM-DD HH-MM"的形式
        // string
        public static string getDateStringByTimeStamp(long startTimeStamp, long endTimeStamp)
        {
            string str = "";
            int[] startArray = getDateByTimeStamp(startTimeStamp);
            int[] endArray = getDateByTimeStamp(endTimeStamp);
            str = str + startArray[1].ToString("D2") + "-" + startArray[2].ToString("D2") + " " +
                  startArray[3].ToString("D2") + ":" + startArray[4].ToString("D2") + "至"
                  + endArray[1].ToString("D2") + "-" + endArray[2].ToString("D2") + " " + endArray[3].ToString("D2") + ":" +
                  endArray[4].ToString("D2");
            return str;
        }


        #region 初始化时间

        /// <summary>
        /// 校正本地时间
        /// </summary>
        /// <param name="timer">服务器以秒计算的时间</param>
        public static void CorrectLocalTime(long timer, long pServerTicks, int timeZone)
        {
            // DateTime serverTime = new DateTime(1970, 1, 1).AddSeconds(timer);
            DateTime nowTime = DateTime.Now;
            serverEnterMSec = pServerTicks;
            clientEnterMSec = nowTime.Ticks / 10000;
            serverEnterSec = timer;
            clientEnterSec = nowTime.Ticks / 10000000;
            serverTimeZone = timeZone;
            DateTime serverTime = ConvertTimestamp2DateTime(timer, timeZone);
            LogHelper.DebugFormat("Server Time day : {0}, hour : {1}, min : {2}", serverTime.Day, serverTime.Hour,
                serverTime.Minute);
            SynTimer = true;
            if (CorrectionTime != null)
                CorrectionTime.ExecuteCalls();
            //LogHelper.Info("CorrectLocalTime:" + timer + " " + pServerTicks);
        }

        /// <summary>
        /// 获得服务器当前的ticks
        /// </summary>
        /// <param name="timer"></param>
        /// <returns></returns>
        public static long ServerNowTicks()
        {
            DateTime nowTime = DateTime.Now;
            return (serverEnterMSec + nowTime.Ticks / 10000 - clientEnterMSec);
        }

        public static long GetServerSeconds()
        {
            DateTime nowTime = DateTime.Now;
            return (serverEnterSec + nowTime.Ticks / 10000000 - clientEnterSec);
        }

        public static int GetElapseDays(long t)
        {
            long dt = GetServerSeconds() - t;
            int days = UnityEngine.Mathf.CeilToInt((float)((double)dt / (24 * 3600)));
            return days;
        }

        /// <summary>
        /// 获取服务器时间戳
        /// (不允许修改)
        /// </summary>
        /// <returns></returns>
        public static DateTime GetServerDataTime()
        {
            long serverSec = GetServerSeconds();
            return ConvertTimestamp2DateTime(serverSec, serverTimeZone);
        }

        /// <summary>
        /// 获取本地时间（校验）
        /// </summary>
        /// <returns></returns>
        public static DateTime GetLocalDataTime()
        {
            long serverSec = GetServerSeconds();
            return ConvertTimestamp2DateTime(serverSec, serverTimeZone);
        }

        /// <summary>
        /// 获取总秒数(系统时间)
        /// </summary>
        /// <returns></returns>
        public static long GetTotleSeconds()
        {
            long serverSec = GetServerSeconds();
            return serverSec;
        }

        public static long GetTotleMilliseconds()
        {
            long serverSec = GetServerSeconds();
            return serverSec * 1000;
        }

        #endregion


        #region 转换时间格式

        public enum TimeStrFormat
        {
            YMDHMS,
            YMD,
            DHMS,
            HMS,
            MS,
            S,
            HM,
            DHM,
        }

        public enum SymbolFormat
        {
            Chinese,
            English,
            Symbol,
        }

        /// <summary>
        /// 转换秒为具体的时间--增加时区信息
        /// </summary>
        /// <returns></returns>
        public static DateTime ConvertTimestamp2DateTime(long t, int timeZone)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(t);
            return start.AddHours(timeZone);
        }


        public static DateTime ConvertTimestamp2DateTime(long t)
        {
            return ConvertTimestamp2DateTime(t, serverTimeZone);
        }

        public static string ConvertTimestamp2Str(long t, TimeStrFormat TSFormat)
        {
            DateTime time = ConvertTimestamp2DateTime(t, serverTimeZone);
            StringBuilder sb = new StringBuilder();
            int year, month, day, hour, minute, second;

            year = time.Year;
            month = time.Month;
            day = time.Day;
            hour = time.Hour;
            minute = time.Minute;
            second = time.Second;

            switch (TSFormat)
            {
                case TimeStrFormat.YMDHMS:
                    sb.Append(string.Format("{0:00}", year)).Append("-");
                    sb.Append(string.Format("{0:00}", month)).Append("-");
                    sb.Append(string.Format("{0:00}", day)).Append(" ");
                    sb.Append(string.Format("{0:00}", hour)).Append(":");
                    sb.Append(string.Format("{0:00}", minute)).Append(":");
                    sb.Append(string.Format("{0:00}", second));
                    break;
                case TimeStrFormat.YMD:
                    sb.Append(string.Format("{0:00}", year)).Append("-");
                    sb.Append(string.Format("{0:00}", month)).Append("-");
                    sb.Append(string.Format("{0:00}", day));
                    break;
                case TimeStrFormat.HMS:
                    sb.Append(string.Format("{0:00}", hour)).Append(":");
                    sb.Append(string.Format("{0:00}", minute)).Append(":");
                    sb.Append(string.Format("{0:00}", second));
                    break;
                case TimeStrFormat.HM:
                    sb.Append(string.Format("{0:00}", hour)).Append(":");
                    sb.Append(string.Format("{0:00}", minute));
                    break;
                case TimeStrFormat.MS:
                    sb.Append(string.Format("{0:00}", minute)).Append(":");
                    sb.Append(string.Format("{0:00}", second));
                    break;
                case TimeStrFormat.DHM:
                    sb.Append(string.Format("{0:00}", day)).Append(":");
                    sb.Append(string.Format("{0:00}", hour)).Append(":");
                    sb.Append(string.Format("{0:00}", minute)).Append(":");
                    break;
                default:
                    break;
            }

            return sb.ToString();
        }

        public static string ConvertSec2Str(long sec, TimeStrFormat TSFormat, SymbolFormat SFormat = SymbolFormat.Symbol)
        {
            StringBuilder sb = new StringBuilder();
            int day, hour, minute, second;

            string dSymbol, hSymbol, mSymbol, sSymbol;
            switch (SFormat)
            {
                case SymbolFormat.Chinese:
                    dSymbol = "天";
                    hSymbol = "小时";
                    mSymbol = "分";
                    sSymbol = "秒";
                    break;
                case SymbolFormat.English:
                    dSymbol = "d";
                    hSymbol = "h";
                    mSymbol = "m";
                    sSymbol = "s";
                    break;
                case SymbolFormat.Symbol:
                default:
                    dSymbol = hSymbol = mSymbol = ":";
                    sSymbol = "";
                    break;
            }

            switch (TSFormat)
            {
                case TimeStrFormat.DHMS:
                    day = Convert.ToInt16(sec / 86400);
                    hour = Convert.ToInt16((sec % 86400) / 3600);
                    minute = Convert.ToInt16((sec % 86400 % 3600) / 60);
                    second = Convert.ToInt16(sec % 86400 % 3600 % 60);

                    if (day > 0)
                        sb.Append(string.Format("{0:00}", day)).Append(dSymbol);
                    if (hour >= 0)
                        sb.Append(string.Format("{0:00}", hour)).Append(hSymbol);
                    if (minute >= 0)
                        sb.Append(string.Format("{0:00}", minute)).Append(mSymbol);
                    sb.Append(string.Format("{0:00}", second)).Append(sSymbol);
                    break;
                case TimeStrFormat.DHM:
                    day = Convert.ToInt16(sec / 86400);
                    hour = Convert.ToInt16((sec % 86400) / 3600);
                    minute = Convert.ToInt16((sec % 86400 % 3600) / 60);
                    if (day >= 0)
                        sb.Append(string.Format("{0:00}", day)).Append(dSymbol);
                    if (hour >= 0)
                        sb.Append(string.Format("{0:00}", hour)).Append(hSymbol);
                    sb.Append(string.Format("{0:00}", minute)).Append(mSymbol);
                    break;
                case TimeStrFormat.HMS:
                    hour = Convert.ToInt16(sec / 3600);
                    minute = Convert.ToInt16((sec % 3600) / 60);
                    second = Convert.ToInt16(sec % 3600 % 60);

                    if (hour >= 0)
                        sb.Append(string.Format("{0:00}", hour)).Append(hSymbol);
                    if (minute >= 0)
                        sb.Append(string.Format("{0:00}", minute)).Append(mSymbol);
                    sb.Append(string.Format("{0:00}", second)).Append(sSymbol);
                    break;
                case TimeStrFormat.HM:
                    hour = Convert.ToInt16(sec / 3600);
                    minute = Convert.ToInt16((sec % 3600) / 60);

                    if (hour >= 0)
                        sb.Append(string.Format("{0:00}", hour)).Append(hSymbol);
                    sb.Append(string.Format("{0:00}", minute)).Append(SFormat == SymbolFormat.Symbol ? "" : mSymbol);

                    break;
                case TimeStrFormat.MS:
                    minute = Convert.ToInt16(sec / 60);
                    second = Convert.ToInt16(sec % 60);

                    if (minute >= 0)
                        sb.Append(string.Format("{0:00}", minute)).Append(mSymbol);
                    sb.Append(string.Format("{0:00}", second)).Append(sSymbol);
                    break;
                case TimeStrFormat.S:
                    second = (int)sec;
                    sb.Append(string.Format("{0:00}", second)).Append(sSymbol);
                    break;
                default:
                    break;
            }

            return sb.ToString();
        }

        public static long ConvertStr2Sec(string str, TimeStrFormat TSFormat)
        {
            if (string.IsNullOrEmpty(str))
                return 0;

            string[] strArray = str.Split(':');
            if (strArray == null)
                return 0;
            long t = 0;
            int day, hour, minute, second;
            day = hour = minute = second = 0;
            switch (TSFormat)
            {
                case TimeStrFormat.DHMS:
                    if (strArray.Length == 4)
                    {
                        day = IntParse(strArray[0]);
                        hour = IntParse(strArray[1]);
                        minute = IntParse(strArray[2]);
                        second = IntParse(strArray[3]);
                    }
                    else if (strArray.Length == 3)
                    {
                        hour = IntParse(strArray[0]);
                        minute = IntParse(strArray[1]);
                        second = IntParse(strArray[2]);
                    }
                    else if (strArray.Length == 2)
                    {
                        minute = IntParse(strArray[0]);
                        second = IntParse(strArray[1]);
                    }
                    else
                    {
                        second = IntParse(strArray[0]);
                    }

                    break;
                case TimeStrFormat.HMS:
                    if (strArray.Length == 3)
                    {
                        hour = IntParse(strArray[0]);
                        minute = IntParse(strArray[1]);
                        second = IntParse(strArray[2]);
                    }
                    else if (strArray.Length == 2)
                    {
                        minute = IntParse(strArray[0]);
                        second = IntParse(strArray[1]);
                    }
                    else
                    {
                        second = IntParse(strArray[0]);
                    }

                    break;
                case TimeStrFormat.HM:
                    if (strArray.Length == 2)
                    {
                        hour = IntParse(strArray[0]);
                        minute = IntParse(strArray[1]);
                    }
                    else
                    {
                        minute = IntParse(strArray[0]);
                    }

                    break;
                case TimeStrFormat.MS:
                    if (strArray.Length == 2)
                    {
                        minute = IntParse(strArray[0]);
                        second = IntParse(strArray[1]);
                    }
                    else
                    {
                        second = IntParse(strArray[0]);
                    }

                    break;
                case TimeStrFormat.S:
                    second = IntParse(strArray[0]);
                    break;
                default:
                    break;
            }

            t = day * 86400 + hour * 3600 + minute * 60 + second;

            return t;
        }

        /// <summary>
        /// 转换秒为具体的时间(1:1:12:21 天:时:分:秒)
        /// </summary>
        /// <param name="t">秒</param>
        /// <returns>天:时:分:秒</returns>
        public static string ChangeSecsToTime(long t)
        {
            return ConvertSec2Str(t, TimeStrFormat.DHMS, SymbolFormat.Chinese);
        }

        /// <summary>
        /// 转换秒为具体的时间(1:12:21 时:分:秒)
        /// </summary>
        /// <param name="t">秒</param>
        /// <returns>时:分:秒</returns>
        public static string ConvertSecsToHours(long t)
        {
            return ConvertSec2Str(t, TimeStrFormat.HMS);
        }

        public static string ConvertSecsSemicolon(long t)
        {
            return ConvertSec2Str(t, TimeStrFormat.DHMS);
        }

        public static string ConvertSecs(long t)
        {
            return ConvertSec2Str(t, TimeStrFormat.DHMS, SymbolFormat.Chinese);
        }

        public static string ConvertSecs(long t, TimeStrFormat fmt = TimeStrFormat.DHMS,
            SymbolFormat tzone = SymbolFormat.Chinese)
        {
            return ConvertSec2Str(t, fmt, tzone);
        }

        /// <summary>
        /// 转换时间戳为具体时间（时:分:秒）
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        /// <returns></returns>
        public static string ConverLocSecsToTime(long t)
        {
            return ConvertTimestamp2Str(t, TimeStrFormat.HMS);
        }

        /// <summary>
        /// 转换时间戳为具体时间（时:分）
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>    
        public static string ConvertSecsToTime(long t)
        {
            return ConvertTimestamp2Str(t, TimeStrFormat.MS);
        }

        /// <summary>
        /// 转换时间戳为具体时间（小时：分钟   01:01）
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string ConvertSecsToMinute(long t)
        {
            return ConvertTimestamp2Str(t, TimeStrFormat.HM);
        }

        /// <summary>
        /// 1h1m1s
        /// </summary>
        /// <param name="bufferLife"></param>
        /// <returns></returns>
        public static string ConvertCountDown(long t)
        {
            return ConvertSec2Str(t, TimeStrFormat.HMS, SymbolFormat.English);
        }

        public static string GetSurplusTime2(long t, TimeStrFormat tsf = TimeStrFormat.HMS)
        {
            return ConvertSec2Str(t, tsf);
        }

        public static string GetSurplusTime(long t)
        {
            return ConvertSec2Str(t, TimeStrFormat.HMS);
        }

        /// <summary>
        /// 转换时间戳为具体时间（年-月-日 2015-04-09）
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string ConvertSecsToDay(long t)
        {
            return ConvertTimestamp2Str(t, TimeStrFormat.YMD);
        }

        /// <summary>
        /// 转换时间戳为具体时间（年-月-日 时-分-秒）
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string ConvertSecsToSecend(long t)
        {
            return ConvertTimestamp2Str(t, TimeStrFormat.YMDHMS);
        }

        /// <summary>
        /// 转换秒为具体的时间(12:21 分:秒)
        /// </summary>
        /// <param name="t">秒</param>
        /// <returns>时:分:秒</returns>
        public static string ConvertSecsToHMS(long t)
        {
            return ConvertSec2Str(t, TimeStrFormat.HMS);
        }

        /// <summary>
        /// 转换秒为具体的时间(12:21 分:秒)
        /// </summary>
        /// <param name="t">秒</param>
        /// <returns>时:分:秒</returns>
        public static string ConvertSecsToDHMS(long t)
        {
            return ConvertSec2Str(t, TimeStrFormat.DHMS);
        }

        /// <summary>
        /// 转换秒为具体的时间(12:21 分:秒)
        /// </summary>
        /// <param name="t"></param>
        /// <returns>分:秒</returns>
        public static string ConvertSecsToMS(long t)
        {
            return ConvertSec2Str(t, TimeStrFormat.MS);
        }

        public static DateTime ConvertSecsToDateTime(long t)
        {
            return ConvertTimestamp2DateTime(t, serverTimeZone);
        }

        #endregion


        #region 获取时间

        public static int GetSecondBetweenNowTime(int Year, int Month, int Day, int Hour, int Minute, int Second)
        {
            DateTime openTime = GetLocalDataTime();
            DateTime Time = new DateTime(Year, Month, Day, Hour, Minute, Second).AddHours(serverTimeZone);
            //Time = TimeZone.CurrentTimeZone.ToLocalTime(Time);
            TimeSpan span = Time - openTime;
            return (int)span.TotalSeconds;
        }

        public static long GetSecondByFormate(string t)
        {
            return ConvertStr2Sec(t, TimeStrFormat.HM);
        }

        /// <summary>
        /// 获取当天0点之后的总秒数
        /// </summary>
        /// <returns></returns>
        public static long GetLocalTotleSecond()
        {
            return GetLocalDataTime().Hour * 3600 + GetLocalDataTime().Minute * 60 + GetLocalDataTime().Second;
        }

        public static string GetGMTTime()
        {
            return GetLocalDataTime().ToUniversalTime().ToString("r");
        }

        /// <summary>
        /// 当前时间是否在今日特定时间之后 参数周几:点:分 格式
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        public static bool IsAfterDayTime(string endTime)
        {
            //int time_start = 0;
            int time_end = 0;
            int weekday = 0;
            int hour = 0;
            int minute = 0;
            string[] s2 = endTime.Split(':');
            weekday = int.Parse(s2[0]);
            if (weekday == 0)
                weekday = 7;
            hour = int.Parse(s2[1]);
            minute = int.Parse(s2[2]);
            time_end = (weekday - 1) * 24 * 3600 + hour * 3600 + minute * 60;

            int index = (int)GetLocalDataTime().DayOfWeek;
            if (index == 0)
                index = 7;

            int secNow = (int)GetLocalTotleSecond();
            secNow = (index - 1) * 24 * 3600 + secNow;
            if (secNow > time_end) //if (secNow > time_start && secNow < time_end)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 当前时间是否在今日特定时间之后 参数 点:分 格式
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        public static bool IsAfterDayTimeWithoutWeekday(string endTime)
        {
            long time_end = ConvertStr2Sec(endTime, TimeStrFormat.HM);

            long secNow = GetLocalTotleSecond();
            if (secNow > time_end)
                return true;
            return false;
        }

        /// <summary>
        /// 当前时间是否在今日特定时间之后 参数 点:分:秒 格式
        /// </summary>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public static bool IsAfterDayTimeWithoutWeekdayHMS(string endTime)
        {
            long time_end = ConvertStr2Sec(endTime, TimeStrFormat.HMS);

            long secNow = GetLocalTotleSecond();
            if (secNow > time_end)
                return true;
            return false;
        }

        /// <summary>
        /// 当前时间是否在今日特定时间之后 参数几点几分
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        public static bool IsAfterDayTime(int hour, int minute)
        {
            //int time_start = 0;
            long time_end = hour * 3600 + minute * 60;
            long secNow = GetLocalTotleSecond();
            if (secNow > time_end) //if (secNow > time_start && secNow < time_end)
            {
                return true;
            }

            return false;
        }

        public static bool IsAfterDayTime(long time_end)
        {
            long secNow = GetServerSeconds();
            if (time_end - secNow > 0)
                return true;
            return false;
        }

        /// <summary>
        /// 获取到今日特定时间的总秒数 参数 点:分 格式
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        public static long GetTimeWithoutWeekday(string endTime)
        {
            long time_end = ConvertStr2Sec(endTime, TimeStrFormat.HM);
            long secNow = GetLocalTotleSecond();
            return time_end - secNow;
        }

        /// <summary>
        /// 获取到今日特定时间的总秒数 参数 点:分:秒 格式
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        public static long GetTimeWithoutWeekdayHMS(string endTime)
        {
            long time_end = ConvertStr2Sec(endTime, TimeStrFormat.HMS);
            long secNow = GetLocalTotleSecond();
            return time_end - secNow;
        }

        public static long GetRemainingTime(long endTime)
        {
            return endTime - GetServerSeconds();
        }

        public static long GetTotleSecondsByTime(string endTime)
        {
            return ConvertStr2Sec(endTime, TimeStrFormat.HM);
        }

        #endregion


        /// <summary>
        /// String 强转 Int 时调用 默认返回 0
        /// 避免转换过程中包含空字符、以及非数字字符
        /// 导致程序报错
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int IntParse(string value, int defaultValue = 0)
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            value = value.Trim();
            int result;
            if (int.TryParse(value, out result))
            {
                return result;
            }

            return defaultValue;
        }


        /// <summary>
        /// 随机数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputList"></param>
        /// <returns></returns>
        public static List<T> GetRandomList<T>(List<T> inputList)
        {
            //Copy to a array
            T[] copyArray = new T[inputList.Count];
            inputList.CopyTo(copyArray);

            //Add range
            List<T> copyList = new List<T>();
            copyList.AddRange(copyArray);

            //Set outputList and random
            List<T> outputList = new List<T>();
            System.Random rd = new System.Random(DateTime.Now.Millisecond);

            while (copyList.Count > 0)
            {
                //Select an index and item
                int rdIndex = rd.Next(0, copyList.Count - 1);
                T remove = copyList[rdIndex];

                //remove it from copyList and add it to output
                copyList.Remove(remove);
                outputList.Add(remove);
            }

            return outputList;
        }
    }
}