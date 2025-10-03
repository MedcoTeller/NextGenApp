using SharedLoggerClientLib;

namespace GlobalShared
{
    public class Utils
    {
        private static SharedLogClient Logger = new SharedLogClient("NextGen");
        private string SubsystemName;

        public Utils(string subSystemName)
        {
            SubsystemName = subSystemName;
            LogInfo($"Subsystem {subSystemName} initialized.");
        }

        #region Logger
        public void LogInfo(string message)
        {
            Logger.Log(SubsystemName, SharedLoggerLib.LogLevel.Info, message);
        }

        public void LogError(string message)
        {
            Logger.Log(SubsystemName, SharedLoggerLib.LogLevel.Error, message);
        }

        public void LogDebug(string message)
        {
            Logger.Log(SubsystemName, SharedLoggerLib.LogLevel.Debug, message);
        }

        public void LogWarning(string message)
        {
            Logger.Log(SubsystemName, SharedLoggerLib.LogLevel.Warning, message);
        }

        public void LogFatal(string message)
        {
            Logger.Log(SubsystemName, SharedLoggerLib.LogLevel.Fatal, message);
        }
        #endregion


    }
}
