using System.Diagnostics;

public class OnlyDebug
{
    [Conditional("DEBUG")]
    static public void Log(object message) {
#if DEBUG
        UnityEngine.Debug.Log(message);
#endif
    }

    [Conditional("DEBUG")]
    static public void LogWarning(object message) {
#if DEBUG
        UnityEngine.Debug.LogWarning(message);
#endif
    }

    [Conditional("DEBUG")]
    static public void LogError(object message) {
#if DEBUG
        UnityEngine.Debug.LogError(message);
#endif
    }



}
