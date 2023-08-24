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

    public Transform worldButtonContainer; // Container para os bot�es dos mundos
    public RectTransform worldContent;
    public Button worldButtonPrefab; // Prefab do bot�o do mundo

    [HideInInspector] public string levelEditorPath; // Caminho para a pasta "LevelEditor"

    private List<string> availableWorlds; // Lista de mundos dispon�veis
    public string currentWorldName; // Nome do mundo atual

    public TextMeshProUGUI currentWorldText; // Refer�ncia para o texto do n�vel atual


    public GameObject currentPanel; // Refer�ncia para o painel atual que est� sendo exibido
    public GameObject worldListPanel; // Refer�ncia para o painel com a lista de mundos


    public GameObject currentWorldListPanel; // Refer�ncia para o painel atual que est� sendo exibido
    public GameObject closeAllPanel;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }

        // Obt�m o caminho completo para a pasta "LevelEditor"
        levelEditorPath = Path.Combine(Application.persistentDataPath, "LevelEditor");

        // Carrega os mundos dispon�veis
        LoadAvailableWorlds();

        if (!string.IsNullOrEmpty(LevelEditorController.instance.AtualWorld))
        {
            currentWorldName = LevelEditorController.instance.AtualWorld;
            closeAllPanel.SetActive(false);
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
        // Obt�m o caminho completo para a pasta do mundo
        string worldFolderPath = Path.Combine(levelEditorPath, worldName);

        // Verifica se a pasta do mundo existe
        if (Directory.Exists(worldFolderPath))
        {
            // Obt�m o caminho completo para o arquivo de informa��es do mundo
            string worldInfoFilePath = Path.Combine(worldFolderPath, "World.Info");

            // Verifica se o arquivo de informa��es do mundo existe
            if (File.Exists(worldInfoFilePath))
            {
                // L� as informa��es do mundo do arquivo de informa��es
                string worldInfo = File.ReadAllText(worldInfoFilePath);

                // Exibe o conte�do do arquivo para depura��o
                UnityEngine.Debug.Log("Conte�do do arquivo de informa��es do mundo (" + worldName + "):\n" + worldInfo);

                // Extrai o autor do mundo do conte�do do arquivo
                string[] lines = worldInfo.Split('\n');
                string authorName = lines[1].Substring(7);

                return authorName;
            }
            else
            {
                UnityEngine.Debug.LogWarning("O arquivo de informa��es do mundo n�o existe: " + worldInfoFilePath);
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("A pasta do mundo n�o existe: " + worldFolderPath);
        }

        return "";
    }

}
