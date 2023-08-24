using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
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
    public string currentWorldName; // Nome do mundo atual

    public TextMeshProUGUI currentWorldText; // Referência para o texto do nível atual


    public GameObject currentPanel; // Referência para o painel atual que está sendo exibido
    public GameObject worldListPanel; // Referência para o painel com a lista de mundos


    public GameObject currentWorldListPanel; // Referência para o painel atual que está sendo exibido
    public GameObject closeAllPanel;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }

        // Obtém o caminho completo para a pasta "LevelEditor"
        levelEditorPath = Path.Combine(Application.persistentDataPath, "LevelEditor");

        // Carrega os mundos disponíveis
        LoadAvailableWorlds();

        if (!string.IsNullOrEmpty(LevelEditorController.instance.AtualWorld))
        {
            currentWorldName = LevelEditorController.instance.AtualWorld;
            closeAllPanel.SetActive(false);
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
        float totalHeight = availableWorlds.Count * buttonHeight;
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

        // Atualiza o TextMeshProUGUI com o nome do mundo e autor
        string authorName = GetAuthorName(worldName);
        currentWorldText.text = "Mundo: " + worldName + " | Autor: " + authorName;


        // Fecha o painel atual
        if (currentWorldListPanel != null)
        {
            currentWorldListPanel.SetActive(false);
        }


    }

    private string GetAuthorName(string worldName)
    {
        // Obtém o caminho completo para a pasta do mundo
        string worldFolderPath = Path.Combine(levelEditorPath, worldName);

        // Verifica se a pasta do mundo existe
        if (Directory.Exists(worldFolderPath))
        {
            // Obtém o caminho completo para o arquivo de informações do mundo
            string worldInfoFilePath = Path.Combine(worldFolderPath, "World.Info");

            // Verifica se o arquivo de informações do mundo existe
            if (File.Exists(worldInfoFilePath))
            {
                // Lê as informações do mundo do arquivo de informações
                string worldInfo = File.ReadAllText(worldInfoFilePath);

                // Exibe o conteúdo do arquivo para depuração
                UnityEngine.Debug.Log("Conteúdo do arquivo de informações do mundo (" + worldName + "):\n" + worldInfo);

                // Extrai o autor do mundo do conteúdo do arquivo
                string[] lines = worldInfo.Split('\n');
                string authorName = lines[1].Substring(7);

                return authorName;
            }
            else
            {
                UnityEngine.Debug.LogWarning("O arquivo de informações do mundo não existe: " + worldInfoFilePath);
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("A pasta do mundo não existe: " + worldFolderPath);
        }

        return "";
    }

}
