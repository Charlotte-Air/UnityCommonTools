using System;
using UnityEngine;
using Framework.Utils;
using System.Threading;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

namespace Framework.Manager
{
    public class SDKManager : SingletonMonoBehaviour<SDKManager>
    {
        private const string WXAPPID = "";
        private const string WXSECRET = "";
        private const string SDK_JAVA_CLASS = "com.xxxx.game";
        private const string UNITY_PLAYER_JAVA_OBJECT = "currentActivity";
        private const string UNITY_PLAYER_JAVA_CLASS = "com.unity3d.player.UnityPlayer";

        private AndroidJavaClass AndroidJavaClass = null;
        private AndroidJavaObject AndroidJavaObject = null;

        public LoginSdkCallBack LoginSDKCallback = null;
        public delegate void LoginSdkCallBack(string ticket, string userId = "", string userData = "");
        public LoginSdkOutCallBack LogoutSDKCallbakc = null;
        public delegate void LoginSdkOutCallBack(string msg);

        protected override void OnSingletonInit()
        {

        }

        protected override void OnSingletonRelease()
        {

        }

        public void Init()
        {
            AndroidJavaClass = new AndroidJavaClass(UNITY_PLAYER_JAVA_CLASS);
            AndroidJavaObject = AndroidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
        }
        
        /// <summary>
        /// 使用微信登入
        /// </summary>
        private void OnWxLogin()
        {
            
        }
        
        /// <summary>
        /// 使用QQ登入
        /// </summary>
        private void OnQQLogin()
        {
            
        }

        public void OnWxLoginCallBack(string data)
        {
            if (data != "用户取消" || data != "用户拒绝" || data != "其他错误")
            {
                //微信登入成功
                GetWxAccessTokenData(data).ToCoroutine();
            }
            else
            {
                
            }
        }

        
        /// <summary>
        /// 获取微信access_token数据
        /// </summary>
        /// <param name="code"></param>
        private async UniTask GetWxAccessTokenData(string code)
        {
            var url = $"https://api.weixin.qq.com/sns/oauth2/access_token?appid={WXAPPID}&secret=SECRET&code={code}&grant_type=authorization_code";
            UnityWebRequest request = new UnityWebRequest(url);
            DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
            request.downloadHandler = dH;
            var cts = new CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(3));
            try
            {
                await request.SendWebRequest().WithCancellation(cts.Token);
            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken == cts.Token)
                {

                }
            }
            if (request.isDone && request.error == null)
            {
                var tokenData = JsonUtility.FromJson<WxAccessTokenData>(request.downloadHandler.text);
                await GetWxUserWxInfoData(tokenData);
            }
        }
        
        
        /// <summary>
        /// 获取微信用户信息
        /// </summary>
        /// <param name="tokenData"></param>
        private async UniTask GetWxUserWxInfoData(WxAccessTokenData tokenData)
        {
            var url = $"https://api.weixin.qq.com/sns/userinfo?access_token={tokenData.access_token}&openid={tokenData.openid}";
            UnityWebRequest request = new UnityWebRequest(url);
            DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
            request.downloadHandler = dH;
            var cts = new CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(3));
            try
            {
                await request.SendWebRequest().WithCancellation(cts.Token);
            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken == cts.Token)
                {

                }
            }
            if (request.isDone && request.error == null)
            {
                var userInfoData = JsonUtility.FromJson<WxUserInfoData>(request.downloadHandler.text);
                
            }
        }
        
        class WxAccessTokenData
        {
            /// <summary>
            /// 接口调用凭证
            /// </summary>
            public string access_token;
            /// <summary>
            /// access_token 接口调用凭证超时时间，单位（秒）
            /// </summary>
            public string expires_in;
            /// <summary>
            /// 用户刷新 access_token
            /// </summary>
            public string refresh_token;
            /// <summary>
            /// 授权用户唯一标识
            /// </summary>
            public string openid;
            /// <summary>
            /// 用户授权的作用域（snsapi_userinfo）
            /// </summary>
            public string scope;
        }

        class WxUserInfoData
        {
            /// <summary>
            /// 授权用户唯一标识
            /// </summary>
            public string openid;
            /// <summary>
            /// 普通用户昵称
            /// </summary>
            public string nickname;
            /// <summary>
            /// 普通用户性别，1 为男性，2 为女性
            /// </summary>
            public int sex;
            /// <summary>
            /// 普通用户个人资料填写的省份
            /// </summary>
            public string province;
            /// <summary>
            /// 普通用户个人资料填写的城市
            /// </summary>
            public string city;
            /// <summary>
            /// 国家，如中国为 CN
            /// </summary>
            public string country;
            /// <summary>
            /// 用户头像，最后一个数值代表正方形头像大小（有 0、46、64、96、132 数值可选，0 代表 640*640 正方形头像），用户没有头像时该项为空
            /// </summary>
            public string headimgurl;
            /// <summary>
            /// 用户特权信息，json 数组，如微信沃卡用户为（chinaunicom）
            /// </summary>
            public string[] privilege;
            /// <summary>
            /// 用户统一标识。针对一个微信开放平台账号下的应用，同一用户的 unionid 是唯一的。
            /// </summary>
            public string unionid;
        }
    }
}