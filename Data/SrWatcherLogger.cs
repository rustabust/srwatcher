namespace sr_watcher.Data
{
    public class SrWatcherLogger //: ILogger
    {
        
        public static void LogInfo(string message)
        {
            Log(LogLevel.Information, message);
        }

        public static void LogWarning(string message)
        {
            Log(LogLevel.Warning, message);
        }

        public static void LogError(string message, Exception ex) 
        {
            Log(LogLevel.Error, message, ex);
        }

        public static void Log(LogLevel logLevel, string message, Exception? ex = null) 
        {
            System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToString()}:{logLevel.ToString()} - {message}");

            if (ex != null )
            {
                System.Diagnostics.Debug.WriteLine(" " + ex.Message);
                System.Diagnostics.Debug.WriteLine(" " + ex.StackTrace);
            }
        } 
    }
}
