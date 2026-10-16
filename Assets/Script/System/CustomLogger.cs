using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using Debug = UnityEngine.Debug;

public enum LogLevel
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3,
    None = 4
}

/// <summary>
/// ログ出力をまとめるクラス。Debug.Log を直接呼ばず、こちらを経由して出力する。
/// </summary>
public static class CustomLogger
{
    // 現在のログレベル（これより優先度の低いログは無視される）
    public static LogLevel CurrentLogLevel = LogLevel.Debug;

    // ファイル出力を有効にするか
    public static bool EnableFileOutput = false;

    // ログファイルの出力先パス（Application.persistentDataPath は静的初期化時に呼べないため遅延取得する）
    private static string _logFilePath;
    private static string LogFilePath =>
        _logFilePath ??= Path.Combine(Application.persistentDataPath, "app_log.txt");

    /// <summary>
    /// デバッグログ（開発用の詳細情報）。
    /// エディタ・Development Build・ENABLE_LOG シンボル定義時のみ有効で、
    /// それ以外のビルドでは呼び出し自体がコンパイル時に除去される。
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("ENABLE_LOG")]
    public static void LogDebug(string message, [CallerFilePath] string filePath = "")
    {
        Log(LogLevel.Debug, message, filePath);
    }

    /// <summary>
    /// 一般ログ
    /// </summary>
    public static void LogInfo(string message, [CallerFilePath] string filePath = "")
    {
        Log(LogLevel.Info, message, filePath);
    }

    /// <summary>
    /// 警告ログ
    /// </summary>
    public static void LogWarning(string message, [CallerFilePath] string filePath = "")
    {
        Log(LogLevel.Warning, message, filePath);
    }

    /// <summary>
    /// エラーログ
    /// </summary>
    public static void LogError(string message, [CallerFilePath] string filePath = "")
    {
        Log(LogLevel.Error, message, filePath);
    }

    private static void Log(LogLevel level, string message, string filePath)
    {
        if (level < CurrentLogLevel) return;

        // 呼び出し元のクラス名をファイルパスから抽出
        string className = Path.GetFileNameWithoutExtension(filePath);
        string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string formattedMessage = $"[{timeStamp}] [{level.ToString().ToUpper()}] [{className}] {message}";

        // Unityコンソールへ出力
        switch (level)
        {
            case LogLevel.Debug:
            case LogLevel.Info:
                Debug.Log(formattedMessage);
                break;
            case LogLevel.Warning:
                Debug.LogWarning(formattedMessage);
                break;
            case LogLevel.Error:
                Debug.LogError(formattedMessage);
                break;
        }

        // テキストファイルへ書き込み（オプション）
        if (EnableFileOutput)
        {
            WriteToFile(formattedMessage);
        }
    }

    private static void WriteToFile(string logMessage)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(LogFilePath, true))
            {
                writer.WriteLine(logMessage);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"ログファイルへの書き込みに失敗しました: {ex.Message}");
        }
    }
}
