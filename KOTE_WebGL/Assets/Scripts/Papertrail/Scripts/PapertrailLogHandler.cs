using System;
using Papertrail;
using UnityEditor.PackageManager;
using UnityEngine;
using Object = UnityEngine.Object;

public class PapertrailLogHandler : ILogHandler
{

    private ILogHandler defaultLogHandler = Debug.unityLogger.logHandler;
    
    public void LogFormat(LogType logType, Object context, string format, params object[] args)
    {
        // format the log message into a single string
        string message = "";
        foreach (object argObject in args)
        {
            
            if (argObject is string)
            {
                message = message + argObject + " ";
            }
        }
        
        // pull the stack trace
        string stackTrace = StackTraceUtility.ExtractStackTrace();
        
        // if there is a log message send it to papertrail
        if (!string.IsNullOrEmpty(message))
        {
            PapertrailLogger.Instance.Application_LogMessageReceived(message, stackTrace, logType);
        }
        
        // only log to console if debugging is enabled
        if (IsLogTypeAllowed(GameSettings.FilterLogType))
        {
            defaultLogHandler.LogFormat(logType, context, format, args);
        }
    }


    public void LogException(Exception exception, Object context)
    {
        PapertrailLogger.Instance.Application_LogMessageReceived(exception.Message, StackTraceUtility.ExtractStackTrace(), LogType.Exception);
        
        defaultLogHandler.LogException(exception, context);
    }
    
    // this function is the same as Debug.unityLogger.IsLogTypeAllowed, redone to use our own FilterLogType
    private bool IsLogTypeAllowed(LogType logType)
    {
            if (logType == LogType.Exception)
                return true;
            if (GameSettings.FilterLogType != LogType.Exception)
                return logType <= GameSettings.FilterLogType;
           
            return false;
    }
}
