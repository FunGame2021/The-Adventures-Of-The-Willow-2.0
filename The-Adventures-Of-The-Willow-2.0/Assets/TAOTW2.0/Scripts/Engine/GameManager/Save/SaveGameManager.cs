using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class SaveLevelData
{
    public string levelName;
    public bool solved;
    public Dictionary<string, int> statistics = new Dictionary<string, int>();
}

[System.Serializable]
public class SavePlayerData
{
    public int coins;
    public Vector2 position;
}

[System.Serializable]
public class SaveGameData
{
    public string title;
    public SavePlayerData player = new SavePlayerData();
    public List<SaveLevelData> levels = new List<SaveLevelData>();
}

public class SaveGameManager : MonoBehaviour
{
    public static SaveGameManager instance;

    public SaveGameData saveData = new SaveGameData();

    private string currentWorldName; // Nome atual do mundo

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetCurrentWorldName(string worldName)
    {
        currentWorldName = worldName;
    }

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(GetSavePath(), json);
    }

    public void LoadGame()
    {
        if (File.Exists(GetSavePath()))
        {
            string json = File.ReadAllText(GetSavePath());
            saveData = JsonUtility.FromJson<SaveGameData>(json);
        }
    }

    private string GetSavePath()
    {
        string saveFolder = Path.Combine(Application.persistentDataPath, ProfileManager.instance.selectedProfile);
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }
        return Path.Combine(saveFolder, currentWorldName + ".TAOWLS");
    }
}