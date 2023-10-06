using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using TMPro;

//current
#region Save data level and world classes
[System.Serializable]
public class CurrentSaveLevelData
{
    public string CurrentLevelName;
    public bool currentLevelsolved;
    public int currentLevelCoinsCollected;
    public int currentLevelDeaths;
    public int currentLevelEnemiesKilled;
    public float currentLevelCompletionTime;
}

[System.Serializable]
public class CurrentSaveWorldData
{
    public string worldName;
    public List<CurrentSaveLevelData> levels = new List<CurrentSaveLevelData>();
}

//Save
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
    public string actualPlayerWorldName; // Para ignorar
    public SavePlayerData player = new SavePlayerData();
    public List<SaveWorldData> worlds = new List<SaveWorldData>();
}
#endregion
public class SaveGameManager : MonoBehaviour
{
    public static SaveGameManager instance;

    public SaveGameData saveData = new SaveGameData();
    public CurrentSaveLevelData currentSaveLevelData = new CurrentSaveLevelData();
    public CurrentSaveWorldData currentSaveWorldData = new CurrentSaveWorldData();

    public string currentWorldName; // Nome atual do mundo
    public string currentLevelName; // Nome atual do nível


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

    void Start()
    {
        if (PlayWorld.instance != null)
        {
            SetCurrentWorldAndLevel(PlayWorld.instance.selectedWorldName, PlayWorld.instance.selectedLevelName);
        }
        LoadGame();
    }

    public void SetCurrentWorldAndLevel(string worldName, string levelName)
    {
        currentWorldName = worldName;
        currentLevelName = levelName;

        // Atualize o nome do mundo atual diretamente no objeto currentWorld
        CurrentSaveWorldData currentWorld = currentSaveWorldData;
        currentWorld.worldName = worldName;

        // Verifique se o nível atual já existe na lista de níveis do mundo atual
        CurrentSaveLevelData currentLevel = currentWorld.levels.Find(level => level.CurrentLevelName == levelName);

        if (currentLevel == null)
        {
            // Se o nível atual não existir, crie-o
            currentLevel = new CurrentSaveLevelData
            {
                CurrentLevelName = levelName
            };
            currentWorld.levels.Add(currentLevel);
        }
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

        // Verificar se o nível atual já existe na lista de níveis do mundo atual
        SaveLevelData currentLevel = currentWorld.levels.Find(level => level.levelName == currentLevelName);

        if (currentLevel == null)
        {
            // Se o nível atual não existir, crie-o
            currentLevel = new SaveLevelData
            {
                levelName = currentLevelName
            };
            currentWorld.levels.Add(currentLevel);
        }

        // Atualize os dados do nível atual
        currentLevel.solved = true;
        currentLevel.coinsCollected = currentSaveLevelData.currentLevelCoinsCollected;
        currentLevel.deaths = currentSaveLevelData.currentLevelDeaths;
        currentLevel.enemiesKilled = currentSaveLevelData.currentLevelEnemiesKilled;
        currentLevel.completionTime = currentSaveLevelData.currentLevelCompletionTime;

        // Atualize os dados do jogador
        saveData.player.coins = CoinCollect.instance.coin;
        //saveData.player.position = PlayerManager.instance.playerPosition;

        // Agora, você pode salvar o jogo no arquivo correspondente
        string jsonToSave = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(GetSavePath(), jsonToSave);
    }

    private string GetSavePath()
    {
        // Caminho completo para o arquivo de salvamento
        string saveFolder = Path.Combine(Application.persistentDataPath, ProfileManager.instance.selectedProfile);
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }
        // Use o nome do mundo atual diretamente como o nome do arquivo
        return Path.Combine(saveFolder, currentWorldName + ".TAOWLS");
    }

    public void LoadGame()
    {
        string savePath = GetSavePath();
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            saveData = JsonUtility.FromJson<SaveGameData>(json);

            if(CoinCollect.instance != null)
            {
                // Carregue as informações do jogador
                CoinCollect.instance.coin = saveData.player.coins;
                //PlayerManager.instance.playerPosition = saveData.player.position;
            }

            // Encontre o mundo atual
            SaveWorldData currentWorld = saveData.worlds.Find(world => world.worldName == currentWorldName);

            if (currentWorld != null)
            {
                // Encontre o mundo atual e carregue seus níveis
                currentSaveWorldData.worldName = currentWorld.worldName;
                currentSaveWorldData.levels = currentWorld.levels.Select(level => new CurrentSaveLevelData
                {
                    CurrentLevelName = level.levelName,
                    currentLevelsolved = level.solved,
                    currentLevelCoinsCollected = level.coinsCollected,
                    currentLevelDeaths = level.deaths,
                    currentLevelEnemiesKilled = level.enemiesKilled,
                    currentLevelCompletionTime = level.completionTime
                }).ToList();
            }
        }
    }
   
    public void LevelCompleted()
    {
        // Define os valores finais do nível atual
        currentSaveLevelData.currentLevelsolved = true; // Indica que o nível foi concluído com sucesso
        currentSaveLevelData.currentLevelCoinsCollected = PlayerManager.instance.coinsCollected;
        currentSaveLevelData.currentLevelDeaths = PlayerManager.instance.deaths;
        currentSaveLevelData.currentLevelEnemiesKilled = PlayerManager.instance.enemiesKilled;
        currentSaveLevelData.currentLevelCompletionTime = LevelTimeManager.instance.elapsedTime;

        // Salva o jogo após a conclusão do nível
        SaveGame();
    }
}
