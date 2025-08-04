using System.Collections.Generic;

public class ExceptionController
{
    static private ExceptionController _instance;
    static public ExceptionController getInstance()
    {
        if (_instance == null)
        {
            _instance = new ExceptionController();
        }
        return _instance;
    }
    
    private ExceptionCallBack _exception_callback = null;
    public delegate void ExceptionCallBack(string name, string stack);
    private static Dictionary<string, string> _exception_dic = new Dictionary<string, string>();

    private ExceptionController()
    {
        
    }
    
    private string m_RoleName = "";
    public void SetRoleName(string role_name)
    {
        m_RoleName = role_name;
    }
    
    public void SetExceptionCallBack(ExceptionCallBack callback)
    {
        _exception_callback = callback;
    }
    
    public void SendExceptionMessageToLua(string name, string stack)
    {
        if (_exception_callback != null)
            _exception_callback(name, stack);
        
        _exception_callback = null;
    }

    public void SendExceptionMessageToWebServer(string exception)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        return;
#else

#endif
    }
    public void SendExceptionMessageToWebServer(string sysid, string systemname, string sysversion, string systemmodel, string appbuildversion, string appversion, string exception)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        return;
#else

#endif
    }

    void OnSendException(string result)
    {

    }
}