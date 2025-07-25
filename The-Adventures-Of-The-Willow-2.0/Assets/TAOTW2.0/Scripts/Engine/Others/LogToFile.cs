using UnityEngine;
using System.IO;

public class LogToFile : MonoBehaviour
{
    private static LogToFile instance;
    private string logFilePath;

    void Awake()
    {
        // Garante que só existe um logger
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Caminho para salvar o log
        logFilePath = Path.Combine(Application.persistentDataPath, "log.txt");

        // Inicia com um cabeçalho novo
        File.WriteAllText(logFilePath, $"[LOG INICIADO] {System.DateTime.Now}\n");

        // Liga o listener
        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy()
    {
        if (instance == this)
            Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        string logEntry = $"[{System.DateTime.Now:HH:mm:ss}] [{type}] {logString}";

        // StackTrace apenas para erros e exceções
        if (type == LogType.Error || type == LogType.Exception)
            logEntry += $"\n{stackTrace}";

        File.AppendAllText(logFilePath, logEntry + "\n");
    }
}