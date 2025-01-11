using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldSelect : MonoBehaviour
{
    public static WorldSelect instance;

    public Transform worldButtonContainer; // Container para os botões dos mundos
    public RectTransform worldContent;
    public Button worldButtonPrefab; // Prefab do botão do mundo

    [HideInInspector] public string levelEditorPath; // Caminho para a pasta "LevelEditor"

    private List<string> availableWorlds; // Lista de mundos disponíveis
    private string currentWorldName; // Nome do mundo atual

    public GameObject currentPanel; // Referência para o painel atual que está sendo exibido
    public GameObject worldListPanel; // Referência para o painel com a lista de mundos
    public GameObject LevelListPanel;

    private bool listUpdated;


    //Levels
    public GameObject buttonBackWorldList;
    public Transform levelButtonContainer; // Container para os botões dos níveis
    public RectTransform levelContent;
    public Button levelButtonPrefab; // Prefab do botão do nível
    public string currentLevelName;

    //Game and extra levels game
    public bool isExtraLevels;
    public bool isGameLevels;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }


        if (!isExtraLevels && !isGameLevels)
        {
            // Obtém o caminho completo para a pasta "LevelEditor"
            levelEditorPath = Path.Combine(Application.persistentDataPath, "LevelEditor");
            // Carrega os mundos disponíveis
            LoadAvailableWorlds();
        }
        if(isExtraLevels)
        {
            levelEditorPath = Path.Combine(Application.streamingAssetsPath, "Worlds/ExtraWorlds");
            LoadAvailableWorlds();
        }
        if(isGameLevels)
        {
            levelEditorPath = Path.Combine(Application.streamingAssetsPath, "Worlds/GameWorlds");
            LoadAvailableWorlds();
        }
    }

    public void ShowWorldList()
    {
        // Verifica se o container está definido e é visível
        if (worldButtonContainer == null || !worldButtonContainer.gameObject.activeSelf)
        {
            UnityEngine.Debug.LogWarning("O container de botões do mundo não está definido ou não está visível.");
            return;
        }

        // Carrega os mundos disponíveis
        LoadAvailableWorlds();

        // Limpa os botões de mundo existentes
        foreach (Transform child in worldButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // Instancia os botões dos mundos disponíveis
        foreach (string worldName in availableWorlds)
        {
            // Cria um novo botão de mundo
            Button worldButton = Instantiate(worldButtonPrefab, worldButtonContainer);
            worldButton.GetComponentInChildren<TextMeshProUGUI>().text = worldName;

            // Configura o callback de clique para selecionar o mundo correspondente
            string world = worldName; // Variável temporária para evitar closure
            worldButton.onClick.AddListener(() => SelectWorld(world));
        }

        // Atualiza o tamanho vertical do Content do ScrollView de acordo com o número de botões de mundos
        float buttonHeight = worldButtonPrefab.GetComponent<RectTransform>().rect.height;
        float spacing = 130f; // O valor de spacing do seu Horizontal Layout Group
        float totalHeight = (availableWorlds.Count * buttonHeight) + (spacing * (availableWorlds.Count - 1));
        worldContent.sizeDelta = new Vector2(worldContent.sizeDelta.x, totalHeight);

        // Fecha o painel atual
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
        }

        // Abre o painel com a lista de mundos
        if (worldListPanel != null)
        {
            worldListPanel.SetActive(true);
        }
    }

    private void LoadAvailableWorlds()
    {
        // Verifica se a pasta "LevelEditor" existe
        if (Directory.Exists(levelEditorPath))
        {
            // Obtém todas as pastas dentro de "LevelEditor"
            string[] worldFolders = Directory.GetDirectories(levelEditorPath);

            // Cria uma nova lista de mundos disponíveis
            availableWorlds = new List<string>();

            // Itera sobre todas as pastas de mundo
            foreach (string worldFolder in worldFolders)
            {
                // Verifica se a pasta do mundo contém o arquivo de informações do mundo
                string worldInfoFilePath = Path.Combine(worldFolder, "World.Info");
                if (File.Exists(worldInfoFilePath))
                {
                    // Lê o nome do mundo do arquivo de informações do mundo
                    string worldInfo = File.ReadAllText(worldInfoFilePath);
                    string[] lines = worldInfo.Split('\n');
                    string worldName = lines[0].Substring(6);

                    // Adiciona o nome do mundo à lista de mundos disponíveis
                    availableWorlds.Add(worldName);
                    if (!listUpdated)
                    {
                        listUpdated = true;
                        ShowWorldList();
                    }
                }
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("A pasta 'LevelEditor' não existe!");
        }
    }

    public void SelectWorld(string worldName)
    {
        // Define o nome do mundo selecionado
        currentWorldName = worldName;

        // Obtém o caminho completo para o arquivo de informações do mundo
        string worldInfoFilePath = Path.Combine(levelEditorPath, currentWorldName, "World.Info");

        // Verifica se o arquivo de informações do mundo existe
        if (File.Exists(worldInfoFilePath))
        {
            // Lê o conteúdo do arquivo de informações do mundo
            string worldInfo = File.ReadAllText(worldInfoFilePath);

            // Verifica se a linha "WithWorldmap" está presente e seu valor
            if (worldInfo.Contains("WithWorldmap: True"))
            {
                PlayWorld.instance.isWorldmap = true;
                // Define o nome do mundo selecionado na variável selectedWorldName
                PlayWorld.instance.selectedWorldName = currentWorldName;

                // Abra a cena do mundo em uma nova cena
                UnityEngine.SceneManagement.SceneManager.LoadScene("PlayWorld");
            }
            else if (worldInfo.Contains("WithWorldmap: False"))
            {
                buttonBackWorldList.SetActive(true);
                PlayWorld.instance.isWorldmap = false;
                // Define o nome do mundo selecionado na variável selectedWorldName
                PlayWorld.instance.selectedWorldName = currentWorldName;
                InstantiateLevelButtons(currentWorldName);
                // Mostre a lista de níveis contidos no mundo (você pode implementar isso)
                // Por exemplo, ative um painel que exibe a lista de níveis.
                currentPanel.SetActive(false);
                LevelListPanel.SetActive(true);
            }
        }

    }


    //levels

    public void InstantiateLevelButtons(string worldName)
    {
        // Verifica se o container dos botões de nível está definido
        if (levelButtonContainer == null)
        {
            UnityEngine.Debug.LogWarning("O container de botões de nível não está definido.");
            return;
        }

        // Remove os botões de nível existentes
        foreach (Transform child in levelButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // Obtém o caminho completo para a pasta do mundo
        string worldFolderPath = Path.Combine(levelEditorPath, worldName);

        // Verifica se a pasta do mundo existe
        if (Directory.Exists(worldFolderPath))
        {
            // Obtém todos os arquivos com a extensão ".TAOWLE" dentro da pasta do mundo
            string[] levelFiles = Directory.GetFiles(worldFolderPath, "*.TAOWLE");

            // Instancia um botão para cada nível disponível
            foreach (string levelFile in levelFiles)
            {
                // Obtém o nome do nível
                string levelName = Path.GetFileNameWithoutExtension(levelFile);

                // Cria um novo botão de nível
                Button levelButton = Instantiate(levelButtonPrefab, levelButtonContainer);
                levelButton.GetComponentInChildren<TextMeshProUGUI>().text = levelName;

                // Configura o callback de clique para abrir o nível correspondente
                string level = levelName; // Variável temporária para evitar closure
                levelButton.onClick.AddListener(() =>
                {
                    // Define o nome do nível selecionado
                    currentLevelName = level;

                    // Chame a função que carrega o nível ou execute qualquer outra ação desejada com o nível selecionado
                    PlayWorld.instance.selectedLevelName = currentLevelName;

                    UnityEngine.SceneManagement.SceneManager.LoadScene("PlayLevel");
                    // Fecha o painel atual
                    if (LevelListPanel != null)
                    {
                        LevelListPanel.SetActive(false);
                        worldListPanel.SetActive(false);
                    }

                });

                // Ativa o botão
                levelButton.gameObject.SetActive(true);
            }

            // Atualiza o tamanho vertical do Content do ScrollView de acordo com o número de botões
            float buttonHeight = levelButtonPrefab.GetComponent<RectTransform>().rect.height;
            float spacing = 130f; // O valor de spacing do seu Horizontal Layout Group
            float totalHeight = (levelFiles.Length * buttonHeight) + (spacing * (levelFiles.Length - 1));
            levelContent.sizeDelta = new Vector2(levelContent.sizeDelta.x, totalHeight);
        }
        else
        {
            UnityEngine.Debug.LogWarning("A pasta do mundo não existe: " + worldFolderPath);
        }
    }


}
