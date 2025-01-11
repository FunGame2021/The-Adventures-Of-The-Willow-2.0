using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldSelect : MonoBehaviour
{
    public static WorldSelect instance;

    public Transform worldButtonContainer; // Container para os bot�es dos mundos
    public RectTransform worldContent;
    public Button worldButtonPrefab; // Prefab do bot�o do mundo

    [HideInInspector] public string levelEditorPath; // Caminho para a pasta "LevelEditor"

    private List<string> availableWorlds; // Lista de mundos dispon�veis
    private string currentWorldName; // Nome do mundo atual

    public GameObject currentPanel; // Refer�ncia para o painel atual que est� sendo exibido
    public GameObject worldListPanel; // Refer�ncia para o painel com a lista de mundos
    public GameObject LevelListPanel;

    private bool listUpdated;


    //Levels
    public GameObject buttonBackWorldList;
    public Transform levelButtonContainer; // Container para os bot�es dos n�veis
    public RectTransform levelContent;
    public Button levelButtonPrefab; // Prefab do bot�o do n�vel
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
            // Obt�m o caminho completo para a pasta "LevelEditor"
            levelEditorPath = Path.Combine(Application.persistentDataPath, "LevelEditor");
            // Carrega os mundos dispon�veis
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
        // Verifica se o container est� definido e � vis�vel
        if (worldButtonContainer == null || !worldButtonContainer.gameObject.activeSelf)
        {
            UnityEngine.Debug.LogWarning("O container de bot�es do mundo n�o est� definido ou n�o est� vis�vel.");
            return;
        }

        // Carrega os mundos dispon�veis
        LoadAvailableWorlds();

        // Limpa os bot�es de mundo existentes
        foreach (Transform child in worldButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // Instancia os bot�es dos mundos dispon�veis
        foreach (string worldName in availableWorlds)
        {
            // Cria um novo bot�o de mundo
            Button worldButton = Instantiate(worldButtonPrefab, worldButtonContainer);
            worldButton.GetComponentInChildren<TextMeshProUGUI>().text = worldName;

            // Configura o callback de clique para selecionar o mundo correspondente
            string world = worldName; // Vari�vel tempor�ria para evitar closure
            worldButton.onClick.AddListener(() => SelectWorld(world));
        }

        // Atualiza o tamanho vertical do Content do ScrollView de acordo com o n�mero de bot�es de mundos
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
            // Obt�m todas as pastas dentro de "LevelEditor"
            string[] worldFolders = Directory.GetDirectories(levelEditorPath);

            // Cria uma nova lista de mundos dispon�veis
            availableWorlds = new List<string>();

            // Itera sobre todas as pastas de mundo
            foreach (string worldFolder in worldFolders)
            {
                // Verifica se a pasta do mundo cont�m o arquivo de informa��es do mundo
                string worldInfoFilePath = Path.Combine(worldFolder, "World.Info");
                if (File.Exists(worldInfoFilePath))
                {
                    // L� o nome do mundo do arquivo de informa��es do mundo
                    string worldInfo = File.ReadAllText(worldInfoFilePath);
                    string[] lines = worldInfo.Split('\n');
                    string worldName = lines[0].Substring(6);

                    // Adiciona o nome do mundo � lista de mundos dispon�veis
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
            UnityEngine.Debug.LogWarning("A pasta 'LevelEditor' n�o existe!");
        }
    }

    public void SelectWorld(string worldName)
    {
        // Define o nome do mundo selecionado
        currentWorldName = worldName;

        // Obt�m o caminho completo para o arquivo de informa��es do mundo
        string worldInfoFilePath = Path.Combine(levelEditorPath, currentWorldName, "World.Info");

        // Verifica se o arquivo de informa��es do mundo existe
        if (File.Exists(worldInfoFilePath))
        {
            // L� o conte�do do arquivo de informa��es do mundo
            string worldInfo = File.ReadAllText(worldInfoFilePath);

            // Verifica se a linha "WithWorldmap" est� presente e seu valor
            if (worldInfo.Contains("WithWorldmap: True"))
            {
                PlayWorld.instance.isWorldmap = true;
                // Define o nome do mundo selecionado na vari�vel selectedWorldName
                PlayWorld.instance.selectedWorldName = currentWorldName;

                // Abra a cena do mundo em uma nova cena
                UnityEngine.SceneManagement.SceneManager.LoadScene("PlayWorld");
            }
            else if (worldInfo.Contains("WithWorldmap: False"))
            {
                buttonBackWorldList.SetActive(true);
                PlayWorld.instance.isWorldmap = false;
                // Define o nome do mundo selecionado na vari�vel selectedWorldName
                PlayWorld.instance.selectedWorldName = currentWorldName;
                InstantiateLevelButtons(currentWorldName);
                // Mostre a lista de n�veis contidos no mundo (voc� pode implementar isso)
                // Por exemplo, ative um painel que exibe a lista de n�veis.
                currentPanel.SetActive(false);
                LevelListPanel.SetActive(true);
            }
        }

    }


    //levels

    public void InstantiateLevelButtons(string worldName)
    {
        // Verifica se o container dos bot�es de n�vel est� definido
        if (levelButtonContainer == null)
        {
            UnityEngine.Debug.LogWarning("O container de bot�es de n�vel n�o est� definido.");
            return;
        }

        // Remove os bot�es de n�vel existentes
        foreach (Transform child in levelButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // Obt�m o caminho completo para a pasta do mundo
        string worldFolderPath = Path.Combine(levelEditorPath, worldName);

        // Verifica se a pasta do mundo existe
        if (Directory.Exists(worldFolderPath))
        {
            // Obt�m todos os arquivos com a extens�o ".TAOWLE" dentro da pasta do mundo
            string[] levelFiles = Directory.GetFiles(worldFolderPath, "*.TAOWLE");

            // Instancia um bot�o para cada n�vel dispon�vel
            foreach (string levelFile in levelFiles)
            {
                // Obt�m o nome do n�vel
                string levelName = Path.GetFileNameWithoutExtension(levelFile);

                // Cria um novo bot�o de n�vel
                Button levelButton = Instantiate(levelButtonPrefab, levelButtonContainer);
                levelButton.GetComponentInChildren<TextMeshProUGUI>().text = levelName;

                // Configura o callback de clique para abrir o n�vel correspondente
                string level = levelName; // Vari�vel tempor�ria para evitar closure
                levelButton.onClick.AddListener(() =>
                {
                    // Define o nome do n�vel selecionado
                    currentLevelName = level;

                    // Chame a fun��o que carrega o n�vel ou execute qualquer outra a��o desejada com o n�vel selecionado
                    PlayWorld.instance.selectedLevelName = currentLevelName;

                    UnityEngine.SceneManagement.SceneManager.LoadScene("PlayLevel");
                    // Fecha o painel atual
                    if (LevelListPanel != null)
                    {
                        LevelListPanel.SetActive(false);
                        worldListPanel.SetActive(false);
                    }

                });

                // Ativa o bot�o
                levelButton.gameObject.SetActive(true);
            }

            // Atualiza o tamanho vertical do Content do ScrollView de acordo com o n�mero de bot�es
            float buttonHeight = levelButtonPrefab.GetComponent<RectTransform>().rect.height;
            float spacing = 130f; // O valor de spacing do seu Horizontal Layout Group
            float totalHeight = (levelFiles.Length * buttonHeight) + (spacing * (levelFiles.Length - 1));
            levelContent.sizeDelta = new Vector2(levelContent.sizeDelta.x, totalHeight);
        }
        else
        {
            UnityEngine.Debug.LogWarning("A pasta do mundo n�o existe: " + worldFolderPath);
        }
    }


}
