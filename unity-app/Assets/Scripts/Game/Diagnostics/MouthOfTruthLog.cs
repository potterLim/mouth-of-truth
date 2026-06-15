using System.Diagnostics;

namespace MouthOfTruth.Game.Diagnostics
{
    internal static class MouthOfTruthLog
    {
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        internal static void LogInfo(string message)
        {
            UnityEngine.Debug.Log(message);
        }
    }
}
