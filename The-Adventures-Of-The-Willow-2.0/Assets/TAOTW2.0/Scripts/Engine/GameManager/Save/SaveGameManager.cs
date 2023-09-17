using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class SaveLevelData
{
    public string levelName;
    public bool solved;
    public int coinsCollected;
    public int deaths;
    public int enemiesKilled;
    public float completionTime;
}

[System.Serializable]
public class SavePlayerData
{
    public int coins;
    public Vector2 position;
}

[System.Serializable]
public class SaveWorldData
{
    public string worldName;
    public List<SaveLevelData> levels = new List<SaveLevelData>();
}

[System.Serializable]
public class SaveGameData
{
    public string title;
    public SavePlayerData player = new SavePlayerData();
    public List<SaveWorldData> worlds = new List<SaveWorldData>();
}

public class SaveGameManager : MonoBehaviour
{
    public static SaveGameManager instance;

    public SaveGameData saveData = new SaveGameData();

    private string currentWorldName; // Nome atual do mundo
    private string currentLevelName; // Nome atual do nível

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

    public void SetCurrentWorldAndLevel(string worldName, string levelName)
    {
        currentWorldName = worldName;
        currentLevelName = levelName;
    }

    public void SaveGame()
    {
        // Procurar o mundo atual no saveData
        SaveWorldData currentWorld = saveData.worlds.Find(world => world.worldName == currentWorldName);

        // Se o mundo atual não existir, criá-lo
        if (currentWorld == null)
        {
            currentWorld = new SaveWorldData { worldName = currentWorldName };
            saveData.worlds.Add(currentWorld);
        }

        // Procurar o nível atual no mundo atual
        SaveLevelData currentLevel = currentWorld.levels.Find(level => level.levelName == currentLevelName);

        // Se o nível atual não existir, criá-lo
        if (currentLevel == null)
        {
            currentLevel = new SaveLevelData { levelName = currentLevelName };
            currentWorld.levels.Add(currentLevel);
        }

        // Preencher os dados do nível atual
        currentLevel.solved = true; // Você pode definir isso com base em alguma lógica do seu jogo
        currentLevel.coinsCollected = PlayerManager.instance.coinsCollected;
        currentLevel.deaths = PlayerManager.instance.deaths;
        currentLevel.enemiesKilled = PlayerManager.instance.enemiesKilled;
        currentLevel.completionTime = PlayerManager.instance.completionTime;

        // Agora, você pode salvar o jogo em um arquivo específico para o mundo
        string worldSavePath = GetWorldSavePath(currentWorldName);
        string json = JsonUtility.ToJson(currentWorld, true);
        File.WriteAllText(worldSavePath, json);
    }

    public void LoadGame()
    {
        // Carrega os dados de todos os mundos salvos
        foreach (SaveWorldData worldData in saveData.worlds)
        {
            string worldSavePath = GetWorldSavePath(worldData.worldName);
            if (File.Exists(worldSavePath))
            {
                string json = File.ReadAllText(worldSavePath);
                SaveWorldData loadedWorldData = JsonUtility.FromJson<SaveWorldData>(json);

                // Substitui o mundo existente com os dados carregados
                int existingWorldIndex = saveData.worlds.FindIndex(world => world.worldName == worldData.worldName);
                if (existingWorldIndex >= 0)
                {
                    saveData.worlds[existingWorldIndex] = loadedWorldData;
                }
            }
        }
    }

    private string GetWorldSavePath(string worldName)
    {
        string saveFolder = Path.Combine(Application.persistentDataPath, ProfileManager.instance.selectedProfile);
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }
        return Path.Combine(saveFolder, worldName + ".TAOWLS");
    }
}
