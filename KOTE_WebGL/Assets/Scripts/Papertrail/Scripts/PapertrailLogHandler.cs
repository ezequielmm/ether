using System;
using Papertrail;
using UnityEngine;
using Object = UnityEngine.Object;

public class PapertrailLogHandler : ILogHandler, IDisposable
{

    private ILogHandler defaultLogHandler = null;

    public PapertrailLogHandler(ILogHandler unityLogHandler) 
    {
        defaultLogHandler = unityLogHandler;
    }

    public void Dispose()
    {
        if (defaultLogHandler != null && Debug.unityLogger.logHandler != defaultLogHandler) 
        {
            Debug.unityLogger.logHandler = defaultLogHandler;
        }
    }

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
        
        // if there is a log message send it to papertrail
        if (!string.IsNullOrEmpty(message))
        {
            PapertrailLogger.Instance.Application_LogMessageReceived(message, logType);
        }
        
        // only log to console if debugging is enabled
        if (IsLogTypeAllowed(logType))
        {
            defaultLogHandler.LogFormat(logType, context, format, args);
        }
    }


    public void LogException(Exception exception, Object context)
    {
        PapertrailLogger.Instance.Application_LogMessageReceived(exception.Message,  LogType.Exception);
        
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
