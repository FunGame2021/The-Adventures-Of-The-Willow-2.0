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

    public Transform worldButtonContainer; // Container para os bot�es dos mundos
    public Transform levelButtonContainer; // Container para os bot�es dos n�veis
    public RectTransform levelContent;
    public RectTransform worldContent;

    public Button worldButtonPrefab; // Prefab do bot�o do mundo
    public Button levelButtonPrefab; // Prefab do bot�o do n�vel

    [HideInInspector] public string levelEditorPath; // Caminho para a pasta "LevelEditor"

    private List<string> availableWorlds; // Lista de mundos dispon�veis
    public string currentWorldName; // Nome do mundo atual
    public string currentLevelName;
    public string currentAuthorName;
    public bool currentWithWorldmap;


    public TMP_InputField worldNameInputField; // Refer�ncia para o campo de entrada do nome do mundo
    public TMP_InputField authorNameInputField; // Refer�ncia para o campo de entrada do autor do mundo

    public TMP_InputField NewWorldNameInputField; // Refer�ncia para o campo de entrada do nome do mundo
    public TMP_InputField NewAuthorNameInputField; // Refer�ncia para o campo de entrada do autor do mundo

    public TextMeshProUGUI currentWorldText; // Refer�ncia para o texto do n�vel atual


    public GameObject currentPanel; // Refer�ncia para o painel atual que est� sendo exibido
    public GameObject worldListPanel; // Refer�ncia para o painel com a lista de mundos


    public GameObject currentWorldListPanel; // Refer�ncia para o painel atual que est� sendo exibido
    public GameObject levelListPanel; // Refer�ncia para o painel com a lista de mundos
    public GameObject closeAllPanel;


    public GameObject WarnTestLevelPanel;

    //Toggle info se tem mundo ou n�o
    public Toggle toggle;


    private IEnumerator Start()
    {
        if (instance == null)
        {
            instance = this;
        }

        // Obt�m o caminho completo para a pasta "LevelEditor"
        levelEditorPath = Path.Combine(Application.persistentDataPath, "LevelEditor");

        // Carrega os mundos dispon�veis
        LoadAvailableWorlds();


        // Defina o valor inicial do toggle para true
        toggle.isOn = true;
        currentWithWorldmap = true; // Inicialmente, assume-se que o toggle est� ativado

        // Assine o evento OnValueChanged do toggle
        toggle.onValueChanged.AddListener(OnToggleValueChanged);

        if (!string.IsNullOrEmpty(LevelEditorController.instance.AtualLevel) && !string.IsNullOrEmpty(LevelEditorController.instance.AtualWorld))
        {
            currentLevelName = LevelEditorController.instance.AtualLevel;
            currentWorldName = LevelEditorController.instance.AtualWorld;
            closeAllPanel.SetActive(false);
            // Aguarde at� que o SectorManager esteja pronto
            yield return WaitUntilSectorManagerIsReady();

            // Aguarde at� que o SectorManager esteja pronto
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


    // M�todo chamado sempre que o valor do toggle for alterado
    private void OnToggleValueChanged(bool value)
    {
        // Atualize a vari�vel currentWithWorldmap com o valor do toggle
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
            // Carrega o mundo atual e o n�vel atual
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

                    LevelEditorManager.instance.isWorldMapEditor = false;
                    // Chame a fun��o que carrega o n�vel ou execute qualquer outra a��o desejada com o n�vel selecionado
                    LevelEditorManager.instance.LoadLevel(worldName, level, "Sector1");
                    SectorManager.instance.currentSectorName = "Sector1";

                    // Fecha o painel atual
                    if (levelListPanel != null)
                    {
                        levelListPanel.SetActive(false);
                        closeAllPanel.SetActive(false);
                    }

                });

                // Ativa o bot�o
                levelButton.gameObject.SetActive(true);
            }

            // Atualiza o tamanho vertical do Content do ScrollView de acordo com o n�mero de bot�es
            float buttonHeight = levelButtonPrefab.GetComponent<RectTransform>().rect.height;
            float spacing = 130f;  // O valor de spacing do seu Vertical Layout Group
            float totalHeight = (availableWorlds.Count * buttonHeight) + (spacing * (availableWorlds.Count - 1));
            levelContent.sizeDelta = new Vector2(levelContent.sizeDelta.x, totalHeight + 1000);

        }
        else
        {
            UnityEngine.Debug.LogWarning("A pasta do mundo n�o existe: " + worldFolderPath);
        }
    }

    public void SelectWorld(string worldName)
    {
        // Define o nome do mundo selecionado
        currentWorldName = worldName;

        // Atualiza o TextMeshProUGUI com o nome do mundo e autor
        string authorName = GetAuthorName(worldName);
        currentWorldText.text = "Mundo: " + worldName + " | Autor: " + authorName;

        // Instancia os bot�es de n�vel para o mundo selecionado
        InstantiateLevelButtons(worldName);

        // Fecha o painel atual
        if (currentWorldListPanel != null)
        {
            currentWorldListPanel.SetActive(false);
        }

        // Abre o painel de n�veis
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
        // Exibe a lista de mundos dispon�veis
        UnityEngine.Debug.Log("Mundos Dispon�veis:");
        foreach (string worldName in availableWorlds)
        {
            UnityEngine.Debug.Log(worldName);
        }
    }

    private void DisplayAvailableLevels(string worldName)
    {
        // Obt�m o caminho completo para a pasta do mundo
        string worldFolderPath = Path.Combine(levelEditorPath, worldName);

        // Verifica se a pasta do mundo existe
        if (Directory.Exists(worldFolderPath))
        {
            // Obt�m todos os arquivos com a extens�o ".TAOWLE" dentro da pasta do mundo
            string[] levelFiles = Directory.GetFiles(worldFolderPath, "*.TAOWLE");

            // Exibe a lista de n�veis dispon�veis para o mundo selecionado
            UnityEngine.Debug.Log("N�veis Dispon�veis para o Mundo " + worldName + ":");
            foreach (string levelFile in levelFiles)
            {
                string levelName = Path.GetFileNameWithoutExtension(levelFile);
                UnityEngine.Debug.Log(levelName);
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("A pasta do mundo n�o existe!");
        }
    }
    #endregion

    

    #region Criar mundo
    public void OKCreateNewWorldInput()
    {
        // Obt�m o nome e autor do mundo dos campos de entrada de texto
        string worldName = worldNameInputField.text;
        string authorName = authorNameInputField.text;
        bool withWorldmap = currentWithWorldmap;

        // Verifica se o nome e autor n�o est�o vazios
        if (string.IsNullOrEmpty(worldName) || string.IsNullOrEmpty(authorName))
        {
            UnityEngine.Debug.LogWarning("Por favor, preencha todos os campos!");
            return;
        }

        // Aqui voc� pode adicionar a l�gica para o n�mero de n�veis, se necess�rio

        // Chama o m�todo CreateNewWorld com os valores fornecidos
        CreateNewWorld(worldName, authorName, withWorldmap);


        // Limpa os campos de entrada de texto
        worldNameInputField.text = "";
        authorNameInputField.text = "";
    }

    public void CreateNewWorld(string worldName, string authorName, bool withWorldmap)
    {
        // Obt�m o caminho para a pasta "LevelEditor" dentro de "Application.persistentDataPath"
        string levelEditorPath = Path.Combine(Application.persistentDataPath, "LevelEditor");

        // Verifica se a pasta "LevelEditor" existe
        if (!Directory.Exists(levelEditorPath))
        {
            // Cria a pasta "LevelEditor" se n�o existir
            Directory.CreateDirectory(levelEditorPath);
        }

        // Cria o caminho completo para a pasta do mundo dentro de "LevelEditor"
        string worldFolderPath = Path.Combine(levelEditorPath, worldName);

        // Verifica se a pasta do mundo j� existe
        if (!Directory.Exists(worldFolderPath))
        {
            // Cria a pasta do mundo
            Directory.CreateDirectory(worldFolderPath);

            // Cria o caminho para o arquivo de informa��es do mundo
            string worldInfoFilePath = Path.Combine(worldFolderPath, "World.Info");

            // Cria o conte�do do arquivo de informa��es do mundo com as informa��es do mundo
            string worldInfo = "Nome: " + worldName + "\nAutor: " + authorName + "\nWithWorldmap: " + withWorldmap.ToString();
            File.WriteAllText(worldInfoFilePath, worldInfo);

            UnityEngine.Debug.Log("Novo mundo criado com sucesso!");

            // Atualiza a lista de mundos dispon�veis e instancie os bot�es dos mundos novamente
            LoadAvailableWorlds();
            ShowWorldList();
        }
        else
        {
            UnityEngine.Debug.LogWarning("A pasta do mundo j� existe!");
        }
    }

    public void CreateWorldMap()
    {
        if (currentWithWorldmap)
        {
            // Obt�m o caminho para a pasta do mundo
            string worldFolderPath = Path.Combine(levelEditorPath, currentWorldName);

            // Verifica se o arquivo "World.TAOWWE" j� existe na pasta do mundo
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
            // Obt�m o caminho completo para o arquivo do mundo atual
            string worldFilePath = Path.Combine(worldFolderPath, "World.TAOWWE");

            // Verifica se o arquivo do mundo atual existe
            if (File.Exists(worldFilePath))
            {
                // Define a vari�vel currentWorldEdit com o caminho completo para o arquivo do mundo atual
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
            UnityEngine.Debug.LogError("A pasta do mundo atual n�o existe: " + worldFolderPath);
        }
    }


    #endregion

    #region Carregar Mundos Lista
    public void LoadWorlds()
    {
        // Verifica se a pasta "LevelEditor" existe
        if (Directory.Exists(levelEditorPath))
        {
            // Obt�m todas as pastas dentro de "LevelEditor"
            string[] worldFolders = Directory.GetDirectories(levelEditorPath);

            // Itera sobre todas as pastas do mundo
            foreach (string worldFolder in worldFolders)
            {
                // Verifica se a pasta do mundo cont�m o arquivo de informa��es do mundo
                string worldInfoFilePath = Path.Combine(worldFolder, "World.Info");
                if (File.Exists(worldInfoFilePath))
                {

                    // Atualiza a lista de mundos dispon�veis e instancie os bot�es dos mundos novamente
                    LoadAvailableWorlds();
                    ShowWorldList();

                    // L� as informa��es do mundo do arquivo de informa��es do mundo
                    string worldInfo = File.ReadAllText(worldInfoFilePath);

                    // Extrai as informa��es do mundo do conte�do do arquivo
                    string[] lines = worldInfo.Split('\n');
                    string worldName = lines[0].Substring(6);
                    string authorName = lines[1].Substring(7);

                    // Obt�m o caminho completo para a pasta do mundo
                    string worldFolderPath = Path.Combine(levelEditorPath, worldName);

                    // Verifica se a pasta do mundo existe
                    if (Directory.Exists(worldFolderPath))
                    {
                        // Obt�m todos os arquivos com a extens�o ".TAOWLE" dentro da pasta do mundo
                        string[] levelFiles = Directory.GetFiles(worldFolderPath, "*.TAOWLE");

                        // Obt�m o n�mero de n�veis com base na quantidade de arquivos .TAOWLE
                        int numLevels = levelFiles.Length;

                        UnityEngine.Debug.Log("Mundo encontrado:");
                        UnityEngine.Debug.Log("Nome: " + worldName);
                        UnityEngine.Debug.Log("Autor: " + authorName);
                        UnityEngine.Debug.Log("N�mero de N�veis: " + numLevels);
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning("A pasta do mundo n�o existe!");
                    }
                }
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("A pasta 'LevelEditor' n�o existe!");
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
        // Obt�m o caminho completo para a pasta do mundo
        string worldFolderPath = Path.Combine(levelEditorPath, worldName);

        // Verifica se a pasta do mundo existe
        if (Directory.Exists(worldFolderPath))
        {
            // Exclui a pasta do mundo e todos os seus arquivos e subpastas
            Directory.Delete(worldFolderPath, true);

            UnityEngine.Debug.Log("Mundo exclu�do: " + worldFolderPath);
        }
        else
        {
            UnityEngine.Debug.LogWarning("O mundo n�o existe: " + worldFolderPath);
        }
    }
    public void DuplicateWorld(string worldName, string newWorldName, string authorName, bool withWorldmap)
    {
        // Obt�m o caminho completo para a pasta do mundo original
        string originalWorldFolderPath = Path.Combine(levelEditorPath, worldName);

        // Obt�m o caminho completo para a pasta do novo mundo duplicado
        string newWorldFolderPath = Path.Combine(levelEditorPath, newWorldName);

        // Verifica se a pasta do mundo original existe
        if (Directory.Exists(originalWorldFolderPath))
        {
            // Verifica se a pasta do novo mundo j� existe
            if (!Directory.Exists(newWorldFolderPath))
            {
                // Cria uma c�pia da pasta do mundo original para o novo caminho
                //CopyDirectory(originalWorldFolderPath, newWorldFolderPath);

                UnityEngine.Debug.Log("Mundo duplicado: " + newWorldFolderPath);

                // Atualiza as informa��es do mundo duplicado
                UpdateWorldInfo(newWorldFolderPath, newWorldName, authorName, withWorldmap);
            }
            else
            {
                UnityEngine.Debug.LogWarning("O novo mundo j� existe: " + newWorldFolderPath);
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("O mundo original n�o existe: " + originalWorldFolderPath);
        }
    }

    public void OpenFolder()
    {
        OpenWorldFolder(currentWorldName);

    }
    public void OpenWorldFolder(string worldName)
    {
        // Obt�m o caminho completo para a pasta do mundo
        string worldFolderPath = Path.Combine(levelEditorPath, worldName);

        // Verifica se a pasta do mundo existe
        if (Directory.Exists(worldFolderPath))
        {
            // Abre a pasta do mundo no sistema operacional
            Process.Start(worldFolderPath);
        }
        else
        {
            UnityEngine.Debug.LogWarning("A pasta do mundo n�o existe: " + worldFolderPath);
        }
    }
    #endregion

    #region Update ao Mundo info Name and Autor
    private void UpdateWorldInfo(string worldFolderPath, string worldName, string authorName, bool withWorldmap)
    {
        // Obt�m o caminho completo para o arquivo de informa��es do mundo
        string worldInfoFilePath = Path.Combine(worldFolderPath, "World.Info");

        // Verifica se o arquivo de informa��es do mundo existe
        if (File.Exists(worldInfoFilePath))
        {
            // L� as informa��es do mundo do arquivo de informa��es
            string worldInfo = File.ReadAllText(worldInfoFilePath);

            // Atualiza o nome do mundo no arquivo de informa��es
            worldInfo = worldInfo.Replace("Nome: " + worldName, "Nome: " + worldName);

            // Atualiza o autor do mundo no arquivo de informa��es
            worldInfo = worldInfo.Replace("Autor: " + authorName, "Autor: " + authorName);

            // Verifica se a linha com o valor booleano "WithWorldmap" existe no arquivo
            if (worldInfo.Contains("WithWorldmap:"))
            {
                // Atualiza o valor do toggle "WithWorldmap" no arquivo de informa��es
                worldInfo = worldInfo.Replace("WithWorldmap: " + !withWorldmap, "WithWorldmap: " + withWorldmap);
            }
            else
            {
                // Se a linha n�o existir, adiciona-a ao arquivo com o valor atual de withWorldmap
                worldInfo += "\nWithWorldmap: " + withWorldmap.ToString();
            }

            // Escreve as informa��es atualizadas de volta no arquivo
            File.WriteAllText(worldInfoFilePath, worldInfo);
        }
        else
        {
            UnityEngine.Debug.LogWarning("O arquivo de informa��es do mundo n�o existe: " + worldInfoFilePath);
        }
    }

    public void ChangeWorldNameInput()
    {
        // Obt�m o nome atual do mundo
        string currentWorldName = this.currentWorldName;

        // Obt�m o novo nome do mundo dos campos de entrada de texto
        string newWorldName = worldNameInputField.text;

        // Verifica se o novo nome n�o est� vazio
        if (string.IsNullOrEmpty(newWorldName))
        {
            UnityEngine.Debug.LogWarning("Por favor, preencha o novo nome do mundo!");
            return;
        }

        // Chama o m�todo ChangeWorldName com os valores fornecidos
        ChangeWorldName(currentWorldName, newWorldName);

        // Limpa o campo de entrada de texto
        worldNameInputField.text = "";
    }

    public void ChangeAuthorNameInput()
    {
        // Obt�m o nome atual do mundo
        string currentWorldName = this.currentWorldName;

        // Obt�m o novo autor do mundo dos campos de entrada de texto
        string newAuthorName = authorNameInputField.text;

        // Verifica se o novo autor n�o est� vazio
        if (string.IsNullOrEmpty(newAuthorName))
        {
            UnityEngine.Debug.LogWarning("Por favor, preencha o novo autor do mundo!");
            return;
        }

        // Chama o m�todo ChangeAuthorName com os valores fornecidos
        ChangeAuthorName(currentWorldName, newAuthorName);

        // Limpa o campo de entrada de texto
        authorNameInputField.text = "";
    }

    public void ChangeNewWorldNameInput()
    {
        // Obt�m o nome atual do mundo
        string currentWorldName = this.currentWorldName;

        // Obt�m o novo nome do mundo dos campos de entrada de texto
        string newWorldName = NewWorldNameInputField.text;

        // Verifica se o novo nome n�o est� vazio
        if (string.IsNullOrEmpty(newWorldName))
        {
            UnityEngine.Debug.LogWarning("Por favor, preencha o novo nome do mundo!");
            return;
        }

        // Chama o m�todo ChangeWorldName com os valores fornecidos
        ChangeWorldName(currentWorldName, newWorldName);

        // Limpa o campo de entrada de texto
        worldNameInputField.text = "";
    }

    public void ChangeNewAuthorNameInput()
    {
        // Obt�m o nome atual do mundo
        string currentWorldName = this.currentWorldName;

        // Obt�m o novo autor do mundo dos campos de entrada de texto
        string newAuthorName = NewAuthorNameInputField.text;

        // Verifica se o novo autor n�o est� vazio
        if (string.IsNullOrEmpty(newAuthorName))
        {
            UnityEngine.Debug.LogWarning("Por favor, preencha o novo autor do mundo!");
            return;
        }

        // Chama o m�todo ChangeAuthorName com os valores fornecidos
        ChangeAuthorName(currentWorldName, newAuthorName);

        // Limpa o campo de entrada de texto
        authorNameInputField.text = "";
    }

    public void ChangeWorldName(string worldName, string newWorldName)
    {
        // Obt�m o caminho completo para a pasta do mundo atual
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

                // Atualiza o nome do mundo no conte�do do arquivo de informa��es
                worldInfo = worldInfo.Replace("Nome: " + worldName, "Nome: " + newWorldName);

                // Escreve as informa��es atualizadas de volta no arquivo
                File.WriteAllText(worldInfoFilePath, worldInfo);

                UnityEngine.Debug.Log("Nome do mundo alterado com sucesso: " + newWorldName);

                // Renomeia a pasta do mundo para o novo nome
                string newWorldFolderPath = Path.Combine(levelEditorPath, newWorldName);
                Directory.Move(worldFolderPath, newWorldFolderPath);

                // Atualiza as informa��es do mundo com o novo nome
                UpdateWorldInfo(newWorldFolderPath, newWorldName, currentAuthorName, currentWithWorldmap);
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
    }


    public void ChangeAuthorName(string worldName, string newAuthorName)
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

                // Atualiza o autor do mundo no arquivo de informa��es
                worldInfo = worldInfo.Replace("Autor: " + GetAuthorName(worldName), "Autor: " + newAuthorName);

                // Escreve as informa��es atualizadas de volta no arquivo
                File.WriteAllText(worldInfoFilePath, worldInfo);

                // Atualiza o valor de currentAuthorName para refletir a mudan�a
                currentAuthorName = newAuthorName;

                // Chama o m�todo UpdateWorldInfo para atualizar o arquivo com o valor atualizado de currentWithWorldmap
                UpdateWorldInfo(worldFolderPath, currentWorldName, newAuthorName, currentWithWorldmap);

                UnityEngine.Debug.Log("Autor do mundo alterado com sucesso: " + worldInfoFilePath);
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


    #endregion
}
