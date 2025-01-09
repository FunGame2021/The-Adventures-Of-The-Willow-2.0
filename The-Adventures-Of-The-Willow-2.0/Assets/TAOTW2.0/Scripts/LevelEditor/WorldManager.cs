using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using TMPro;
using System.Diagnostics;
using System.Collections;

public class WorldManager : MonoBehaviour
{
    public static WorldManager instance;

    public Transform worldButtonContainer; // Container para os botões dos mundos
    public Transform levelButtonContainer; // Container para os botões dos níveis
    public RectTransform levelContent;
    public RectTransform worldContent;

    public Button worldButtonPrefab; // Prefab do botão do mundo
    public Button levelButtonPrefab; // Prefab do botão do nível

    [HideInInspector] public string levelEditorPath; // Caminho para a pasta "LevelEditor"

    private List<string> availableWorlds; // Lista de mundos disponíveis
    public string currentWorldName; // Nome do mundo atual
    public string currentLevelName;
    public string currentAuthorName;
    public bool currentWithWorldmap;


    public TMP_InputField worldNameInputField; // Referência para o campo de entrada do nome do mundo
    public TMP_InputField authorNameInputField; // Referência para o campo de entrada do autor do mundo

    public TMP_InputField NewWorldNameInputField; // Referência para o campo de entrada do nome do mundo
    public TMP_InputField NewAuthorNameInputField; // Referência para o campo de entrada do autor do mundo

    public TextMeshProUGUI currentWorldText; // Referência para o texto do nível atual


    public GameObject currentPanel; // Referência para o painel atual que está sendo exibido
    public GameObject worldListPanel; // Referência para o painel com a lista de mundos


    public GameObject currentWorldListPanel; // Referência para o painel atual que está sendo exibido
    public GameObject levelListPanel; // Referência para o painel com a lista de mundos
    public GameObject closeAllPanel;


    public GameObject WarnTestLevelPanel;

    //Toggle info se tem mundo ou não
    public Toggle toggle;


    private IEnumerator Start()
    {
        if (instance == null)
        {
            instance = this;
        }

        // Obtém o caminho completo para a pasta "LevelEditor"
        levelEditorPath = Path.Combine(Application.persistentDataPath, "LevelEditor");

        // Carrega os mundos disponíveis
        LoadAvailableWorlds();


        // Defina o valor inicial do toggle para true
        toggle.isOn = true;
        currentWithWorldmap = true; // Inicialmente, assume-se que o toggle está ativado

        // Assine o evento OnValueChanged do toggle
        toggle.onValueChanged.AddListener(OnToggleValueChanged);

        if (!string.IsNullOrEmpty(LevelEditorController.instance.AtualLevel) && !string.IsNullOrEmpty(LevelEditorController.instance.AtualWorld))
        {
            currentLevelName = LevelEditorController.instance.AtualLevel;
            currentWorldName = LevelEditorController.instance.AtualWorld;
            closeAllPanel.SetActive(false);
            // Aguarde até que o SectorManager esteja pronto
            yield return WaitUntilSectorManagerIsReady();

            // Aguarde até que o SectorManager esteja pronto
            LoadCurrentWorldAndLevel();
        }

    }
    private IEnumerator WaitUntilSectorManagerIsReady()
    {
        while (SectorManager.instance == null)
        {
            yield return null;
        }
    }


    // Método chamado sempre que o valor do toggle for alterado
    private void OnToggleValueChanged(bool value)
    {
        // Atualize a variável currentWithWorldmap com o valor do toggle
        currentWithWorldmap = value;
        CreateWorldMap();
        // Verifica se o mundo atual foi selecionado e salva o arquivo World.Info
        if (!string.IsNullOrEmpty(currentWorldName))
        {
            string wordFolderPath = Path.Combine(levelEditorPath, currentWorldName);
            UpdateWorldInfo(wordFolderPath, currentWorldName, currentAuthorName, currentWithWorldmap);
        }
    }

    public void LoadCurrentWorldAndLevel()
    {
        if (!string.IsNullOrEmpty(currentWorldName) && !string.IsNullOrEmpty(currentLevelName) && SectorManager.instance != null)
        {
            // Carrega o mundo atual e o nível atual
            SelectWorld(currentWorldName);
            SectorManager.instance.currentSectorName = "Sector1";
            LevelEditorManager.instance.LoadLevel(currentWorldName, currentLevelName, "Sector1");
        }
        else
        {
            UnityEngine.Debug.Log("SectorManager is null");
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
        float spacing = 130f;  // O valor de spacing do seu Vertical Layout Group
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

                    LevelEditorManager.instance.isWorldMapEditor = false;
                    // Chame a função que carrega o nível ou execute qualquer outra ação desejada com o nível selecionado
                    LevelEditorManager.instance.LoadLevel(worldName, level, "Sector1");
                    SectorManager.instance.currentSectorName = "Sector1";

                    // Fecha o painel atual
                    if (levelListPanel != null)
                    {
                        levelListPanel.SetActive(false);
                        closeAllPanel.SetActive(false);
                    }

                });

                // Ativa o botão
                levelButton.gameObject.SetActive(true);
            }

            // Atualiza o tamanho vertical do Content do ScrollView de acordo com o número de botões
            float buttonHeight = levelButtonPrefab.GetComponent<RectTransform>().rect.height;
            float spacing = 130f;  // O valor de spacing do seu Vertical Layout Group
            float totalHeight = (availableWorlds.Count * buttonHeight) + (spacing * (availableWorlds.Count - 1));
            levelContent.sizeDelta = new Vector2(levelContent.sizeDelta.x, totalHeight + 1000);

        }
        else
        {
            UnityEngine.Debug.LogWarning("A pasta do mundo não existe: " + worldFolderPath);
        }
    }

    public void SelectWorld(string worldName)
    {
        // Define o nome do mundo selecionado
        currentWorldName = worldName;

        // Atualiza o TextMeshProUGUI com o nome do mundo e autor
        string authorName = GetAuthorName(worldName);
        currentWorldText.text = "Mundo: " + worldName + " | Autor: " + authorName;

        // Instancia os botões de nível para o mundo selecionado
        InstantiateLevelButtons(worldName);

        // Fecha o painel atual
        if (currentWorldListPanel != null)
        {
            currentWorldListPanel.SetActive(false);
        }

        // Abre o painel de níveis
        if (levelListPanel != null)
        {
            levelListPanel.SetActive(true);
        }

    }

    //Called on button
    public void TestGame()
    {
        if (LevelEditorController.instance != null)
        {
            LevelEditorController.instance.StartGame();
        }
    }

    #region Debug
    private void DisplayAvailableWorlds()
    {
        // Exibe a lista de mundos disponíveis
        UnityEngine.Debug.Log("Mundos Disponíveis:");
        foreach (string worldName in availableWorlds)
        {
            UnityEngine.Debug.Log(worldName);
        }
    }

    private void DisplayAvailableLevels(string worldName)
    {
        // Obtém o caminho completo para a pasta do mundo
        string worldFolderPath = Path.Combine(levelEditorPath, worldName);

        // Verifica se a pasta do mundo existe
        if (Directory.Exists(worldFolderPath))
        {
            // Obtém todos os arquivos com a extensão ".TAOWLE" dentro da pasta do mundo
            string[] levelFiles = Directory.GetFiles(worldFolderPath, "*.TAOWLE");

            // Exibe a lista de níveis disponíveis para o mundo selecionado
            UnityEngine.Debug.Log("Níveis Disponíveis para o Mundo " + worldName + ":");
            foreach (string levelFile in levelFiles)
            {
                string levelName = Path.GetFileNameWithoutExtension(levelFile);
                UnityEngine.Debug.Log(levelName);
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("A pasta do mundo não existe!");
        }
    }
    #endregion

    

    #region Criar mundo
    public void OKCreateNewWorldInput()
    {
        // Obtém o nome e autor do mundo dos campos de entrada de texto
        string worldName = worldNameInputField.text;
        string authorName = authorNameInputField.text;
        bool withWorldmap = currentWithWorldmap;

        // Verifica se o nome e autor não estão vazios
        if (string.IsNullOrEmpty(worldName) || string.IsNullOrEmpty(authorName))
        {
            UnityEngine.Debug.LogWarning("Por favor, preencha todos os campos!");
            return;
        }

        // Aqui você pode adicionar a lógica para o número de níveis, se necessário

        // Chama o método CreateNewWorld com os valores fornecidos
        CreateNewWorld(worldName, authorName, withWorldmap);


        // Limpa os campos de entrada de texto
        worldNameInputField.text = "";
        authorNameInputField.text = "";
    }

    public void CreateNewWorld(string worldName, string authorName, bool withWorldmap)
    {
        // Obtém o caminho para a pasta "LevelEditor" dentro de "Application.persistentDataPath"
        string levelEditorPath = Path.Combine(Application.persistentDataPath, "LevelEditor");

        // Verifica se a pasta "LevelEditor" existe
        if (!Directory.Exists(levelEditorPath))
        {
            // Cria a pasta "LevelEditor" se não existir
            Directory.CreateDirectory(levelEditorPath);
        }

        // Cria o caminho completo para a pasta do mundo dentro de "LevelEditor"
        string worldFolderPath = Path.Combine(levelEditorPath, worldName);

        // Verifica se a pasta do mundo já existe
        if (!Directory.Exists(worldFolderPath))
        {
            // Cria a pasta do mundo
            Directory.CreateDirectory(worldFolderPath);

            // Cria o caminho para o arquivo de informações do mundo
            string worldInfoFilePath = Path.Combine(worldFolderPath, "World.Info");

            // Cria o conteúdo do arquivo de informações do mundo com as informações do mundo
            string worldInfo = "Nome: " + worldName + "\nAutor: " + authorName + "\nWithWorldmap: " + withWorldmap.ToString();
            File.WriteAllText(worldInfoFilePath, worldInfo);

            UnityEngine.Debug.Log("Novo mundo criado com sucesso!");

            // Atualiza a lista de mundos disponíveis e instancie os botões dos mundos novamente
            LoadAvailableWorlds();
            ShowWorldList();
        }
        else
        {
            UnityEngine.Debug.LogWarning("A pasta do mundo já existe!");
        }
    }

    public void CreateWorldMap()
    {
        if (currentWithWorldmap)
        {
            // Obtém o caminho para a pasta do mundo
            string worldFolderPath = Path.Combine(levelEditorPath, currentWorldName);

            // Verifica se o arquivo "World.TAOWWE" já existe na pasta do mundo
            string worldMapFilePath = Path.Combine(worldFolderPath, "World.TAOWWE");
            if (!File.Exists(worldMapFilePath))
            {
                // Cria o arquivo "World.TAOWWE" na pasta do mundo
                File.WriteAllText(worldMapFilePath, "");

                UnityEngine.Debug.Log("Arquivo do mapa do mundo criado na pasta do mundo: " + worldMapFilePath);
            }
        }
    }

    public void EditWorld()
    {
        // Verifica se a pasta do mundo atual existe
        string worldFolderPath = Path.Combine(levelEditorPath, currentWorldName);
        if (Directory.Exists(worldFolderPath))
        {
            // Obtém o caminho completo para o arquivo do mundo atual
            string worldFilePath = Path.Combine(worldFolderPath, "World.TAOWWE");

            // Verifica se o arquivo do mundo atual existe
            if (File.Exists(worldFilePath))
            {
                // Define a variável currentWorldEdit com o caminho completo para o arquivo do mundo atual
                LevelEditorManager.instance.LoadWorld(currentWorldName);
                LevelEditorManager.instance.isWorldMapEditor = true;
                // Exibe no console o caminho completo para o arquivo do mundo atual
                UnityEngine.Debug.Log("Editing the world: " + currentWorldName);
            }
            else
            {
                CreateWorldMap();
            }
        }
        else
        {
            UnityEngine.Debug.LogError("A pasta do mundo atual não existe: " + worldFolderPath);
        }
    }


    #endregion

    #region Carregar Mundos Lista
    public void LoadWorlds()
    {
        // Verifica se a pasta "LevelEditor" existe
        if (Directory.Exists(levelEditorPath))
        {
            // Obtém todas as pastas dentro de "LevelEditor"
            string[] worldFolders = Directory.GetDirectories(levelEditorPath);

            // Itera sobre todas as pastas do mundo
            foreach (string worldFolder in worldFolders)
            {
                // Verifica se a pasta do mundo contém o arquivo de informações do mundo
                string worldInfoFilePath = Path.Combine(worldFolder, "World.Info");
                if (File.Exists(worldInfoFilePath))
                {

                    // Atualiza a lista de mundos disponíveis e instancie os botões dos mundos novamente
                    LoadAvailableWorlds();
                    ShowWorldList();

                    // Lê as informações do mundo do arquivo de informações do mundo
                    string worldInfo = File.ReadAllText(worldInfoFilePath);

                    // Extrai as informações do mundo do conteúdo do arquivo
                    string[] lines = worldInfo.Split('\n');
                    string worldName = lines[0].Substring(6);
                    string authorName = lines[1].Substring(7);

                    // Obtém o caminho completo para a pasta do mundo
                    string worldFolderPath = Path.Combine(levelEditorPath, worldName);

                    // Verifica se a pasta do mundo existe
                    if (Directory.Exists(worldFolderPath))
                    {
                        // Obtém todos os arquivos com a extensão ".TAOWLE" dentro da pasta do mundo
                        string[] levelFiles = Directory.GetFiles(worldFolderPath, "*.TAOWLE");

                        // Obtém o número de níveis com base na quantidade de arquivos .TAOWLE
                        int numLevels = levelFiles.Length;

                        UnityEngine.Debug.Log("Mundo encontrado:");
                        UnityEngine.Debug.Log("Nome: " + worldName);
                        UnityEngine.Debug.Log("Autor: " + authorName);
                        UnityEngine.Debug.Log("Número de Níveis: " + numLevels);
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning("A pasta do mundo não existe!");
                    }
                }
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("A pasta 'LevelEditor' não existe!");
        }
    }

    #endregion

    #region Manage Worlds delete duplicate

    public void DeleteThisWorld()
    {
        DeleteWorld(currentWorldName);
    }


    public void DeleteWorld(string worldName)
    {
        // Obtém o caminho completo para a pasta do mundo
        string worldFolderPath = Path.Combine(levelEditorPath, worldName);

        // Verifica se a pasta do mundo existe
        if (Directory.Exists(worldFolderPath))
        {
            // Exclui a pasta do mundo e todos os seus arquivos e subpastas
            Directory.Delete(worldFolderPath, true);

            UnityEngine.Debug.Log("Mundo excluído: " + worldFolderPath);
        }
        else
        {
            UnityEngine.Debug.LogWarning("O mundo não existe: " + worldFolderPath);
        }
    }
    public void DuplicateWorld(string worldName, string newWorldName, string authorName, bool withWorldmap)
    {
        // Obtém o caminho completo para a pasta do mundo original
        string originalWorldFolderPath = Path.Combine(levelEditorPath, worldName);

        // Obtém o caminho completo para a pasta do novo mundo duplicado
        string newWorldFolderPath = Path.Combine(levelEditorPath, newWorldName);

        // Verifica se a pasta do mundo original existe
        if (Directory.Exists(originalWorldFolderPath))
        {
            // Verifica se a pasta do novo mundo já existe
            if (!Directory.Exists(newWorldFolderPath))
            {
                // Cria uma cópia da pasta do mundo original para o novo caminho
                //CopyDirectory(originalWorldFolderPath, newWorldFolderPath);

                UnityEngine.Debug.Log("Mundo duplicado: " + newWorldFolderPath);

                // Atualiza as informações do mundo duplicado
                UpdateWorldInfo(newWorldFolderPath, newWorldName, authorName, withWorldmap);
            }
            else
            {
                UnityEngine.Debug.LogWarning("O novo mundo já existe: " + newWorldFolderPath);
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("O mundo original não existe: " + originalWorldFolderPath);
        }
    }

    public void OpenFolder()
    {
        OpenWorldFolder(currentWorldName);

    }
    public void OpenWorldFolder(string worldName)
    {
        // Obtém o caminho completo para a pasta do mundo
        string worldFolderPath = Path.Combine(levelEditorPath, worldName);

        // Verifica se a pasta do mundo existe
        if (Directory.Exists(worldFolderPath))
        {
            // Abre a pasta do mundo no sistema operacional
            Process.Start(worldFolderPath);
        }
        else
        {
            UnityEngine.Debug.LogWarning("A pasta do mundo não existe: " + worldFolderPath);
        }
    }
    #endregion

    #region Update ao Mundo info Name and Autor
    private void UpdateWorldInfo(string worldFolderPath, string worldName, string authorName, bool withWorldmap)
    {
        // Obtém o caminho completo para o arquivo de informações do mundo
        string worldInfoFilePath = Path.Combine(worldFolderPath, "World.Info");

        // Verifica se o arquivo de informações do mundo existe
        if (File.Exists(worldInfoFilePath))
        {
            // Lê as informações do mundo do arquivo de informações
            string worldInfo = File.ReadAllText(worldInfoFilePath);

            // Atualiza o nome do mundo no arquivo de informações
            worldInfo = worldInfo.Replace("Nome: " + worldName, "Nome: " + worldName);

            // Atualiza o autor do mundo no arquivo de informações
            worldInfo = worldInfo.Replace("Autor: " + authorName, "Autor: " + authorName);

            // Verifica se a linha com o valor booleano "WithWorldmap" existe no arquivo
            if (worldInfo.Contains("WithWorldmap:"))
            {
                // Atualiza o valor do toggle "WithWorldmap" no arquivo de informações
                worldInfo = worldInfo.Replace("WithWorldmap: " + !withWorldmap, "WithWorldmap: " + withWorldmap);
            }
            else
            {
                // Se a linha não existir, adiciona-a ao arquivo com o valor atual de withWorldmap
                worldInfo += "\nWithWorldmap: " + withWorldmap.ToString();
            }

            // Escreve as informações atualizadas de volta no arquivo
            File.WriteAllText(worldInfoFilePath, worldInfo);
        }
        else
        {
            UnityEngine.Debug.LogWarning("O arquivo de informações do mundo não existe: " + worldInfoFilePath);
        }
    }

    public void ChangeWorldNameInput()
    {
        // Obtém o nome atual do mundo
        string currentWorldName = this.currentWorldName;

        // Obtém o novo nome do mundo dos campos de entrada de texto
        string newWorldName = worldNameInputField.text;

        // Verifica se o novo nome não está vazio
        if (string.IsNullOrEmpty(newWorldName))
        {
            UnityEngine.Debug.LogWarning("Por favor, preencha o novo nome do mundo!");
            return;
        }

        // Chama o método ChangeWorldName com os valores fornecidos
        ChangeWorldName(currentWorldName, newWorldName);

        // Limpa o campo de entrada de texto
        worldNameInputField.text = "";
    }

    public void ChangeAuthorNameInput()
    {
        // Obtém o nome atual do mundo
        string currentWorldName = this.currentWorldName;

        // Obtém o novo autor do mundo dos campos de entrada de texto
        string newAuthorName = authorNameInputField.text;

        // Verifica se o novo autor não está vazio
        if (string.IsNullOrEmpty(newAuthorName))
        {
            UnityEngine.Debug.LogWarning("Por favor, preencha o novo autor do mundo!");
            return;
        }

        // Chama o método ChangeAuthorName com os valores fornecidos
        ChangeAuthorName(currentWorldName, newAuthorName);

        // Limpa o campo de entrada de texto
        authorNameInputField.text = "";
    }

    public void ChangeNewWorldNameInput()
    {
        // Obtém o nome atual do mundo
        string currentWorldName = this.currentWorldName;

        // Obtém o novo nome do mundo dos campos de entrada de texto
        string newWorldName = NewWorldNameInputField.text;

        // Verifica se o novo nome não está vazio
        if (string.IsNullOrEmpty(newWorldName))
        {
            UnityEngine.Debug.LogWarning("Por favor, preencha o novo nome do mundo!");
            return;
        }

        // Chama o método ChangeWorldName com os valores fornecidos
        ChangeWorldName(currentWorldName, newWorldName);

        // Limpa o campo de entrada de texto
        worldNameInputField.text = "";
    }

    public void ChangeNewAuthorNameInput()
    {
        // Obtém o nome atual do mundo
        string currentWorldName = this.currentWorldName;

        // Obtém o novo autor do mundo dos campos de entrada de texto
        string newAuthorName = NewAuthorNameInputField.text;

        // Verifica se o novo autor não está vazio
        if (string.IsNullOrEmpty(newAuthorName))
        {
            UnityEngine.Debug.LogWarning("Por favor, preencha o novo autor do mundo!");
            return;
        }

        // Chama o método ChangeAuthorName com os valores fornecidos
        ChangeAuthorName(currentWorldName, newAuthorName);

        // Limpa o campo de entrada de texto
        authorNameInputField.text = "";
    }

    public void ChangeWorldName(string worldName, string newWorldName)
    {
        // Obtém o caminho completo para a pasta do mundo atual
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

                // Atualiza o nome do mundo no conteúdo do arquivo de informações
                worldInfo = worldInfo.Replace("Nome: " + worldName, "Nome: " + newWorldName);

                // Escreve as informações atualizadas de volta no arquivo
                File.WriteAllText(worldInfoFilePath, worldInfo);

                UnityEngine.Debug.Log("Nome do mundo alterado com sucesso: " + newWorldName);

                // Renomeia a pasta do mundo para o novo nome
                string newWorldFolderPath = Path.Combine(levelEditorPath, newWorldName);
                Directory.Move(worldFolderPath, newWorldFolderPath);

                // Atualiza as informações do mundo com o novo nome
                UpdateWorldInfo(newWorldFolderPath, newWorldName, currentAuthorName, currentWithWorldmap);
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
    }


    public void ChangeAuthorName(string worldName, string newAuthorName)
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

                // Atualiza o autor do mundo no arquivo de informações
                worldInfo = worldInfo.Replace("Autor: " + GetAuthorName(worldName), "Autor: " + newAuthorName);

                // Escreve as informações atualizadas de volta no arquivo
                File.WriteAllText(worldInfoFilePath, worldInfo);

                // Atualiza o valor de currentAuthorName para refletir a mudança
                currentAuthorName = newAuthorName;

                // Chama o método UpdateWorldInfo para atualizar o arquivo com o valor atualizado de currentWithWorldmap
                UpdateWorldInfo(worldFolderPath, currentWorldName, newAuthorName, currentWithWorldmap);

                UnityEngine.Debug.Log("Autor do mundo alterado com sucesso: " + worldInfoFilePath);
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


    #endregion
}
