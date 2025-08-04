using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

// Usage:
//    yield return new WaitForEndOfFrame();     		=>      yield return Yielders.EndOfFrame;
//    yield return new WaitForFixedUpdate();    		=>      yield return Yielders.FixedUpdate;
//    yield return new WaitForSeconds(1.0f);    		=>      yield return Yielders.GetWaitForSeconds(1.0f);
// http://forum.unity3d.com/threads/c-coroutine-waitforseconds-garbage-collection-tip.224878/
// #gulu WARNING: 
//      Code commented below are incorrect in Unity 5.5.0
//          - float DOES NOT needs customized IEqualityComparer (but enums and structs do)
//      however all these lines are kept to help later reader to share this knowledge 
//------------------------------------------------------------------
///////////////////// obsoleted code begins \\\\\\\\\\\\\\\\\\\\\\\\
//// dictionary with a key of ValueType will box the value to perform comparison / hash code calculation while scanning the hashtable.
//// here we implement IEqualityComparer<float> and pass it to your dictionary to avoid that GC
//class FloatComparer : IEqualityComparer<float>
//{
//    bool IEqualityComparer<float>.Equals(float x, float y)
//    {
//        return x == y;
//    }
//    int IEqualityComparer<float>.GetHashCode(float obj)
//    {
//        return obj.GetHashCode();
//    }
//}
//\\\\\\\\\\\\\\\\\\\\\\\\ obsoleted code ends /////////////////////
//------------------------------------------------------------------

namespace Framework.Utils.Unity
{
    public static class Yielders
    {
        static bool _enabled = true;
        static int _internalCounter = 0;
        public static void ClearWaitForSeconds()
        {
            _waitForSecondsYielders.Clear();
        }
    
        static Dictionary<float, WaitForSeconds> _waitForSecondsYielders = new Dictionary<float, WaitForSeconds>(100, new FloatComparer());
        public static WaitForSeconds GetWaitForSeconds(float seconds)
        {
            _internalCounter++;

            if (!_enabled)
                return new WaitForSeconds(seconds);

            WaitForSeconds wfs;
            if (!_waitForSecondsYielders.TryGetValue(seconds, out wfs))
                _waitForSecondsYielders.Add(seconds, wfs = new WaitForSeconds(seconds));
            return wfs;
        }
    
        static WaitForEndOfFrame _endOfFrame = new WaitForEndOfFrame();
        public static WaitForEndOfFrame EndOfFrame
        {
            get { _internalCounter++; return _enabled ? _endOfFrame : new WaitForEndOfFrame(); }
        }

        static WaitForFixedUpdate _fixedUpdate = new WaitForFixedUpdate();
        public static WaitForFixedUpdate FixedUpdate
        {
            get { _internalCounter++; return _enabled ? _fixedUpdate : new WaitForFixedUpdate(); }
        }
    }
}
