using System;
using System.IO;
using System.Threading.Tasks;

public static class Logger
{
    public static void SetLogger(LOGTYPE logType, string log)
    {
        WriteLog(logType, log);
    }
    public static void SetLogger(LOGTYPE logType, float log)
    {
        WriteLog(logType, log.ToString());
    }
    public static void SetLogger(LOGTYPE logType, int log)
    {
        WriteLog(logType, log.ToString());
    }
    public static void SetLogger(LOGTYPE logType, double log)
    {
        WriteLog(logType, log.ToString());
    }
    public static void SetLogger(LOGTYPE logType, long log)
    {
        WriteLog(logType, log.ToString());
    }
    public static void SetLogger(LOGTYPE logType, char log)
    {
        WriteLog(logType, log.ToString());
    }
    public static async Task SetLoggerAsync(LOGTYPE logType, string log)
    {
        await WriteLogAsync(logType, log);
    }
    public static async Task SetLoggerAsync(LOGTYPE logType, float log)
    {
        await WriteLogAsync(logType, log.ToString());
    }
    public static async Task SetLoggerAsync(LOGTYPE logType, int log)
    {
        await WriteLogAsync(logType, log.ToString());
    }
    public static async Task SetLoggerAsync(LOGTYPE logType, double log)
    {
        await WriteLogAsync(logType, log.ToString());
    }
    public static async Task SetLoggerAsync(LOGTYPE logType, long log)
    {
        await WriteLogAsync(logType, log.ToString());
    }
    public static async Task SetLoggerAsync(LOGTYPE logType, char log)
    {
        await WriteLogAsync(logType, log.ToString());
    }

    private static string SetLogType(LOGTYPE type)
    {
        return type switch
        {
            LOGTYPE.INFO => "INFO",
            LOGTYPE.DEBUG => "DEBUG",
            LOGTYPE.WARNING => "WARNING",
            LOGTYPE.ERROR => "ERROR",
            _ => ""
        };
    }

    private static string GetLogDirectory()
    {
        // 현재 작업 디렉터리를 가져옴
        string currentDirectory = Directory.GetCurrentDirectory();
        string logDirectory = Path.Combine(currentDirectory, "Logs");

        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        string logFilePath = Path.Combine(logDirectory, "Log.txt");
        return logFilePath;
    }

    private static void WriteLog(LOGTYPE logType, string log)
    {
        string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{SetLogType(logType)}] {log}";

        if (logType == LOGTYPE.ERROR)
        {
            logMessage += $"{Environment.NewLine}StackTrace: {Environment.StackTrace}";
        }

        string logFilePath = GetLogDirectory();

        lock (typeof(Logger))  // 멀티스레드 환경에서 안전하게 파일에 접근하기 위해 lock 사용
        {
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }
    }

    private static async Task WriteLogAsync(LOGTYPE logType, string log)
    {
        string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{SetLogType(logType)}] {log}";

        if (logType == LOGTYPE.ERROR)
        {
            logMessage += $"{Environment.NewLine}StackTrace: {Environment.StackTrace}";
        }

        string logFilePath = GetLogDirectory();

        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            await writer.WriteLineAsync(logMessage);
        }
    }
}

public enum LOGTYPE
{
    INFO,
    DEBUG,
    WARNING,
    ERROR,
}
