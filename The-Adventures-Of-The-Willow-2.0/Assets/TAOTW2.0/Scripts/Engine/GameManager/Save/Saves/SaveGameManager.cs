using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
    public int stars;
    public Vector2 position;
    public bool isBig;
    public bool isSmall;
    public bool isFirePower;
    public bool isAirPower;
    public bool isBubblePower;
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

    [HideInInspector] public bool asWorldData;
    [HideInInspector] public Vector3 loadedPlayerWorldPosition;
    private Vector3 currentPlayerOnWorldPos;
    public int TotalCoins;
    [SerializeField] private bool isWorldmapPlayed;

    [HideInInspector] public bool isSaveBig;
    [HideInInspector] public bool isSaveSmall;
    [HideInInspector] public bool isSaveFirePower;
    [HideInInspector] public bool isSaveAirPower;
    [HideInInspector] public bool isSaveBubblePower;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        if (PlayWorld.instance != null)
        {
            SetCurrentWorldAndLevel(PlayWorld.instance.selectedWorldName, PlayWorld.instance.selectedLevelName);
        }
        CoinCollect coinCollect = FindAnyObjectByType<CoinCollect>();
        if (coinCollect != null)
        {
            CoinCollect.instance = coinCollect;
        }
        LoadGame();

    }
    void Update()
    {
        GameObject PlayerObject = GameObject.FindGameObjectWithTag("Player");
        if (PlayerObject != null)
        {
            currentPlayerOnWorldPos = PlayerObject.transform.position;
        }
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
            saveData.player = new SavePlayerData
            {
                isBig = true,  // Valor padrão para o estado inicial
                isSmall = false,
                isFirePower = false,
                isAirPower = false,
                isBubblePower = false
            };
        }

        // Verificar se um nível está ativo antes de salvá-lo
        if (!isWorldmapPlayed)
        {
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

                Debug.Log("New SaveLevelData created.");
            }

            // Atualize os dados do nível atual
            currentLevel.solved = currentSaveLevelData.currentLevelsolved;
            currentLevel.coinsCollected = currentSaveLevelData.currentLevelCoinsCollected;
            currentLevel.deaths = currentSaveLevelData.currentLevelDeaths;
            currentLevel.enemiesKilled = currentSaveLevelData.currentLevelEnemiesKilled;
            currentLevel.completionTime = currentSaveLevelData.currentLevelCompletionTime;

        }
        else
        {
            // Se estiver no mapa do mundo, atualize a posição do jogador
            saveData.player.position = currentPlayerOnWorldPos;
        }
        if (PlayerStates.instance != null)
        {
            // Atualize os dados do jogador
            saveData.player.coins = TotalCoins;
            saveData.player.isSmall = PlayerStates.instance.isSmall;
            saveData.player.isBig = PlayerStates.instance.isBig;
            saveData.player.isAirPower = PlayerStates.instance.isAirPower;
            saveData.player.isBubblePower = PlayerStates.instance.isBubblePower;
            saveData.player.isFirePower = PlayerStates.instance.isFirePower;
        }

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

            // Encontre o mundo atual
            SaveWorldData currentWorld = saveData.worlds.Find(world => world.worldName == currentWorldName);

            TotalCoins = saveData.player.coins;
            CoinCollect.instance.SaveChangeCoin(saveData.player.coins);
            // Carregue as informações do jogador
            loadedPlayerWorldPosition = saveData.player.position;
            asWorldData = true;

            isSaveBig = saveData.player.isBig;
            isSaveSmall = saveData.player.isSmall;
            isSaveBubblePower = saveData.player.isBubblePower;
            isSaveFirePower = saveData.player.isFirePower;
            isSaveAirPower = saveData.player.isAirPower;


            if (currentWorld != null)
            {
                // Encontre o mundo atual e carregue todos os seus níveis
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
        else
        {
            // Arquivo de salvamento não existe. Inicialize valores padrão.
            saveData = new SaveGameData
            {
                player = new SavePlayerData
                {
                    isBig = true,  // Valor padrão para o estado inicial
                    isSmall = false,
                    isFirePower = false,
                    isAirPower = false,
                    isBubblePower = false
                }
            };

            Debug.Log("Novo jogo iniciado com valores padrão.");
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

        TotalCoins = CoinCollect.instance.coin;

        // Encontre o nível atual na lista de níveis do mundo atual
        CurrentSaveLevelData currentLevel = currentSaveWorldData.levels.Find(level => level.CurrentLevelName == currentLevelName);

        if (currentLevel != null)
        {
            // Se o nível atual já existir na lista, atualize suas estatísticas
            currentLevel.currentLevelsolved = currentSaveLevelData.currentLevelsolved;
            currentLevel.currentLevelCoinsCollected = currentSaveLevelData.currentLevelCoinsCollected;
            currentLevel.currentLevelDeaths = currentSaveLevelData.currentLevelDeaths;
            currentLevel.currentLevelEnemiesKilled = currentSaveLevelData.currentLevelEnemiesKilled;
            currentLevel.currentLevelCompletionTime = currentSaveLevelData.currentLevelCompletionTime;
        }
        else
        {
            // Se o nível atual não existir na lista, adicione-o
            currentSaveWorldData.levels.Add(currentSaveLevelData);
        }

        // Salva o jogo após a conclusão do nível
        SaveGame();
    }
    public bool IsLevelSolved(string worldName, string levelName)
    {
        // Encontre o mundo atual
        CurrentSaveWorldData currentWorld = currentSaveWorldData;

        // Verifique se o mundo atual existe
        if (currentWorld != null && currentWorld.worldName == worldName)
        {
            // Encontre o nível atual no mundo atual
            CurrentSaveLevelData currentLevel = currentWorld.levels.Find(level => level.CurrentLevelName == levelName);

            // Se o nível atual existir, retorne se foi concluído ou não
            if (currentLevel != null)
            {
                return currentLevel.currentLevelsolved;
            }
        }

        // Se o mundo ou o nível não foram encontrados, retorne false
        return false;
    }

}
