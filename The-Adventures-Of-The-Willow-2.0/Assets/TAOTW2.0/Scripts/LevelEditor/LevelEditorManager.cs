using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Text.RegularExpressions;


public class LevelEditorManager : MonoBehaviour
{
    public static LevelEditorManager instance;

    public Camera mainCamera;

    #region Containers Hide & Show
    public GameObject enemyContainer;
    public GameObject tilemapContainer;
    public GameObject decorContainer;
    public GameObject objectsContainer;

    public Toggle enemyToggle;
    public Toggle decorToggle;
    public Toggle tilemapToggle;
    public Toggle objectsToggle;
    #endregion

    #region Tilemap
    public Button tilemapButtonPrefab; // Prefab do botão de Tilemap
    public Grid tilemapGrid; // Referência ao Grid onde os Tilemaps serão adicionados
    public Transform uiButtonContainer; // Container dos botões no UI
                                        // Variável para contar o número de Tilemaps criados

    private int tilemapCount = 0;

    public Color selectedTilemapColor; // Cor dos Tilemaps selecionados
    public Color deselectedTilemapColor; // Cor dos Tilemaps deselecionados
    public Color selectedButtonColor; // Cor dos Tilemaps selecionados
    public Color deselectedButtonColor; // Cor dos Tilemaps deselecionados

    [HideInInspector] public List<Tilemap> tilemaps = new List<Tilemap>();
    [HideInInspector] public List<Button> tilemapButtons = new List<Button>();


    public TMP_InputField tilemapNameInput;
    public Toggle toggleSolid;
    public Toggle toggleWallPlatform;
    public TMP_InputField zPosInput;
    public TMP_InputField ShortLayerPosInput;



    public GameObject tilemapOptionsPanel; // Referência ao painel de opções
    public GameObject deleteWarningPanel;
    public Button okButton; // Referência ao botão "OK" no painel de opções
    [HideInInspector]public Button selectedButton; // Variável para armazenar o botão selecionado


    [HideInInspector] public Tilemap selectedTilemap;
    private Renderer objectRenderer;

    //Armazenar Info Temporáriamente
    [HideInInspector] public string tempTilemapName;
    [HideInInspector] public bool tempIsSolid;
    [HideInInspector] public bool tempIsWallPlatform;
    [HideInInspector] public float tempZPos;
    [HideInInspector] public int tempShortLayerPos;


    public LayerMask wallLayer;
    public LayerMask groundLayer;
    public LayerMask defaultLayer;

    #endregion

    #region Objects
    public ObjectsData ScriptableObjectData;
    public string selectedObjectName;
    public Transform ObjectsContainer;
    #endregion

    #region GameObjects
    public GameObjectsData ScriptableGameObjectData;
    public string selectedGameObjectName;
    public Transform GameObjectsContainer;
    #endregion

    #region Decor
    public DecorData ScriptableDecorData;
    public string selectedDecorName;
    

    //Decor 2 No z-pos or shortlayer choose
    public Decor2Data ScriptableDecor2Data;
    public string selectedDecor2Name;
    public Transform decor2Container;

    #endregion


    #region Enemy
    public EnemyData ScriptableEnemyData;
    public string selectedEnemyName;
    private bool snapGrid;
    //public event Action<string> OnEnemySelected;

    public Transform EnemyContainer;public EraserTool eraserTool;

    //drag enemy and objects
    public Button snapGridButton; // Arraste o botão aqui no Inspector
    public Button selectPointButton; // Botão de ativação/desativação da ferramenta selectpoint
    public bool isActiveSelectPoint = false; // Indica se a ferramenta select point está ativa 
    



    #endregion
    //Nodes

    [SerializeField] private GameObject NodeObjectPrefab;
    #region Gride
    public int gridWidth = 10; // Largura inicial da grelha
    public int gridHeight = 10; // Altura inicial da grelha

    public int currentGridWidth; // Largura atual da grelha
    public int currentGridHeight; // Altura atual da grelha

    public TMP_InputField inputFieldX; // Referência ao InputField para X
    public TMP_InputField inputFieldY; // Referência ao InputField para Y

    public int gridSizeX; // Variável para armazenar o valor de X
    public int gridSizeY; // Variável para armazenar o valor de Y

    public LineRenderer gridOutline; // Referência ao LineRenderer para desenhar a linha da grelha

    public GameObject warnSizeGrid;
    public UnityEngine.UI.Button confirmButton;

    #endregion

    public TMP_InputField levelNameInput;
    public TMP_InputField authorInput;

    [SerializeField] private List<TileCategoryData> tileCategories; // Lista de categorias de telhas

    #region WorldMap
    public bool isWorldMapEditor;
    [SerializeField] private GameObject levelDotPrefab;
    #endregion

    [SerializeField] private GridVisualizer gridVisualizer;

    [SerializeField] private GameObject PanelLevelEditor;
    [SerializeField] private GameObject PanelWorldEditor;

    //Info Level Edito Object selected to intantiate
    public string selectedStringInfo;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }

        mainCamera = Camera.main;

        currentGridWidth = gridWidth;
        currentGridHeight = gridHeight;

        // Cria um botão e um Tilemap no início
        AddTilemap();

        // Adiciona os listeners dos botões
        AddTilemapButtonListeners();

        // Define o tipo de conteúdo do campo de entrada para permitir apenas números
        zPosInput.contentType = TMP_InputField.ContentType.DecimalNumber;

        // Associa o evento de clique do botão "OK" ao método OnOKButtonClick
        okButton.onClick.AddListener(OnOKButtonClick);

        // Define os valores iniciais dos campos temporários
        tempTilemapName = tilemapNameInput.text;
        tempIsSolid = toggleSolid.isOn;
        tempIsWallPlatform = toggleWallPlatform.isOn;
        //tempIsOneWayPlatform = toggleOneWayPlatform.isOn;
        float.TryParse(zPosInput.text, out tempZPos);
        int.TryParse(ShortLayerPosInput.text, out tempShortLayerPos);

        GenerateGrid();
        DrawGridOutline();

        LevelEditorCamera levelEditorCamera = FindObjectOfType<LevelEditorCamera>();
        if (levelEditorCamera != null)
        {
            levelEditorCamera.UpdateCameraBounds();
        }


        // Configurar os callbacks para os eventos de alteração de texto nos InputFields
        inputFieldX.onValueChanged.AddListener(OnInputFieldXValueChanged);
        inputFieldY.onValueChanged.AddListener(OnInputFieldYValueChanged);


        selectPointButton.onClick.AddListener(ToggleSelectPoint); // Adiciona um listener ao botão para alternar a ferramenta

        // Configurar os callbacks para os eventos de alteração de estado dos botões de alternância
        enemyToggle.onValueChanged.AddListener(OnEnemyToggleValueChanged);
        tilemapToggle.onValueChanged.AddListener(OnTilemapToggleValueChanged);
        decorToggle.onValueChanged.AddListener(OnDecorToggleValueChanged);
        objectsToggle.onValueChanged.AddListener(OnObjectsToggleValueChanged);
    }

    private void Update()
    {
        // Verifica se o botão direito do mouse foi clicado
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            // Converte a posição do mouse para a posição no canvas
            Vector2 inputPosition = Mouse.current.position.ReadValue();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(uiButtonContainer.GetComponent<RectTransform>(), inputPosition, null, out Vector2 localPoint);

            // Verifica se o clique foi em algum botão de Tilemap
            foreach (Button button in tilemapButtons)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(button.GetComponent<RectTransform>(), inputPosition))
                {
                    int index = tilemapButtons.IndexOf(button);
                    OpenTilemapOptions(index);
                    break;
                }
            }
        }

        // Instanciar
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (isActiveSelectPoint)
            {
                return;
            }
            if (PlatformNodeEditor.instance.isNodeEditor)
            {
                return;
            }
            if (eraserTool.isActiveEraserTile || eraserTool.isActiveEraserEnemy)
            {
                return;
            }
            // Verifica se o clique foi realizado em um elemento do UI
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return; // Sai da função se o clique foi no UI
            }

            if (!string.IsNullOrEmpty(selectedEnemyName))
            {
                if (TileButton.instance.selectedTile != null || !string.IsNullOrEmpty(selectedDecorName) ||
                    !string.IsNullOrEmpty(selectedDecor2Name) || !string.IsNullOrEmpty(selectedObjectName)
                    || !string.IsNullOrEmpty(selectedGameObjectName))
                {
                    return;
                }

                // Procura o prefab do inimigo com base no nome selecionado
                GameObject enemyPrefab = FindEnemyPrefabByName(selectedEnemyName);
                selectedStringInfo = selectedEnemyName;

                // Verifica se o prefab do inimigo foi encontrado
                if (enemyPrefab != null)
                {
                    // Converte a posição do mouse para a posição no mundo
                    Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

                    // Define a posição Z do inimigo para 0
                    mousePosition.z = 0f;

                    if (snapGrid)
                    {
                        // Define o tamanho do passo da grade (ajuste conforme necessário)
                        float gridSize = 1.0f; // Tamanho do incremento da grade

                        if (UserInput.instance.playerMoveAndExtraActions.UI.ShiftClick.IsPressed())
                        {
                            // Calcula a posição alinhada à grelha, mantendo o centro do objeto no centro das células da grade
                            mousePosition.x = Mathf.Floor(mousePosition.x / gridSize) * gridSize + gridSize / 1f;
                            mousePosition.y = Mathf.Floor(mousePosition.y / gridSize) * gridSize + gridSize / 1f;

                        }
                        else
                        {
                            // Calcula a posição alinhada à grelha, mantendo o centro do objeto no centro das células da grade
                            mousePosition.x = Mathf.Floor(mousePosition.x / gridSize) * gridSize + gridSize / 2f;
                            mousePosition.y = Mathf.Floor(mousePosition.y / gridSize) * gridSize + gridSize / 2f;
                        }
                    }



                    // Instancia o inimigo no mundo na posição do mouse
                    GameObject instantiatedEnemy = Instantiate(enemyPrefab, mousePosition, Quaternion.identity);

                    // Define o contêiner como pai do objeto instanciado
                    instantiatedEnemy.transform.SetParent(enemyContainer.transform);

                    //UndoAndRedo.instance.AddAction(new AddEnemyAction(instantiatedEnemy, enemyContainer, enemyPrefab, mousePosition));
                }
            }
            else if (!string.IsNullOrEmpty(selectedDecorName))
            {
                if (TileButton.instance.selectedTile != null || !string.IsNullOrEmpty(selectedEnemyName) ||
                    !string.IsNullOrEmpty(selectedDecor2Name) || !string.IsNullOrEmpty(selectedObjectName)
                    || !string.IsNullOrEmpty(selectedGameObjectName))
                {
                    return;
                }

                // Procura o prefab do objeto decorativo com base no nome selecionado
                GameObject decorPrefab = FindDecorPrefabByName(selectedDecorName);

                selectedStringInfo = selectedDecorName;

                // Verifica se o prefab do objeto decorativo foi encontrado
                if (decorPrefab != null)
                {
                    // Converte a posição do mouse para a posição no mundo
                    Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

                    // Define a posição Z do objeto decorativo para 0
                    mousePosition.z = 0f;
                    if (snapGrid)
                    {
                        // Define o tamanho do passo da grade (ajuste conforme necessário)
                        float gridSize = 1.0f; // Tamanho do incremento da grade

                        // Verifica se a tecla Shift está pressionada
                        if (UserInput.instance.playerMoveAndExtraActions.UI.ShiftClick.IsPressed())
                        {
                            // Calcula a posição alinhada à grelha, mantendo o centro do objeto no centro das células da grade
                            mousePosition.x = Mathf.Floor(mousePosition.x / gridSize) * gridSize + gridSize / 1f;
                            mousePosition.y = Mathf.Floor(mousePosition.y / gridSize) * gridSize + gridSize / 1f;
                        }
                        else
                        {
                            // Calcula a posição alinhada à grelha, mantendo o centro do objeto no centro das células da grade
                            mousePosition.x = Mathf.Floor(mousePosition.x / gridSize) * gridSize + gridSize / 2f;
                            mousePosition.y = Mathf.Floor(mousePosition.y / gridSize) * gridSize + gridSize / 2f;
                        }
                    }
                    // Instancia o objeto decorativo no mundo na posição do mouse
                    GameObject instantiatedDecor = Instantiate(decorPrefab, mousePosition, Quaternion.identity);

                    // Define o contêiner como pai do objeto instanciado
                    instantiatedDecor.transform.SetParent(decorContainer.transform);

                    //UndoAndRedo.instance.AddAction(new AddDecorAction(instantiatedDecor, decorContainer, decorPrefab, mousePosition));
                }
            }
            else if (!string.IsNullOrEmpty(selectedDecor2Name))
            {
                if (TileButton.instance.selectedTile != null || !string.IsNullOrEmpty(selectedEnemyName) ||
                    !string.IsNullOrEmpty(selectedDecorName) || !string.IsNullOrEmpty(selectedObjectName)
                    || !string.IsNullOrEmpty(selectedGameObjectName))
                {
                    return;
                }

                // Procura o prefab do objeto decorativo com base no nome selecionado
                GameObject decor2Prefab = FindDecor2PrefabByName(selectedDecor2Name);

                selectedStringInfo = selectedDecor2Name;

                // Verifica se o prefab do objeto decorativo foi encontrado
                if (decor2Prefab != null)
                {
                    // Converte a posição do mouse para a posição no mundo
                    Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

                    // Define a posição Z do objeto decorativo para 0
                    mousePosition.z = 0f;
                    if (snapGrid)
                    {
                        // Define o tamanho do passo da grade (ajuste conforme necessário)
                        float gridSize = 1.0f; // Tamanho do incremento da grade

                        // Verifica se a tecla Shift está pressionada
                        if (UserInput.instance.playerMoveAndExtraActions.UI.ShiftClick.IsPressed())
                        {
                            // Calcula a posição alinhada à grelha, mantendo o centro do objeto no centro das células da grade
                            mousePosition.x = Mathf.Floor(mousePosition.x / gridSize) * gridSize + gridSize / 1f;
                            mousePosition.y = Mathf.Floor(mousePosition.y / gridSize) * gridSize + gridSize / 1f;
                        }
                        else
                        {
                            // Calcula a posição alinhada à grelha, mantendo o centro do objeto no centro das células da grade
                            mousePosition.x = Mathf.Floor(mousePosition.x / gridSize) * gridSize + gridSize / 2f;
                            mousePosition.y = Mathf.Floor(mousePosition.y / gridSize) * gridSize + gridSize / 2f;
                        }
                    }
                    // Instancia o objeto decorativo no mundo na posição do mouse
                    GameObject instantiatedDecor2 = Instantiate(decor2Prefab, mousePosition, Quaternion.identity);

                    // Define o contêiner como pai do objeto instanciado
                    instantiatedDecor2.transform.SetParent(decor2Container.transform);

                    //UndoAndRedo.instance.AddAction(new AddDecorAction(instantiatedDecor2, decor2Container, decor2Prefab, mousePosition));
                }
            }
            else if (!string.IsNullOrEmpty(selectedObjectName))
            {
                if (TileButton.instance.selectedTile != null || !string.IsNullOrEmpty(selectedEnemyName) ||
                    !string.IsNullOrEmpty(selectedDecor2Name) || !string.IsNullOrEmpty(selectedDecorName)
                    || !string.IsNullOrEmpty(selectedGameObjectName))
                {
                    return;
                }

                // Procura o prefab do objeto com base no nome selecionado
                GameObject objectPrefab = FindObjectPrefabByName(selectedObjectName);

                selectedStringInfo = selectedObjectName;

                // Verifica se o prefab do objeto foi encontrado
                if (objectPrefab != null)
                {
                    // Converte a posição do mouse para a posição no mundo
                    Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

                    // Define a posição Z do objeto para 0
                    mousePosition.z = 0f;
                    if (snapGrid)
                    {
                        // Define o tamanho do passo da grade (ajuste conforme necessário)
                        float gridSize = 1.0f; // Tamanho do incremento da grade

                        // Verifica se a tecla Shift está pressionada
                        if (UserInput.instance.playerMoveAndExtraActions.UI.ShiftClick.IsPressed())
                        {
                            // Calcula a posição alinhada à grelha, mantendo o centro do objeto no centro das células da grade
                            mousePosition.x = Mathf.Floor(mousePosition.x / gridSize) * gridSize + gridSize / 1f;
                            mousePosition.y = Mathf.Floor(mousePosition.y / gridSize) * gridSize + gridSize / 1f;
                        }
                        else
                        {
                            // Calcula a posição alinhada à grelha, mantendo o centro do objeto no centro das células da grade
                            mousePosition.x = Mathf.Floor(mousePosition.x / gridSize) * gridSize + gridSize / 2f;
                            mousePosition.y = Mathf.Floor(mousePosition.y / gridSize) * gridSize + gridSize / 2f;
                        }
                    }
                    // Instancia o objeto no mundo na posição do mouse
                    GameObject instantiatedObject = Instantiate(objectPrefab, mousePosition, Quaternion.identity);

                    // Define o contêiner como pai do objeto instanciado
                    instantiatedObject.transform.SetParent(objectsContainer.transform);

                    // Adiciona a ação de adicionar objeto ao sistema de Undo/Redo
                    //UndoAndRedo.instance.AddAction(new AddObjectAction(instantiatedObject, objectsContainer, objectPrefab, mousePosition));
                }
            }
            else if (!string.IsNullOrEmpty(selectedGameObjectName))
            {
                if (TileButton.instance.selectedTile != null || !string.IsNullOrEmpty(selectedEnemyName) ||
                    !string.IsNullOrEmpty(selectedDecor2Name) || !string.IsNullOrEmpty(selectedDecorName)
                    || !string.IsNullOrEmpty(selectedObjectName))
                {
                    return;
                }


                // Verifica se o gameObject selecionado é "PlayerPos", se for e ele estiver na cena
                // move ele para noma posição senão instancia ele.
                if (selectedGameObjectName == "PlayerPos")
                {
                    // Verifica se já existe um GameObject com o nome "PlayerPos" na cena
                    GameObject existingPlayerPos = GameObject.Find("PlayerPos(Clone)");
                    if (existingPlayerPos != null)
                    {
                        // Converte a posição do mouse para a posição no mundo
                        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                        mousePosition.z = 0f;
                        if (snapGrid)
                        {
                            // Define o tamanho do passo da grade (ajuste conforme necessário)
                            float gridSize = 1.0f; // Tamanho do incremento da grade

                            // Verifica se a tecla Shift está pressionada
                            if (UserInput.instance.playerMoveAndExtraActions.UI.ShiftClick.IsPressed())
                            {
                                // Calcula a posição alinhada à grelha
                                mousePosition.x = Mathf.Floor(mousePosition.x / gridSize) * gridSize + gridSize / 1f;
                                mousePosition.y = Mathf.Floor(mousePosition.y / gridSize) * gridSize + gridSize / 1f;
                            }
                            else
                            {
                                // Calcula a posição alinhada à grelha
                                mousePosition.x = Mathf.Floor(mousePosition.x / gridSize) * gridSize + gridSize / 2f;
                                mousePosition.y = Mathf.Floor(mousePosition.y / gridSize) * gridSize + gridSize / 2f;
                            }
                        }

                        // Mova o "PlayerPos" para a nova posição
                        existingPlayerPos.transform.position = mousePosition;
                    }
                    else
                    {
                        // Caso contrário, o Prefab "PlayerPos" não está na cena, então podemos instanciá-lo
                        GameObject gameObjectPrefab = FindGameObjectPrefabByName(selectedGameObjectName);

                        selectedStringInfo = selectedGameObjectName;

                        if (gameObjectPrefab != null)
                        {
                            // Converte a posição do mouse para a posição no mundo
                            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                            mousePosition.z = 0f;
                            if (snapGrid)
                            {
                                // Define o tamanho do passo da grade (ajuste conforme necessário)
                                float gridSize = 1.0f; // Tamanho do incremento da grade

                                // Verifica se a tecla Shift está pressionada
                                if (UserInput.instance.playerMoveAndExtraActions.UI.ShiftClick.IsPressed())
                                {
                                    // Calcula a posição alinhada à grelha, mantendo o centro do objeto no centro das células da grade
                                    mousePosition.x = Mathf.Floor(mousePosition.x / gridSize) * gridSize + gridSize / 1f;
                                    mousePosition.y = Mathf.Floor(mousePosition.y / gridSize) * gridSize + gridSize / 1f;
                                }
                                else
                                {
                                    // Calcula a posição alinhada à grelha, mantendo o centro do objeto no centro das células da grade
                                    mousePosition.x = Mathf.Floor(mousePosition.x / gridSize) * gridSize + gridSize / 2f;
                                    mousePosition.y = Mathf.Floor(mousePosition.y / gridSize) * gridSize + gridSize / 2f;
                                }
                            }
                            // Instancia o objeto no mundo na posição do mouse
                            GameObject instantiatedObject = Instantiate(gameObjectPrefab, mousePosition, Quaternion.identity);

                            // Define o contêiner como pai do objeto instanciado
                            instantiatedObject.transform.SetParent(GameObjectsContainer.transform);

                            // Adiciona a ação de adicionar objeto ao sistema de Undo/Redo
                            //UndoAndRedo.instance.AddAction(new AddObjectAction(instantiatedObject, objectsContainer, objectPrefab, mousePosition));

                        }
                    }
                }
                else //se o selectedGameObjectName não foro o Playerpos instancia os objetor normais não PlayerPos
                {
                    
                    GameObject gameObjectPrefab = FindGameObjectPrefabByName(selectedGameObjectName);

                    selectedStringInfo = selectedGameObjectName;

                    if (gameObjectPrefab != null)
                    {
                        // Converte a posição do mouse para a posição no mundo
                        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                        mousePosition.z = 0f;
                        if (snapGrid)
                        {
                            // Define o tamanho do passo da grade (ajuste conforme necessário)
                            float gridSize = 1.0f; // Tamanho do incremento da grade

                            // Verifica se a tecla Shift está pressionada
                            if (UserInput.instance.playerMoveAndExtraActions.UI.ShiftClick.IsPressed())
                            {
                                // Calcula a posição alinhada à grelha, mantendo o centro do objeto no centro das células da grade
                                mousePosition.x = Mathf.Floor(mousePosition.x / gridSize) * gridSize + gridSize / 1f;
                                mousePosition.y = Mathf.Floor(mousePosition.y / gridSize) * gridSize + gridSize / 1f;
                            }
                            else
                            {
                                // Calcula a posição alinhada à grelha, mantendo o centro do objeto no centro das células da grade
                                mousePosition.x = Mathf.Floor(mousePosition.x / gridSize) * gridSize + gridSize / 2f;
                                mousePosition.y = Mathf.Floor(mousePosition.y / gridSize) * gridSize + gridSize / 2f;
                            }
                        }
                        // Instancia o objeto no mundo na posição do mouse
                        GameObject instantiatedObject = Instantiate(gameObjectPrefab, mousePosition, Quaternion.identity);

                        // Define o contêiner como pai do objeto instanciado
                        instantiatedObject.transform.SetParent(GameObjectsContainer.transform);

                        // Adiciona a ação de adicionar objeto ao sistema de Undo/Redo
                        //UndoAndRedo.instance.AddAction(new AddObjectAction(instantiatedObject, objectsContainer, objectPrefab, mousePosition));
                    }
                }
            }
        }
        // Atualizar valores temporários das opções do tilemap
        UpdateTempValues();

        if (isWorldMapEditor)
        {
            PanelWorldEditor.SetActive(true);
            PanelLevelEditor.SetActive(false);
        }
        else
        {
            PanelLevelEditor.SetActive(true);
            PanelWorldEditor.SetActive(false);
        }
    }
    private void AddTilemapButtonListeners()
    {
        // Adiciona um listener para cada botão de Tilemap existente
        foreach (Button button in tilemapButtons)
        {
            int index = tilemapButtons.IndexOf(button);
            button.onClick.AddListener(() => SelectTilemap(index));
        }
    }

    #region Tilemap
    public void AddTilemap()
    {
        // Cria um novo Tilemap no Grid da cena
        GameObject newTilemapObject = new GameObject("Tilemap_" + tilemapCount.ToString());
        newTilemapObject.transform.SetParent(tilemapGrid.transform, false);


        Tilemap newTilemap = newTilemapObject.AddComponent<Tilemap>();
        TilemapRenderer newTilemapRenderer = newTilemapObject.AddComponent<TilemapRenderer>();

        
        // Define a posição do novo Tilemap
        newTilemapObject.transform.position = new Vector3Int(0, 0, 0);

        // Adiciona o Tilemap à lista
        tilemaps.Add(newTilemap);

        // Cria um novo botão para o Tilemap no UI
        Button newButton = Instantiate(tilemapButtonPrefab, uiButtonContainer);
        newButton.transform.SetParent(uiButtonContainer, false);

        // Configura o callback de clique para selecionar o Tilemap correspondente
        int index = tilemapButtons.Count;
        newButton.onClick.AddListener(() => SelectTilemap(index));

        // Adiciona o botão à lista
        tilemapButtons.Add(newButton);

        // Atualiza os listeners dos botões
        AddTilemapButtonListeners();

        // Incrementa a contagem de Tilemaps para o próximo Tilemap
        tilemapCount++;

        // Seleciona o Tilemap recém-criado, se for o único existente
        if (tilemaps.Count == 1)
        {
            SelectTilemap(0);
        }
    }

    public void SelectTilemap(int index)
    {
        if (index < 0 || index >= tilemaps.Count)
            return;

        // Seleciona o Tilemap atual
        selectedTilemap = tilemaps[index];

        selectedButton = tilemapButtons[index];

        // Altera a cor dos Tilemaps desselecionados
        for (int i = 0; i < tilemaps.Count; i++)
        {
            Color tilemapColor = (i == index) ? selectedTilemapColor : deselectedTilemapColor;
            tilemaps[i].GetComponent<TilemapRenderer>().material.SetColor("_Color", tilemapColor);
        }

        // Altera a cor dos botões de acordo com o Tilemap selecionado
        for (int i = 0; i < tilemapButtons.Count; i++)
        {
            Color buttonColor = (i == index) ? selectedButtonColor : deselectedButtonColor;
            tilemapButtons[i].GetComponent<Image>().color = buttonColor;
        }
    }

    private void OpenTilemapOptions(int index)
    {
        tilemapOptionsPanel.SetActive(true);
        // Verifica se o índice está dentro dos limites válidos
        if (index < 0 || index >= tilemapButtons.Count)
            return;

        // Seleciona o Tilemap correspondente ao índice do botão
        SelectTilemap(index);

        // Obtém o Tilemap selecionado
        Tilemap tilemap = selectedTilemap;

        tilemapNameInput.text = tilemap.name;

        toggleSolid.isOn = (tilemap.GetComponent<TilemapCollider2D>() != null);
        toggleWallPlatform.isOn = (tilemap.GetComponent<TilemapCollider2D>() != null);
        zPosInput.text = tilemap.transform.position.z.ToString();


        // Armazena as informações temporárias
        tempTilemapName = tilemapNameInput.text;
        tempIsSolid = toggleSolid.isOn;
        tempIsWallPlatform = toggleWallPlatform.isOn;
        float.TryParse(zPosInput.text, out tempZPos);
        int.TryParse(ShortLayerPosInput.text, out tempShortLayerPos);
        UpdateTempValues();
    }

    public void DeleteTilemap()
    {
        // Verifica se há um Tilemap selecionado
        if (selectedTilemap != null)
        {
            // Verifica se há apenas um Tilemap disponível
            if (tilemaps.Count == 1)
            {
                // Mostra um painel de aviso informando que não é possível excluir o único Tilemap
                ShowDeleteWarningPanel();
                return;
            }

            // Remove o Tilemap da lista e do Grid
            tilemaps.Remove(selectedTilemap);
            Destroy(selectedTilemap.gameObject);

            // Remove o botão correspondente do UI
            int selectedIndex = tilemapButtons.IndexOf(selectedButton);
            tilemapButtons.Remove(selectedButton);
            Destroy(selectedButton.gameObject);

            // Seleciona outro Tilemap, se houver algum
            if (tilemaps.Count > 0)
            {
                int newIndex = Mathf.Clamp(selectedIndex, 0, tilemaps.Count - 1);
                SelectTilemap(newIndex);
            }
            else
            {
                selectedTilemap = null;
            }
        }
    }

    private void ShowDeleteWarningPanel()
    {
        // Exibir um painel de aviso informando que não é possível excluir o único Tilemap
        deleteWarningPanel.SetActive(true);
        // Aqui você pode adicionar código para exibir uma mensagem de aviso adequada ao usuário
    }

    private void UpdateTempValues()
    {
        tempTilemapName = tilemapNameInput.text;
        tempIsSolid = toggleSolid.isOn;
        //tempIsOneWayPlatform = toggleOneWayPlatform.isOn;
        tempIsWallPlatform = toggleWallPlatform.isOn;
        float.TryParse(zPosInput.text, out tempZPos);
        int.TryParse(ShortLayerPosInput.text, out tempShortLayerPos);
    }

    private void OnOKButtonClick()
    {
        // Verifica se há um Tilemap selecionado
        if (selectedTilemap != null)
        {

            // Aplica as informações temporárias ao Tilemap selecionado
            selectedTilemap.name = tempTilemapName;

            //adicionar colisor ou remover
            if (tempIsSolid)
            {
                // Adiciona um Collider2D ao Tilemap se ainda não existir
                if (selectedTilemap.GetComponent<TilemapCollider2D>() == null)
                {
                    selectedTilemap.gameObject.AddComponent<TilemapCollider2D>();
                }

                // Obtém o componente TilemapCollider2D do Tilemap
                TilemapCollider2D tilemapCollider2D = selectedTilemap.GetComponent<TilemapCollider2D>();

                // Verifica se o TilemapCollider2D existe
                if (tilemapCollider2D != null)
                {
                    // Ativa o "Used by Composite" no TilemapCollider2D
                    tilemapCollider2D.usedByComposite = true;
                }

                // Adiciona um CompositeCollider2D ao Tilemap se ainda não existir
                if (selectedTilemap.GetComponent<CompositeCollider2D>() == null)
                {
                    selectedTilemap.gameObject.AddComponent<CompositeCollider2D>();
                }


                // Obtém o componente CompositeCollider2D do Tilemap
                CompositeCollider2D compositeCollider2D = selectedTilemap.GetComponent<CompositeCollider2D>();

                // Verifica se o CompositeCollider2D existe
                if (compositeCollider2D != null)
                {
                    // Obtém o Rigidbody2D associado ao CompositeCollider2D
                    Rigidbody2D rb = compositeCollider2D.attachedRigidbody;

                    // Verifica se o Rigidbody2D existe
                    if (rb != null)
                    {
                        // Define o tipo de corpo como estático
                        rb.bodyType = RigidbodyType2D.Static;
                    }
                }

            }
            else
            {
                // Remove o Collider2D do Tilemap
                TilemapCollider2D tilemapCollider2D = selectedTilemap.GetComponent<TilemapCollider2D>();
                if (tilemapCollider2D != null)
                {
                    Destroy(tilemapCollider2D);
                }

                // Remove o CompositeCollider2D do Tilemap
                CompositeCollider2D compositeCollider2D = selectedTilemap.GetComponent<CompositeCollider2D>();
                if (compositeCollider2D != null)
                {
                    Destroy(compositeCollider2D);
                }
                // Remove o Rigidbody2D se estiver presente
                Rigidbody2D rigidbody2D = selectedTilemap.GetComponent<Rigidbody2D>();
                if (rigidbody2D != null)
                {
                    Destroy(rigidbody2D);
                }
            }

            //adicionar colisor ou remover
            if(tempIsSolid && !tempIsWallPlatform)
            {
                SetTilemapLayer(selectedTilemap, groundLayer);
            }
            if (tempIsSolid && tempIsWallPlatform)
            {
                SetTilemapLayer(selectedTilemap, wallLayer);
            }
            if (!tempIsSolid && !tempIsWallPlatform)
            {
                SetTilemapLayer(selectedTilemap, defaultLayer);
            }
            ////Adiciona o Effector2d para o One Way Platform
            //if(tempIsOneWayPlatform && tempIsSolid)
            //{
            //    // Obtém o componente TilemapCollider2D do Tilemap
            //    TilemapCollider2D tilemapCollider2D = selectedTilemap.GetComponent<TilemapCollider2D>();

            //    // Verifica se o TilemapCollider2D existe
            //    if (tilemapCollider2D != null)
            //    {
            //        // Ativa o "Used by Effector2D" no TilemapCollider2D
            //        tilemapCollider2D.usedByEffector = true;
            //    }
            //    // Adiciona um Effector2D ao Tilemap se ainda não existir
            //    if (selectedTilemap.GetComponent<PlatformEffector2D>() == null)
            //    {
            //        selectedTilemap.gameObject.AddComponent<PlatformEffector2D>();
            //    }

            //    // Obtém o componente CompositeCollider2D do Tilemap
            //    CompositeCollider2D compositeCollider2D = selectedTilemap.GetComponent<CompositeCollider2D>();

            //    // Verifica se o CompositeCollider2D existe
            //    if (compositeCollider2D != null)
            //    {
            //        compositeCollider2D.usedByEffector = true;
            //    }


            //}
            //else
            //{
            //    // Obtém o componente TilemapCollider2D do Tilemap
            //    TilemapCollider2D tilemapCollider2D = selectedTilemap.GetComponent<TilemapCollider2D>();

            //    // Verifica se o TilemapCollider2D existe
            //    if (tilemapCollider2D != null)
            //    {
            //        // Ativa o "Used by Effector2D" no TilemapCollider2D
            //        tilemapCollider2D.usedByEffector = false;
            //    }
            //    // Remove o Effector2D do Tilemap
            //    PlatformEffector2D platformEffector2D = selectedTilemap.GetComponent<PlatformEffector2D>();
            //    if (platformEffector2D != null)
            //    {
            //        Destroy(platformEffector2D);
            //    }

            //    // Obtém o componente CompositeCollider2D do Tilemap
            //    CompositeCollider2D compositeCollider2D = selectedTilemap.GetComponent<CompositeCollider2D>();

            //    // Verifica se o CompositeCollider2D existe
            //    if (compositeCollider2D != null)
            //    {
            //        compositeCollider2D.usedByEffector = false;
            //    }
            //}


            // Ajusta o valor de Z-pos
            float zPos;
            if (float.TryParse(zPosInput.text, out zPos))
            {
                selectedTilemap.transform.position = new Vector3(selectedTilemap.transform.position.x, selectedTilemap.transform.position.y, zPos);
            }

            // Obtém o componente Renderer do Tilemap selecionado
            objectRenderer = selectedTilemap.GetComponent<TilemapRenderer>();

            // Altera a posição do short layer
            if (objectRenderer != null)
            {
                objectRenderer.sortingOrder = Mathf.RoundToInt(tempShortLayerPos);
            }
        }

        // Fecha o painel de opções
        tilemapOptionsPanel.SetActive(false);
    }

    #endregion

    #region Objects
    public void SelectObject(string objectName)
    {
        // Limpa a seleção anterior do inimigo
        selectedEnemyName = string.Empty;
        // Limpa a seleção anterior do objeto decorativo
        selectedDecorName = string.Empty;
        selectedDecor2Name = string.Empty;
        selectedGameObjectName = string.Empty;

        selectedObjectName = objectName;

        // Procura o objeto no ObjectsData com base no nome selecionado
        GameObject objectPrefab = FindObjectPrefabByName(selectedObjectName);
    }


    private GameObject FindObjectPrefabByName(string objectName)
    {
        // Percorre as categorias e objetos no objectsData
        foreach (ObjectsData.ObjectCategory category in ScriptableObjectData.categories)
        {
            foreach (ObjectsData.ObjectsInfo objectInfo in category.Objects)
            {
                // Verifica se o nome do objeto corresponde ao nome selecionado
                if (objectInfo.ObjectName == objectName)
                {
                    // Retorna o prefab do objeto correspondente
                    return objectInfo.prefab;
                }
            }
        }

        // Retorna nulo se o prefab não for encontrado
        return null;
    }



    #endregion

    #region GameObjects
    public void SelectGameObject(string gameObjectName)
    {
        // Limpa a seleção anterior do inimigo
        selectedEnemyName = string.Empty;
        // Limpa a seleção anterior do objeto decorativo
        selectedDecorName = string.Empty;
        selectedDecor2Name = string.Empty;
        selectedObjectName = string.Empty;

        selectedGameObjectName = gameObjectName;

        // Procura o objeto no ObjectsData com base no nome selecionado
        GameObject gameObjectPrefab = FindGameObjectPrefabByName(selectedGameObjectName);
    }


    private GameObject FindGameObjectPrefabByName(string gameObjectName)
    {
        // Percorre as categorias e objetos no objectsData
        foreach (GameObjectsData.GameObjectCategory category in ScriptableGameObjectData.categories)
        {
            foreach (GameObjectsData.GameObjectsInfo GameObjectInfo in category.GameObjects)
            {
                // Verifica se o nome do objeto corresponde ao nome selecionado
                if (GameObjectInfo.GameObjectName == gameObjectName)
                {
                    // Retorna o prefab do objeto correspondente
                    return GameObjectInfo.prefab;
                }
            }
        }

        // Retorna nulo se o prefab não for encontrado
        return null;
    }



    #endregion

    #region Decor
    public void SelectDecor(string decorName)
    {
        // Limpa a seleção anterior do objeto
        selectedObjectName = string.Empty;
        // Limpa a seleção anterior do Enemy
        selectedEnemyName = string.Empty;
        // Limpa a seleção anterior do Enemy
        selectedDecor2Name = string.Empty;
        selectedGameObjectName = string.Empty;

        selectedDecorName = decorName;

        // Procura o inimigo no EnemyData com base no nome selecionado
        GameObject decorPrefab = FindDecorPrefabByName(selectedDecorName);

    }
    private GameObject FindDecorPrefabByName(string decorName)
    {
        // Percorre as categorias e decorações no DecorData
        foreach (DecorData.DecorCategory category in ScriptableDecorData.categories)
        {
            foreach (DecorData.DecorInfo decorInfo in category.decorations)
            {
                // Verifica se o nome da decoração corresponde ao nome selecionado
                if (decorInfo.decorName == decorName)
                {
                    // Retorna o prefab da decoração correspondente
                    return decorInfo.prefab;
                }
            }
        }

        // Retorna nulo se o prefab não for encontrado
        return null;
    }

    #endregion

    #region Decor2
    public void SelectDecor2(string decor2Name)
    {
        // Limpa a seleção anterior do objeto
        selectedObjectName = string.Empty;
        // Limpa a seleção anterior do Enemy
        selectedEnemyName = string.Empty;
        // Limpa a seleção anterior do Enemy
        selectedDecorName = string.Empty;
        selectedGameObjectName = string.Empty;

        selectedDecor2Name = decor2Name;

        // Procura o inimigo no EnemyData com base no nome selecionado
        GameObject decor2Prefab = FindDecor2PrefabByName(selectedDecor2Name);

    }
    private GameObject FindDecor2PrefabByName(string decor2Name)
    {
        // Percorre as categorias e decorações no DecorData
        foreach (Decor2Data.Decor2Category category in ScriptableDecor2Data.categories)
        {
            foreach (Decor2Data.Decor2Info decor2Info in category.decorations)
            {
                // Verifica se o nome da decoração corresponde ao nome selecionado
                if (decor2Info.decor2Name == decor2Name)
                {
                    // Retorna o prefab da decoração correspondente
                    return decor2Info.prefab;
                }
            }
        }

        // Retorna nulo se o prefab não for encontrado
        return null;
    }

    #endregion

    #region Enemy
    private void ToggleSelectPoint()
    {
        isActiveSelectPoint = !isActiveSelectPoint; // Inverte o estado da ferramenta

        // Ativa ou desativa visualmente o botão (opcional, depende do seu design de UI)
        ColorBlock colors = selectPointButton.colors;
        colors.normalColor = isActiveSelectPoint ? Color.red : Color.white;
        selectPointButton.colors = colors;
    }

    public void ActivateSnapGrid()
    {
        snapGrid = !snapGrid;

        // Defina a cor do botão com base no estado do snapGrid
        ColorBlock colors = snapGridButton.colors;
        colors.normalColor = snapGrid ? Color.red : Color.white; // Altera para vermelho se snapGrid estiver ativado
        snapGridButton.colors = colors;
    }


    public void SelectEnemy(string enemyName)
    {
        // Limpa a seleção anterior do objeto
        selectedObjectName = string.Empty;
        selectedDecorName = string.Empty;
        selectedDecor2Name = string.Empty;
        selectedGameObjectName = string.Empty;

        selectedEnemyName = enemyName;

        // Procura o inimigo no EnemyData com base no nome selecionado
        GameObject enemyPrefab = FindEnemyPrefabByName(selectedEnemyName);

    }

   
    private GameObject FindEnemyPrefabByName(string enemyName)
    {
        // Percorre as categorias e inimigos no EnemyData
        foreach (EnemyData.EnemyCategory category in ScriptableEnemyData.categories)
        {
            foreach (EnemyData.EnemyInfo enemyInfo in category.enemies)
            {
                // Verifica se o nome do inimigo corresponde ao nome selecionado
                if (enemyInfo.enemyName == enemyName)
                {
                    // Retorna o prefab do inimigo correspondente
                    return enemyInfo.prefab;

                }
            }
        }

        // Retorna nulo se o prefab não for encontrado
        return null;
    }

   


    private bool IsMouseOverUI()
    {
        // Verifica se o mouse está sobre elementos de UI
        return EventSystem.current.IsPointerOverGameObject();
    }

    private Vector3 GetMouseWorldPosition()
    {
        // Obtém a posição do mouse na tela
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Converte a posição do mouse para uma posição no mundo
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        // Define a posição do z como zero para manter o inimigo no plano 2D
        mouseWorldPosition.z = 0f;

        return mouseWorldPosition;
    }

    #endregion

    #region Grid
   
    public void GenerateGrid()
    {
        gridVisualizer.OnGridSizeUpdated();

        // Armazena os dados dos tiles existentes
        Dictionary<Vector3Int, TileBase> existingTiles = new Dictionary<Vector3Int, TileBase>();

        // Percorre todas as células do Tilemap e armazena os tiles existentes
        foreach (var position in selectedTilemap.cellBounds.allPositionsWithin)
        {
            if (selectedTilemap.HasTile(position))
            {
                existingTiles.Add(position, selectedTilemap.GetTile(position));
            }
        }

        // Limpa o Tilemap
        selectedTilemap.ClearAllTiles();


        Vector3Int startingCell = new Vector3Int(0, currentGridHeight - 1, 0); // Coordenadas iniciais da célula (topo esquerdo)

        for (int x = 0; x < currentGridWidth; x++)
        {
            for (int y = 0; y < currentGridHeight; y++)
            {
                Vector3Int cellPosition = startingCell + new Vector3Int(x, -y, 0); // Calcula a posição da célula com base na coordenada inicial

                // Restaura os tiles existentes, se houver um na posição atual
                if (existingTiles.TryGetValue(cellPosition, out TileBase existingTile))
                {
                    selectedTilemap.SetTile(cellPosition, existingTile);
                }
                else
                {
                    selectedTilemap.SetTile(cellPosition, null); // Define a telha como nula (sem telha)
                }
            }
        }
    }

    public void ResizeGrid(int newWidth, int newHeight)
    {
        if (currentGridHeight > newHeight || currentGridWidth > newWidth)
        {
            warnSizeGrid.SetActive(true); 
            confirmButton.onClick.AddListener(() => OnConfirmButtonClicked(newWidth, newHeight));

        }
        else
        {
            PerformGridResize(newWidth, newHeight);
        }
    }
    private void OnConfirmButtonClicked(int newWidth, int newHeight)
    {
        PerformGridResize(newWidth, newHeight);
        warnSizeGrid.SetActive(false);
        confirmButton.onClick.RemoveAllListeners();
    }
    private void PerformGridResize(int newWidth, int newHeight)
    {
        currentGridWidth = newWidth;
        currentGridHeight = newHeight;
        GenerateGrid();
        DrawGridOutline();
        gridVisualizer.OnGridSizeUpdated();

        LevelEditorCamera levelEditorCamera = FindObjectOfType<LevelEditorCamera>();
        if (levelEditorCamera != null)
        {
            levelEditorCamera.UpdateCameraBounds();
        }
    }
    public void OnOKButtonClicked()
    {
        // Verificar se os valores de X e Y são válidos
        if (int.TryParse(inputFieldX.text, out int newX) && int.TryParse(inputFieldY.text, out int newY))
        {
            // Chamar o método de redimensionamento da grelha no LevelEditorManager
            ResizeGrid(newX, newY);
        }
        else
        {
            Debug.LogWarning("Invalid grid size. Please enter valid integer values for X and Y.");
        }
    }

    private void OnInputFieldXValueChanged(string newValue)
    {
        // Atualizar o valor de gridSizeX quando o texto do InputField de X é alterado
        int.TryParse(newValue, out gridSizeX);
    }

    private void OnInputFieldYValueChanged(string newValue)
    {
        // Atualizar o valor de gridSizeY quando o texto do InputField de Y é alterado
        int.TryParse(newValue, out gridSizeY);
    }

    public void DrawGridOutline()
    {
        gridVisualizer.OnGridSizeUpdated();
        Vector3 startPoint = selectedTilemap.CellToWorld(new Vector3Int(0, 0, 0)) - new Vector3(0f, 0f, 0f); // Ponto inicial da linha (canto inferior esquerdo)
        Vector3 endPoint = selectedTilemap.CellToWorld(new Vector3Int(currentGridWidth, currentGridHeight, 0)) - new Vector3(0f, 0f, 0f); // Ponto final da linha (canto superior direito)

        float lineWidth = 0.2f; // Largura da linha

        gridOutline.positionCount = 5;
        gridOutline.startWidth = lineWidth;
        gridOutline.endWidth = lineWidth;
        gridOutline.useWorldSpace = true;

        gridOutline.material = new Material(Shader.Find("Sprites/Default")); // Define o material da linha

        Color lineColor = Color.red; // Cor da linha
        gridOutline.startColor = lineColor;
        gridOutline.endColor = lineColor;


        gridOutline.SetPositions(new Vector3[]
        {
        new Vector3(startPoint.x, startPoint.y, 0f),
        new Vector3(startPoint.x, endPoint.y, 0f),
        new Vector3(endPoint.x, endPoint.y, 0f),
        new Vector3(endPoint.x, startPoint.y, 0f),
        new Vector3(startPoint.x, startPoint.y, 0f)
        });
    }





    #endregion

    #region Toggle Layer Editor
    private void OnEnemyToggleValueChanged(bool isOn)
    {
        // Ativa ou desativa o contêiner de inimigos com base no estado do botão de alternância
        enemyContainer.SetActive(isOn);
    }

    private void OnTilemapToggleValueChanged(bool isOn)
    {
        // Ativa ou desativa o contêiner de tilemap com base no estado do botão de alternância
        tilemapContainer.SetActive(isOn);
    }

    private void OnDecorToggleValueChanged(bool isOn)
    {
        // Ativa ou desativa o contêiner de decoração com base no estado do botão de alternância
        decorContainer.SetActive(isOn);
    }
    private void OnObjectsToggleValueChanged(bool isOn)
    {
        // Ativa ou desativa o contêiner de decoração com base no estado do botão de alternância
        objectsContainer.SetActive(isOn);
    }
    #endregion

    #region Save Data

    public void CreateNewLevel()
    {
        // Obtém o nome do nível e o autor dos campos de entrada
        string levelName = levelNameInput.text;
        string author = authorInput.text;

        // Verifica se o nome do nível já existe
        if (IsLevelNameExists(levelName))
        {
            UnityEngine.Debug.LogWarning("O nome do nível já existe!");
            return;

        }
        // Cria um novo objeto LevelData e define o nome do nível e o autor
        LevelData levelData = new LevelData();
        levelData.levelName = levelName;
        levelData.author = author;

        // Cria um novo TilemapData com um Tilemap vazio
        TilemapData emptyTilemapData = new TilemapData();
        emptyTilemapData.tilemapName = "EmptyTilemap";
        emptyTilemapData.tilemapIndex = 0;
        emptyTilemapData.isSolid = false;
        emptyTilemapData.isWallPlatform = false;
        emptyTilemapData.shortLayerPos = 0;
        emptyTilemapData.zPos = 0;
        emptyTilemapData.tiles = new List<TileData>();


        // Cria um objeto TilemapDataWrapper e define os dados
        TilemapDataWrapper tilemapDataWrapper = new TilemapDataWrapper();
        tilemapDataWrapper.gridSizeData = new GridSizeData();
        tilemapDataWrapper.gridSizeData.currentGridWidth = 20; // Define a largura da grade como 20
        tilemapDataWrapper.gridSizeData.currentGridHeight = 20; // Define a altura da grade como 20


        tilemapDataWrapper.levelPreferences = new LevelPreferences();
        tilemapDataWrapper.levelPreferences.MusicID = 1;

        tilemapDataWrapper.levelName = levelName;
        tilemapDataWrapper.author = author;
        tilemapDataWrapper.tilemapDataList = new List<TilemapData>() { emptyTilemapData };
        tilemapDataWrapper.enemySaveData = new List<EnemySaveData>();
        tilemapDataWrapper.decorSaveData = new List<DecorSaveData>();
        tilemapDataWrapper.decor2SaveData = new List<Decor2SaveData>();
        tilemapDataWrapper.objectSaveData = new List<ObjectSaveData>();
        tilemapDataWrapper.gameObjectSaveData = new List<GameObjectSaveData>();

        // Converte o objeto TilemapDataWrapper em JSON
        string json = JsonUtility.ToJson(tilemapDataWrapper);

        // Obtém o caminho completo para a pasta do mundo atual
        string worldFolderPath = Path.Combine(WorldManager.instance.levelEditorPath, WorldManager.instance.currentWorldName);

        // Obtém o caminho completo para o arquivo do novo nível com a extensão ".TAOWLE" dentro da pasta do nível
        string newLevelFilePath = Path.Combine(worldFolderPath, levelName + ".TAOWLE");

        // Salva o JSON em um arquivo
        File.WriteAllText(newLevelFilePath, json);

        // Atualiza a lista de botões de nível
        WorldManager.instance.InstantiateLevelButtons(WorldManager.instance.currentWorldName);
        Debug.Log("Novo nível criado e salvo: " + newLevelFilePath);
    }

    private bool IsLevelNameExists(string levelName)
    {
        // Obtém o caminho completo para a pasta do mundo atual
        string worldFolderPath = Path.Combine(WorldManager.instance.levelEditorPath, WorldManager.instance.currentWorldName);

        // Verifica se a pasta do mundo existe
        if (Directory.Exists(worldFolderPath))
        {
            // Obtém todos os arquivos com a extensão ".TAOWLE" dentro da pasta do mundo
            string[] levelFiles = Directory.GetFiles(worldFolderPath, "*.TAOWLE");

            // Verifica se algum dos arquivos tem o mesmo nome do nível
            foreach (string levelFile in levelFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(levelFile);
                if (fileName == levelName)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void Save()
    {
        if(isWorldMapEditor)
        {
            SaveWorld();
        }
        else
        {
            SaveLevel();
        }
    }
    public void SaveLevel()
    {
        // Obtém o nome do nível e autor dos campos de entrada
        string levelName = levelNameInput.text;
        string author = authorInput.text;

        // Verifica se houve alterações nos campos de entrada
        if (!string.IsNullOrEmpty(levelName) && levelName != WorldManager.instance.currentLevelName)
        {
            // Atualiza o nome do nível
            string oldLevelName = WorldManager.instance.currentLevelName;
            WorldManager.instance.currentLevelName = levelName;

            // Obtém o caminho completo para a pasta do mundo
            string newWorldFolderPath = Path.Combine(WorldManager.instance.levelEditorPath, WorldManager.instance.currentWorldName);

            // Verifica se a pasta do mundo existe
            if (Directory.Exists(newWorldFolderPath))
            {
                // Obtém o caminho completo para o arquivo antigo
                string oldFilePath = Path.Combine(newWorldFolderPath, oldLevelName + ".TAOWLE");

                // Obtém o caminho completo para o novo arquivo
                string newFilePath = Path.Combine(newWorldFolderPath, levelName + ".TAOWLE");

                // Renomeia o arquivo
                File.Move(oldFilePath, newFilePath);

            }
        }

        // Verifica se houve alterações no autor
        if (author != WorldManager.instance.currentAuthorName)
        {
            // Atualiza o autor
            WorldManager.instance.currentAuthorName = author;
        }

        #region save Things
        // Obtém a lista de Tilemaps ativos na cena
        List<Tilemap> activeTilemaps = new List<Tilemap>();
        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap.gameObject.activeSelf)
            {
                activeTilemaps.Add(tilemap);
            }
        }

        GridSizeData gridSizeData = new GridSizeData();
        gridSizeData.currentGridWidth = currentGridWidth;
        gridSizeData.currentGridHeight = currentGridHeight;

        // Cria uma lista de TilemapData para os Tilemaps ativos
        List<TilemapData> tilemapDataList = new List<TilemapData>();
        foreach (Tilemap tilemap in activeTilemaps)
        {
            TilemapData tilemapData = new TilemapData();
            tilemapData.tilemapName = tilemap.name;
            tilemapData.tilemapIndex = tilemap.transform.GetSiblingIndex(); 
            int WallLayer = LayerMask.NameToLayer("Wall");
            tilemapData.isWallPlatform = (tilemap.gameObject.layer == WallLayer);

            Debug.Log("isWallPlatform: " + tilemapData.isWallPlatform);
            //tilemapData.isOneWayPlatform = tilemap.GetComponent<TilemapCollider2D>() != null;
            tilemapData.isSolid = tilemap.GetComponent<TilemapCollider2D>() != null;
            tilemapData.shortLayerPos = tilemap.GetComponent<TilemapRenderer>().sortingOrder;
            tilemapData.zPos = tilemap.transform.position.z;
            tilemapData.tiles = new List<TileData>();

            BoundsInt bounds = tilemap.cellBounds;
            for (int x = bounds.min.x; x < bounds.max.x; x++)
            {
                for (int y = bounds.min.y; y < bounds.max.y; y++)
                {
                    Vector3Int cellPos = new Vector3Int(x, y, 0);
                    TileBase tile = tilemap.GetTile(cellPos);

                    if (tile != null)
                    {
                        TileData tileData = new TileData();
                        tileData.cellPos = cellPos;
                        tileData.tileName = tile.name;
                        tileData.localPos = tilemap.GetCellCenterLocal(cellPos);
                        tilemapData.tiles.Add(tileData);
                    }
                }
            }

            tilemapDataList.Add(tilemapData);
        }

        // Obtém a lista de EnemySaveData dos inimigos na cena
        List<EnemySaveData> enemyList = new List<EnemySaveData>();
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("EnemyObject");
        foreach (GameObject enemyObject in enemyObjects)
        {
            string enemyName = enemyObject.name.Replace("(Clone)", "");
            Vector3 enemyPosition = enemyObject.transform.position;
            EnemySaveData enemyData = new EnemySaveData();
            enemyData.name = enemyName;
            enemyData.position = enemyPosition;
            enemyList.Add(enemyData);
        }

        // Obtém a lista de GameObjectSaveData dos GameObjects na cena
        List<GameObjectSaveData> gameObjectList = new List<GameObjectSaveData>();
        GameObject[] gameObjectObjects = GameObject.FindGameObjectsWithTag("GameObject");
        foreach (GameObject gameObjectObject in gameObjectObjects)
        {
            string gameObjectName = gameObjectObject.name.Replace("(Clone)", "");
            Vector3 gameObjectPosition = gameObjectObject.transform.position;
            GameObjectSaveData gameObjectData = new GameObjectSaveData();
            gameObjectData.name = gameObjectName;
            gameObjectData.position = gameObjectPosition;
            gameObjectList.Add(gameObjectData);
        }

        // Obtém a lista de DecorSaveData dos elementos decorativos na cena
        List<DecorSaveData> decorList = new List<DecorSaveData>();
        GameObject[] decorObjects = GameObject.FindGameObjectsWithTag("DecorObject");
        foreach (GameObject decorObject in decorObjects)
        {
            string decorName = decorObject.name.Replace("(Clone)", "");
            Vector3 decorPosition = decorObject.transform.position;
            DecorSaveData decorData = new DecorSaveData();
            decorData.name = decorName;
            decorData.position = decorPosition;
            decorList.Add(decorData);
        }

        // Obtém a lista de Decor2SaveData dos elementos decorativos na cena
        List<Decor2SaveData> decor2List = new List<Decor2SaveData>();
        GameObject[] decor2Objects = GameObject.FindGameObjectsWithTag("Decor2Object");
        foreach (GameObject decor2Object in decor2Objects)
        {
            string decor2Name = decor2Object.name.Replace("(Clone)", "");
            Vector3 decor2Position = decor2Object.transform.position;

            // Aqui você precisa acessar o componente SpriteRenderer do decor2Object, não do decor2Data
            SpriteRenderer spriteRenderer = decor2Object.GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                //int decor2ShortLayer = spriteRenderer.sortingOrder;

                Decor2SaveData decor2Data = new Decor2SaveData();
                decor2Data.name = decor2Name;
                decor2Data.position = decor2Position;
                decor2Data.shortLayerName = spriteRenderer.sortingLayerName;
                decor2List.Add(decor2Data);
            }
            else
            {
                Debug.LogWarning("SpriteRenderer component not found on Decor2Object: " + decor2Name);
            }
        }


        // Obtém a lista de ObjectSaveData dos objetos na cena
        List<ObjectSaveData> objectList = new List<ObjectSaveData>();
        GameObject[] objectObjects = GameObject.FindGameObjectsWithTag("ObjectObject");
        foreach (GameObject objectObject in objectObjects)
        {
            string objectName = objectObject.name.Replace("(Clone)", "");
            Vector3 objectPosition = objectObject.transform.position;

            // Obtém o tipo do objeto a partir do ScriptableObjectData
            ObjectType objectType = GetObjectType(objectName);

            ObjectSaveData objectData = new ObjectSaveData();
            objectData.name = objectName;
            objectData.position = objectPosition;
            objectData.objectType = objectType;

            // Preenche os dados relacionados ao movimento apenas para objetos do tipo "Moving"
            if (objectType == ObjectType.Moving)
            {
                PlatformMovement movementComponent = objectObject.GetComponent<PlatformMovement>();
                if (movementComponent != null)
                {
                    objectData.isCircular = movementComponent.isCircular;
                    objectData.isPingPong = movementComponent.isPingPong;
                    objectData.id = movementComponent.id;

                    objectData.node = new List<MovementNodeData>();
                    for (int i = 0; i < movementComponent.nodes.Length; i++)
                    {
                        MovementNodeData nodeData = new MovementNodeData();
                        nodeData.position = movementComponent.nodes[i].position;
                        nodeData.nodeTime = movementComponent.nodeTransitionTimes[i];
                        objectData.node.Add(nodeData);
                    }
                }
            }

            objectList.Add(objectData);
        }




        // Cria um objeto que contém os dados do gridSizeData e tilemapDataList
        TilemapDataWrapper tilemapDataWrapper = new TilemapDataWrapper();
        tilemapDataWrapper.levelName = levelName;
        tilemapDataWrapper.author = author;
        // Obtém o MusicID do LevelSettings e atribui ao LevelPreferences
        tilemapDataWrapper.levelPreferences = new LevelPreferences();
        tilemapDataWrapper.levelPreferences.MusicID = LevelSettings.instance.MusicIDToSave;
        tilemapDataWrapper.levelPreferences.levelTime = LevelSettings.instance.levelTime;
        tilemapDataWrapper.gridSizeData = gridSizeData;
        tilemapDataWrapper.tilemapDataList = tilemapDataList;
        tilemapDataWrapper.enemySaveData = enemyList;
        tilemapDataWrapper.gameObjectSaveData = gameObjectList;
        tilemapDataWrapper.decorSaveData = decorList;
        tilemapDataWrapper.decor2SaveData = decor2List;
        tilemapDataWrapper.objectSaveData = objectList;

        // Converte o objeto para JSON
        string json = JsonUtility.ToJson(tilemapDataWrapper, true);
        #endregion
        // Obtém o caminho completo para a pasta do mundo
        string worldFolderPath = Path.Combine(WorldManager.instance.levelEditorPath, WorldManager.instance.currentWorldName);

        // Verifica se a pasta do mundo existe
        if (!Directory.Exists(worldFolderPath))
        {
            // Cria a pasta do mundo se não existir
            Directory.CreateDirectory(worldFolderPath);
        }

        // Obtém o caminho completo para o arquivo JSON com a extensão ".TAOWLE" dentro da pasta do mundo
        string savePath = Path.Combine(worldFolderPath, levelName + ".TAOWLE");

        // Salva o JSON em um arquivo
        File.WriteAllText(savePath, json);

        Debug.Log("Tilemap data saved to " + savePath);
    }

    public void LoadLevel(string worldName, string level)
    {


        // Obtém o caminho completo para a pasta do mundo
        string worldFolderPath = Path.Combine(WorldManager.instance.levelEditorPath, worldName);

        // Obtém o caminho completo para o arquivo JSON com a extensão ".TAOWLE" dentro da pasta do mundo
        string loadPath = Path.Combine(worldFolderPath, level + ".TAOWLE");

        Debug.Log("Tilemap data loaded from " + worldName + "/" + level + ".TAOWLE");

	
        // Verifica se o arquivo JSON existe
        if (File.Exists(loadPath))
        {
            TileButton.instance.UpdateTileButtons();
            // Lê o conteúdo do arquivo JSON
            string json = File.ReadAllText(loadPath);

            // Converte o JSON de volta para o objeto TilemapDataWrapper
            TilemapDataWrapper tilemapDataWrapper = JsonUtility.FromJson<TilemapDataWrapper>(json);

            // Obtém o objeto GridSizeData do TilemapDataWrapper
            GridSizeData gridSizeData = tilemapDataWrapper.gridSizeData;

            // Obtém a lista de EnemySaveData do TilemapDataWrapper
            List<EnemySaveData> enemyList = tilemapDataWrapper.enemySaveData;
            List<GameObjectSaveData> gameObjectList = tilemapDataWrapper.gameObjectSaveData;
            List<ObjectSaveData> objectList = tilemapDataWrapper.objectSaveData;
            List<DecorSaveData> decorList = tilemapDataWrapper.decorSaveData; 
            List<Decor2SaveData> decor2List = tilemapDataWrapper.decor2SaveData;

            // Obtém a lista de TilemapData do TilemapDataWrappere
            List<TilemapData> tilemapDataList = tilemapDataWrapper.tilemapDataList;

            // Limpa os Tilemaps existentes
            ClearTilemaps();

            Debug.Log("Load");
            Debug.Log(tilemapDataWrapper.levelPreferences.levelTime);
            LevelSettings.instance.SetMusicID(tilemapDataWrapper.levelPreferences.MusicID);
            LevelSettings.instance.newLevelTime = tilemapDataWrapper.levelPreferences.levelTime;
            
            LevelSettings.instance.UpdateValues();
            // Restaura o tamanho do grid
            currentGridWidth = gridSizeData.currentGridWidth;
            currentGridHeight = gridSizeData.currentGridHeight;

            // Gera o grid com o novo tamanho
            GenerateGrid();
            DrawGridOutline();

            gridVisualizer.OnGridSizeUpdated();

            // Limpa os inimigos existentes
            ClearEnemies();
            // Limpa os objetos existentes
            ClearObjects();
            ClearGameObjects();
            // Limpa os elementos decorativos existentes
            ClearDecor();
            ClearNodes();
            levelNameInput.text = tilemapDataWrapper.levelName;
            authorInput.text = tilemapDataWrapper.author;

            // Carrega os inimigos salvos
            foreach (EnemySaveData enemyData in enemyList)
            {
                // Encontre o prefab do inimigo com base no nome do inimigo
                GameObject enemyPrefab = null;
                foreach (EnemyData.EnemyCategory category in ScriptableEnemyData.categories)
                {
                    foreach (EnemyData.EnemyInfo enemyInfo in category.enemies)
                    {
                        if (enemyInfo.enemyName == enemyData.name)
                        {
                            enemyPrefab = enemyInfo.prefab;
                            break;
                        }
                    }
                    if (enemyPrefab != null)
                        break;
                }

                if (enemyPrefab != null)
                {
                    // Crie um objeto do inimigo e defina o nome e a posição
                    GameObject enemyObject = Instantiate(enemyPrefab, enemyData.position, Quaternion.identity);
                    enemyObject.transform.SetParent(enemyContainer.transform);
                }
                else
                {
                    Debug.LogWarning("Prefab not found for enemy: " + enemyData.name);
                }
            }


            foreach (GameObjectSaveData gameObjectData in gameObjectList)
            {
                GameObject gameObjectPrefab = null;
                foreach (GameObjectsData.GameObjectCategory category in ScriptableGameObjectData.categories)
                {
                    foreach (GameObjectsData.GameObjectsInfo gameObjectInfo in category.GameObjects)
                    {
                        if (gameObjectInfo.GameObjectName == gameObjectData.name)
                        {
                            gameObjectPrefab = gameObjectInfo.prefab;
                            break;
                        }
                    }
                    if (gameObjectPrefab != null)
                        break;
                }

                if (gameObjectPrefab != null)
                {
                    // Crie um objeto do inimigo e defina o nome e a posição
                    GameObject gameObjectObject = Instantiate(gameObjectPrefab, gameObjectData.position, Quaternion.identity);
                    gameObjectObject.transform.SetParent(GameObjectsContainer.transform);
                }
                else
                {
                    Debug.LogWarning("Prefab not found for enemy: " + gameObjectData.name);
                }
            }

            // Carrega os objetos salvos
            foreach (ObjectSaveData objectData in objectList)
            {
                GameObject objectPrefab = null;
                foreach (ObjectsData.ObjectCategory category in ScriptableObjectData.categories)
                {
                    foreach (ObjectsData.ObjectsInfo objectInfo in category.Objects)
                    {
                        if (objectInfo.ObjectName == objectData.name)
                        {
                            objectPrefab = objectInfo.prefab;
                            break;
                        }
                    }
                    if (objectPrefab != null)
                        break;
                }

                if (objectPrefab != null)
                {
                    // Crie um novo objeto com base no prefab e defina o nome e a posição
                    GameObject objectObject = Instantiate(objectPrefab, objectData.position, Quaternion.identity);
                    objectObject.transform.SetParent(objectsContainer.transform);

                    ObjectType objectType = objectData.objectType;

                    // Restaure os nós de movimento e o tempo de transição para objetos com componente PlatformMovement
                    if (objectType == ObjectType.Moving)
                    {
                        PlatformMovement movementComponent = objectObject.GetComponent<PlatformMovement>();
                        if (movementComponent != null)
                        {
                            movementComponent.isCircular = objectData.isCircular;
                            movementComponent.isPingPong = objectData.isPingPong;
                            movementComponent.id = objectData.id;

                            movementComponent.nodes = new Transform[objectData.node.Count];
                            movementComponent.nodeTransitionTimes = new float[objectData.node.Count];

                            for (int i = 0; i < objectData.node.Count; i++)
                            {
                                // Instancie o prefab dos nodes em vez de criar um novo Transform vazio
                                GameObject nodePrefab = NodeObjectPrefab; // Obtenha o prefab dos nodes da variável NodeObjectPrefab
                                GameObject newNodeObject = Instantiate(nodePrefab, objectData.node[i].position, Quaternion.identity);
                                newNodeObject.name = "Node " + i; // Defina o nome do objeto do node para identificação

                                Transform nodeTransform = newNodeObject.transform;
                                movementComponent.nodes[i] = nodeTransform;
                                movementComponent.nodeTransitionTimes[i] = objectData.node[i].nodeTime;
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Prefab not found for object: " + objectData.name);
                }
            }


            // Carrega os decorativos salvos
            foreach (DecorSaveData decorData in decorList)
            {
                GameObject decorPrefab = null;

                foreach (DecorData.DecorCategory category in ScriptableDecorData.categories)
                {
                    foreach (DecorData.DecorInfo decorInfo in category.decorations)
                    {
                        if (decorInfo.decorName == decorData.name)
                        {
                            decorPrefab = decorInfo.prefab;
                            break;
                        }
                    }
                    if (decorPrefab != null)
                        break;
                }

                if (decorPrefab != null)
                {
                    GameObject decorObject = Instantiate(decorPrefab, decorData.position, Quaternion.identity);
                    decorObject.transform.SetParent(decorContainer.transform);
                }
                else
                {
                    Debug.LogWarning("Prefab not found for decor: " + decorData.name);
                }
            }

            // Carrega os decorativos salvos
            foreach (Decor2SaveData decor2Data in decor2List)
            {
                GameObject decor2Prefab = null;

                foreach (Decor2Data.Decor2Category category in ScriptableDecor2Data.categories)
                {
                    foreach (Decor2Data.Decor2Info decor2Info in category.decorations)
                    {
                        if (decor2Info.decor2Name == decor2Data.name)
                        {
                            decor2Prefab = decor2Info.prefab;
                            break;
                        }
                    }
                    if (decor2Prefab != null)
                        break;
                }

                if (decor2Prefab != null)
                {
                    GameObject decor2Object = Instantiate(decor2Prefab, decor2Data.position, Quaternion.identity);
                    decor2Object.transform.SetParent(decor2Container.transform);

                    // Encontra o primeiro SpriteRenderer em um ancestral
                    SpriteRenderer[] spriteRenderers = decor2Object.GetComponentsInChildren<SpriteRenderer>();
                    if (spriteRenderers != null)
                    {
                        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
                        {
                            // Define o shortLayer a partir dos dados salvos
                            spriteRenderer.sortingLayerName = decor2Data.shortLayerName;
                        }

                    }
                    else
                    {
                        Debug.LogWarning("SpriteRenderer component not found on Decor2Object: " + decor2Data.name);
                    }
                }
                else
                {
                    Debug.LogWarning("Prefab not found for decor: " + decor2Data.name);
                }
            }

            // Percorre os TilemapData da lista
            foreach (TilemapData tilemapData in tilemapDataList)
            {
                // Cria um novo Tilemap no Grid da cena
                GameObject newTilemapObject = new GameObject(tilemapData.tilemapName);
                newTilemapObject.transform.SetParent(tilemapGrid.transform, false);

                Tilemap newTilemap = newTilemapObject.AddComponent<Tilemap>();
                //newTilemapObject.AddComponent<TilemapRenderer>();
                TilemapRenderer newTilemapRenderer = newTilemapObject.AddComponent<TilemapRenderer>();


                // Define a posição do novo Tilemap
                newTilemapObject.transform.position = new Vector3Int(0, 0, Mathf.RoundToInt(tilemapData.zPos));

                newTilemapRenderer.sortingOrder = tilemapData.shortLayerPos;

                // Adiciona o Tilemap à lista
                tilemaps.Add(newTilemap);

                // Cria um novo botão para o Tilemap no UI
                Button newButton = Instantiate(tilemapButtonPrefab, uiButtonContainer);
                newButton.transform.SetParent(uiButtonContainer, false);

                // Configura o callback de clique para selecionar o Tilemap correspondente
                int index = tilemapButtons.Count;
                newButton.onClick.AddListener(() => SelectTilemap(index));

                // Adiciona o botão à lista
                tilemapButtons.Add(newButton);

                // Atualiza os listeners dos botões
                AddTilemapButtonListeners();

                // Seleciona o Tilemap recém-criado, se for o único existente
                if (tilemaps.Count == 1)
                {
                    SelectTilemap(0);
                }

                // Configura a propriedade "isSolid" do Tilemap
                if (tilemapData.isSolid)
                {
                    // Adiciona um Collider2D ao Tilemap se ainda não existir
                    if (newTilemap.GetComponent<TilemapCollider2D>() == null)
                    {
                        newTilemap.gameObject.AddComponent<TilemapCollider2D>();
                    }

                    // Obtém o componente TilemapCollider2D do Tilemap
                    TilemapCollider2D tilemapCollider2D = newTilemap.GetComponent<TilemapCollider2D>();

                    // Verifica se o TilemapCollider2D existe
                    if (tilemapCollider2D != null)
                    {
                        // Ativa o "Used by Composite" no TilemapCollider2D
                        tilemapCollider2D.usedByComposite = true;
                    }

                    // Adiciona um CompositeCollider2D ao Tilemap se ainda não existir
                    if (newTilemap.GetComponent<CompositeCollider2D>() == null)
                    {
                        newTilemap.gameObject.AddComponent<CompositeCollider2D>();
                    }

                    // Obtém o componente CompositeCollider2D do Tilemap
                    CompositeCollider2D compositeCollider2D = newTilemap.GetComponent<CompositeCollider2D>();

                    // Verifica se o CompositeCollider2D existe
                    if (compositeCollider2D != null)
                    {
                        // Obtém o Rigidbody2D associado ao CompositeCollider2D
                        Rigidbody2D rb = compositeCollider2D.attachedRigidbody;

                        // Verifica se o Rigidbody2D existe
                        if (rb != null)
                        {
                            // Define o tipo de corpo como estático
                            rb.bodyType = RigidbodyType2D.Static;
                        }
                    }

                    //adicionar colisor ou remover
                    if (tilemapData.isSolid && !tilemapData.isWallPlatform)
                    {
                        SetTilemapLayer(newTilemap, groundLayer);
                    }
                    if (tilemapData.isSolid && tilemapData.isWallPlatform)
                    {
                        SetTilemapLayer(newTilemap, wallLayer);
                    }
                }
                else
                {
                    // Define a layer do Tilemap para a layer "Default"
                    SetTilemapLayer(newTilemap, defaultLayer);
                }

                //Configura a propriedade "isOneWayPlatform" do Tilemap
                //if(tilemapData.isOneWayPlatform && tilemapData.isSolid)
                //{
                //    // Obtém o componente TilemapCollider2D do Tilemap
                //    TilemapCollider2D tilemapCollider2D = newTilemap.GetComponent<TilemapCollider2D>();

                //    // Verifica se o TilemapCollider2D existe
                //    if (tilemapCollider2D != null)
                //    {
                //        // Ativa o "Used by Effector2D" no TilemapCollider2D
                //        tilemapCollider2D.usedByEffector = true;
                //    }
                //    // Adiciona um Effector2D ao Tilemap se ainda não existir
                //    if (newTilemap.GetComponent<PlatformEffector2D>() == null)
                //    {
                //        newTilemap.gameObject.AddComponent<PlatformEffector2D>();
                //    }

                //    // Obtém o componente CompositeCollider2D do Tilemap
                //    CompositeCollider2D compositeCollider2D = newTilemap.GetComponent<CompositeCollider2D>();

                //    // Verifica se o CompositeCollider2D existe
                //    if (compositeCollider2D != null)
                //    {
                //        compositeCollider2D.usedByEffector = true;
                //    }
                //}

                // Percorre os TileData do TilemapData
                foreach (TileData tileData in tilemapData.tiles)
                {
                    // Carrega a telha do TileData.tileName
                    TileBase tile = GetTileByName(tileData.tileName);

                    // Verifica se a telha existe
                    if (tile != null)
                    {
                        // Define a telha no Tilemap na posição cellPos
                        newTilemap.SetTile(tileData.cellPos, tile);
                    }
                    else
                    {
                        Debug.LogWarning("Tile not found: " + tileData.tileName);
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Save file not found: " + loadPath);
        }
    }

    public void SaveWorld()
    {
      
        #region save Things
        // Obtém a lista de Tilemaps ativos na cena
        List<Tilemap> activeTilemaps = new List<Tilemap>();
        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap.gameObject.activeSelf)
            {
                activeTilemaps.Add(tilemap);
            }
        }

        GridSizeData gridSizeData = new GridSizeData();
        gridSizeData.currentGridWidth = currentGridWidth;
        gridSizeData.currentGridHeight = currentGridHeight;

        // Cria uma lista de TilemapData para os Tilemaps ativos
        List<TilemapData> tilemapDataList = new List<TilemapData>();
        foreach (Tilemap tilemap in activeTilemaps)
        {
            TilemapData tilemapData = new TilemapData();
            tilemapData.tilemapName = tilemap.name;
            tilemapData.tilemapIndex = tilemap.transform.GetSiblingIndex();
            int WallLayer = LayerMask.NameToLayer("Wall");
            tilemapData.isWallPlatform = (tilemap.gameObject.layer == WallLayer);

            Debug.Log("isWallPlatform: " + tilemapData.isWallPlatform);
            //tilemapData.isOneWayPlatform = tilemap.GetComponent<TilemapCollider2D>() != null;
            tilemapData.isSolid = tilemap.GetComponent<TilemapCollider2D>() != null;
            tilemapData.shortLayerPos = tilemap.GetComponent<TilemapRenderer>().sortingOrder;
            tilemapData.zPos = tilemap.transform.position.z;
            tilemapData.tiles = new List<TileData>();

            BoundsInt bounds = tilemap.cellBounds;
            for (int x = bounds.min.x; x < bounds.max.x; x++)
            {
                for (int y = bounds.min.y; y < bounds.max.y; y++)
                {
                    Vector3Int cellPos = new Vector3Int(x, y, 0);
                    TileBase tile = tilemap.GetTile(cellPos);

                    if (tile != null)
                    {
                        TileData tileData = new TileData();
                        tileData.cellPos = cellPos;
                        tileData.tileName = tile.name;
                        tileData.localPos = tilemap.GetCellCenterLocal(cellPos);
                        tilemapData.tiles.Add(tileData);
                    }
                }
            }

            tilemapDataList.Add(tilemapData);
        }

        // Obtém a lista de GameObjectSaveData dos GameObjects na cena
        List<GameObjectSaveData> gameObjectList = new List<GameObjectSaveData>();
        GameObject[] gameObjectObjects = GameObject.FindGameObjectsWithTag("GameObject");
        foreach (GameObject gameObjectObject in gameObjectObjects)
        {
            string gameObjectName = gameObjectObject.name.Replace("(Clone)", "");
            Vector3 gameObjectPosition = gameObjectObject.transform.position;
            GameObjectSaveData gameObjectData = new GameObjectSaveData();
            gameObjectData.name = gameObjectName;
            gameObjectData.position = gameObjectPosition;
            gameObjectList.Add(gameObjectData);
        }


        // Obtém a lista de LevelDots na cena
        GameObject[] levelDotObjects = GameObject.FindGameObjectsWithTag("LevelDot");
        List<LevelDotData> levelDotDataList = new List<LevelDotData>();

        foreach (GameObject levelDotObject in levelDotObjects)
        {
            LevelDot levelDot = levelDotObject.GetComponent<LevelDot>(); // Obtenha o componente LevelDot
            if (levelDot != null)
            {
                LevelDotData dotData = new LevelDotData();
                dotData.levelPath = levelDot.GetLevelPath();
                Vector3 LevelDotPos = levelDot.transform.position;
                dotData.dotPosition = LevelDotPos;
                levelDotDataList.Add(dotData);
            }
        }

        // Cria um objeto que contém os dados do gridSizeData e tilemapDataList
        TilemapDataWrapper tilemapDataWrapper = new TilemapDataWrapper();
        // Obtém o MusicID do LevelSettings e atribui ao LevelPreferences
        tilemapDataWrapper.levelPreferences = new LevelPreferences();
        tilemapDataWrapper.levelPreferences.MusicID = LevelSettings.instance.MusicIDToSave;
        tilemapDataWrapper.levelPreferences.levelTime = LevelSettings.instance.levelTime;
        tilemapDataWrapper.gridSizeData = gridSizeData;
        tilemapDataWrapper.tilemapDataList = tilemapDataList;
        tilemapDataWrapper.gameObjectSaveData = gameObjectList;

        // Adicione a lista de LevelDotData ao objeto TilemapDataWrapper
        tilemapDataWrapper.levelDotDataList = levelDotDataList;

        // Converte o objeto para JSON
        string json = JsonUtility.ToJson(tilemapDataWrapper, true);
        #endregion
        // Obtém o caminho completo para a pasta do mundo
        string worldFolderPath = Path.Combine(WorldManager.instance.levelEditorPath, WorldManager.instance.currentWorldName);

        // Verifica se a pasta do mundo existe
        if (!Directory.Exists(worldFolderPath))
        {
            // Cria a pasta do mundo se não existir
            Directory.CreateDirectory(worldFolderPath);
        }

        // Obtém o caminho completo para o arquivo JSON com a extensão ".TAOWLE" dentro da pasta do mundo
        string savePath = Path.Combine(worldFolderPath, "World.TAOWWE");


        // Salva o JSON em um arquivo
        File.WriteAllText(savePath, json);

        Debug.Log("Tilemap data saved to " + savePath);
    }

    public void LoadWorld(string worldName)
    {
        // Obtém o caminho completo para a pasta do mundo
        string worldFolderPath = Path.Combine(WorldManager.instance.levelEditorPath, worldName);

        // Obtém o caminho completo para o arquivo JSON "World.TAOWWE" dentro da pasta do mundo
        string loadPath = Path.Combine(worldFolderPath, "World.TAOWWE");

        Debug.Log("Tilemap data loaded from " + worldName + "/World.TAOWWE");

        // Verifica se o arquivo JSON existe
        if (File.Exists(loadPath))
        {
            TileButton.instance.UpdateWorldTileButtons();
            // Lê o conteúdo do arquivo JSON
            string json = File.ReadAllText(loadPath);
            //Verifica se o arquivo não está vazio
            if (!string.IsNullOrEmpty(json))
            {
                // Converte o JSON de volta para o objeto TilemapDataWrapper
                TilemapDataWrapper tilemapDataWrapper = JsonUtility.FromJson<TilemapDataWrapper>(json);

                // Obtém o objeto GridSizeData do TilemapDataWrapper
                GridSizeData gridSizeData = tilemapDataWrapper.gridSizeData;

                List<GameObjectSaveData> gameObjectList = tilemapDataWrapper.gameObjectSaveData;
                List<LevelDotData> levelDotDataList = tilemapDataWrapper.levelDotDataList;


                // Obtém a lista de TilemapData do TilemapDataWrappere
                List<TilemapData> tilemapDataList = tilemapDataWrapper.tilemapDataList;


                // Limpa os Tilemaps existentes
                ClearTilemaps();


                LevelSettings.instance.SetMusicID(tilemapDataWrapper.levelPreferences.MusicID);
                LevelSettings.instance.levelTime = tilemapDataWrapper.levelPreferences.levelTime;
                // Restaura o tamanho do grid
                currentGridWidth = gridSizeData.currentGridWidth;
                currentGridHeight = gridSizeData.currentGridHeight;

                // Gera o grid com o novo tamanho
                GenerateGrid();
                DrawGridOutline();

                // Limpa os inimigos existentes
                ClearEnemies();
                // Limpa os objetos existentes
                ClearObjects();
                ClearGameObjects();
                // Limpa os elementos decorativos existentes
                ClearDecor();
                ClearNodes();

                foreach (GameObjectSaveData gameObjectData in gameObjectList)
                {
                    GameObject gameObjectPrefab = null;
                    foreach (GameObjectsData.GameObjectCategory category in ScriptableGameObjectData.categories)
                    {
                        foreach (GameObjectsData.GameObjectsInfo gameObjectInfo in category.GameObjects)
                        {
                            if (gameObjectInfo.GameObjectName == gameObjectData.name)
                            {
                                gameObjectPrefab = gameObjectInfo.prefab;
                                break;
                            }
                        }
                        if (gameObjectPrefab != null)
                            break;
                    }

                    if (gameObjectPrefab != null)
                    {
                        // Crie um objeto do inimigo e defina o nome e a posição
                        GameObject gameObjectObject = Instantiate(gameObjectPrefab, gameObjectData.position, Quaternion.identity);
                        gameObjectObject.transform.SetParent(GameObjectsContainer.transform);
                    }
                    else
                    {
                        Debug.LogWarning("Prefab not found for enemy: " + gameObjectData.name);
                    }
                }

                foreach (LevelDotData dotData in levelDotDataList)
                {
                    // Crie um novo objeto LevelDot na cena
                    GameObject newLevelDotObject = Instantiate(levelDotPrefab, dotData.dotPosition, Quaternion.identity);

                    // Obtenha o componente LevelDot do novo objeto
                    LevelDot newLevelDot = newLevelDotObject.GetComponent<LevelDot>();

                    // Atribua as informações carregadas ao novo objeto LevelDot
                    if (newLevelDot != null)
                    {
                        newLevelDot.SetLevelPath(dotData.levelPath);
                        // Você pode configurar outros dados do LevelDot aqui, se necessário
                    }
                }


                // Percorre os TilemapData da lista
                foreach (TilemapData tilemapData in tilemapDataList)
                {
                    // Cria um novo Tilemap no Grid da cena
                    GameObject newTilemapObject = new GameObject(tilemapData.tilemapName);
                    newTilemapObject.transform.SetParent(tilemapGrid.transform, false);

                    Tilemap newTilemap = newTilemapObject.AddComponent<Tilemap>();
                    //newTilemapObject.AddComponent<TilemapRenderer>();
                    TilemapRenderer newTilemapRenderer = newTilemapObject.AddComponent<TilemapRenderer>();


                    // Define a posição do novo Tilemap
                    newTilemapObject.transform.position = new Vector3Int(0, 0, Mathf.RoundToInt(tilemapData.zPos));

                    newTilemapRenderer.sortingOrder = tilemapData.shortLayerPos;

                    // Adiciona o Tilemap à lista
                    tilemaps.Add(newTilemap);

                    // Cria um novo botão para o Tilemap no UI
                    Button newButton = Instantiate(tilemapButtonPrefab, uiButtonContainer);
                    newButton.transform.SetParent(uiButtonContainer, false);

                    // Configura o callback de clique para selecionar o Tilemap correspondente
                    int index = tilemapButtons.Count;
                    newButton.onClick.AddListener(() => SelectTilemap(index));

                    // Adiciona o botão à lista
                    tilemapButtons.Add(newButton);

                    // Atualiza os listeners dos botões
                    AddTilemapButtonListeners();

                    // Seleciona o Tilemap recém-criado, se for o único existente
                    if (tilemaps.Count == 1)
                    {
                        SelectTilemap(0);
                    }

                    // Configura a propriedade "isSolid" do Tilemap
                    if (tilemapData.isSolid)
                    {
                        // Adiciona um Collider2D ao Tilemap se ainda não existir
                        if (newTilemap.GetComponent<TilemapCollider2D>() == null)
                        {
                            newTilemap.gameObject.AddComponent<TilemapCollider2D>();
                        }

                        // Obtém o componente TilemapCollider2D do Tilemap
                        TilemapCollider2D tilemapCollider2D = newTilemap.GetComponent<TilemapCollider2D>();

                        // Verifica se o TilemapCollider2D existe
                        if (tilemapCollider2D != null)
                        {
                            // Ativa o "Used by Composite" no TilemapCollider2D
                            tilemapCollider2D.usedByComposite = true;
                        }

                        // Adiciona um CompositeCollider2D ao Tilemap se ainda não existir
                        if (newTilemap.GetComponent<CompositeCollider2D>() == null)
                        {
                            newTilemap.gameObject.AddComponent<CompositeCollider2D>();
                        }

                        // Obtém o componente CompositeCollider2D do Tilemap
                        CompositeCollider2D compositeCollider2D = newTilemap.GetComponent<CompositeCollider2D>();

                        // Verifica se o CompositeCollider2D existe
                        if (compositeCollider2D != null)
                        {
                            // Obtém o Rigidbody2D associado ao CompositeCollider2D
                            Rigidbody2D rb = compositeCollider2D.attachedRigidbody;

                            // Verifica se o Rigidbody2D existe
                            if (rb != null)
                            {
                                // Define o tipo de corpo como estático
                                rb.bodyType = RigidbodyType2D.Static;
                            }
                        }

                        //adicionar colisor ou remover
                        if (tilemapData.isSolid && !tilemapData.isWallPlatform)
                        {
                            SetTilemapLayer(newTilemap, groundLayer);
                        }
                        if (tilemapData.isSolid && tilemapData.isWallPlatform)
                        {
                            SetTilemapLayer(newTilemap, wallLayer);
                        }
                    }
                    else
                    {
                        // Define a layer do Tilemap para a layer "Default"
                        SetTilemapLayer(newTilemap, defaultLayer);
                    }



                    // Percorre os TileData do TilemapData
                    foreach (TileData tileData in tilemapData.tiles)
                    {
                        // Carrega a telha do TileData.tileName
                        TileBase tile = GetTileByName(tileData.tileName);

                        // Verifica se a telha existe
                        if (tile != null)
                        {
                            // Define a telha no Tilemap na posição cellPos
                            newTilemap.SetTile(tileData.cellPos, tile);
                        }
                        else
                        {
                            Debug.LogWarning("Tile not found: " + tileData.tileName);
                        }
                    }
                }
            }
            else
            {
                Debug.Log("Novos dados");
                // Cria um novo TilemapData com um Tilemap vazio
                TilemapData emptyTilemapData = new TilemapData();
                emptyTilemapData.tilemapName = "EmptyTilemap";
                emptyTilemapData.tilemapIndex = 0;
                emptyTilemapData.isSolid = false;
                emptyTilemapData.isWallPlatform = false;
                emptyTilemapData.shortLayerPos = 0;
                emptyTilemapData.zPos = 0;
                emptyTilemapData.tiles = new List<TileData>();

                // Crie um objeto TilemapDataWrapper com as informações padrão

                TilemapDataWrapper defaultWorldData = new TilemapDataWrapper();
                defaultWorldData.gridSizeData = new GridSizeData();
                defaultWorldData.gridSizeData.currentGridWidth = 20; // Tamanho de exemplo
                defaultWorldData.gridSizeData.currentGridHeight = 20; // Tamanho de exemplo
                defaultWorldData.tilemapDataList = new List<TilemapData>() { emptyTilemapData }; // Lista vazia
                defaultWorldData.gameObjectSaveData = new List<GameObjectSaveData>(); // Lista vazia

                // Salva o JSON com as informações padrão no arquivo
                string defaultJson = JsonUtility.ToJson(defaultWorldData);
                File.WriteAllText(loadPath, defaultJson);

                // Carrega o mundo recém-criado
                //LoadSelectedWorld(defaultWorldData);
            }
        }
        else
        {
            Debug.LogWarning("Save file not found: " + loadPath);
        }
    }
    private TileBase GetTileByName(string tileName)
    {
        // Percorre todas as categorias de telhas
        foreach (var category in tileCategories)
        {
            // Procura o tile pelo nome na categoria atual
            foreach (var tile in category.tiles)
            {
                if (tile.name == tileName)
                {
                    return tile;
                }
            }
        }

        return null; // Retorna null se o tile não for encontrado
    }
    public void ClearTilemaps()
    {
        // Destroi todos os Tilemaps na cena
        foreach (Tilemap tilemap in tilemaps)
        {
            Destroy(tilemap.gameObject);
        }

        // Limpa a lista de Tilemaps
        tilemaps.Clear();

        // Destroi todos os botões na UI
        foreach (Button button in tilemapButtons)
        {
            Destroy(button.gameObject);
        }

        // Limpa a lista de botões
        tilemapButtons.Clear();

        // Remove todos os listeners dos botões
        RemoveTilemapButtonListeners();
    }
    void RemoveTilemapButtonListeners()
    {
        // Remova os ouvintes dos botões do Tilemap
        foreach (Button button in tilemapButtons)
        {
            button.onClick.RemoveAllListeners();
        }

        // Limpa a lista de botões do Tilemap
        tilemapButtons.Clear();
    }

    public ObjectType GetObjectType(string objectName)
    {
        // Percorre todas as categorias no ScriptableObjectData
        foreach (ObjectsData.ObjectCategory category in ScriptableObjectData.categories)
        {
            // Procura o objeto pelo nome dentro da categoria atual
            ObjectsData.ObjectsInfo objectInfo = category.Objects.Find(obj => obj.ObjectName == objectName);

            // Se o objeto for encontrado, retorna o tipo dele
            if (objectInfo != null)
            {
                return objectInfo.objectType;
            }
        }

        // Caso o objeto não seja encontrado, retorna ObjectType.Normal por padrão
        return ObjectType.Normal;
    }

    public string saveFileName = "tilemap_save.json";

    private void ClearEnemies()
    {
        // Remove todos os inimigos do contêiner de inimigos
        foreach (Transform child in enemyContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }


    private void ClearGameObjects()
    {
        // Remove todos os inimigos do contêiner de inimigos
        foreach (Transform child in GameObjectsContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void ClearObjects()
    {
        // Remove todos os objetos do contêiner de objetos
        foreach (Transform child in objectsContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void ClearDecor()
    {
        // Remove todos os elementos decorativos do contêiner de elementos decorativos
        foreach (Transform child in decorContainer.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in decor2Container.transform)
        {
            Destroy(child.gameObject);
        }
    }
    private void ClearNodes()
    {
        // Remove todos os elementos que possuem a tag "Node"
        GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");
        foreach (GameObject node in nodes)
        {
            Destroy(node);
        }
    }

    private void SetTilemapLayer(Tilemap tilemap, LayerMask layerMask)
    {
        tilemap.gameObject.layer = LayerMaskToLayer(layerMask);
    }
    private string GetShortLayerName(int layerID)
    {
        // Encontre o nome correspondente ao ID do short layer no objeto atual
        foreach (var layer in SortingLayer.layers)
        {
            if (layer.id == layerID)
            {
                return layer.name;
            }
        }

        // Retorna uma string vazia se não encontrar correspondência
        return string.Empty;
    }
    private int LayerMaskToLayer(LayerMask layerMask)
    {
        int layerNumber = layerMask.value;
        int layer = 0;
        while (layerNumber > 0)
        {
            layerNumber = layerNumber >> 1;
            layer++;
        }
        return layer - 1;
    }
    public void DeleteCurrentLevel(string worldName)
{
    // Obtém o caminho completo para o arquivo do nível atual
    string levelFilePath = Path.Combine(WorldManager.instance.levelEditorPath, worldName, saveFileName);

    // Verifica se o arquivo existe
    if (File.Exists(levelFilePath))
    {
        // Exclui o arquivo do nível
        File.Delete(levelFilePath);

        Debug.Log("Nível excluído: " + levelFilePath);
    }
    else
    {
        Debug.LogWarning("O nível não existe: " + levelFilePath);
    }
}

public void DuplicateCurrentLevel(string worldName, string levelName)
{
    // Obtém o caminho completo para a pasta do mundo
    string worldFolderPath = Path.Combine(WorldManager.instance.levelEditorPath, worldName);

    // Verifica se a pasta do mundo existe
    if (Directory.Exists(worldFolderPath))
    {
        // Obtém o caminho completo para o arquivo do nível atual
        string levelFilePath = Path.Combine(worldFolderPath, levelName + ".TAOWLE");

        // Verifica se o arquivo do nível atual existe
        if (File.Exists(levelFilePath))
        {
            // Obtém o diretório e o nome do arquivo do nível atual
            string levelDirectory = Path.GetDirectoryName(levelFilePath);
            string levelFileName = Path.GetFileNameWithoutExtension(levelFilePath);

            // Verifica se o nível atual já possui um número entre parênteses no nome
            Regex regex = new Regex(@"(.+)\((\d+)\)$");
            Match match = regex.Match(levelFileName);

            // Se o nível atual não possui um número entre parênteses no nome, adiciona "(1)"
            if (!match.Success)
            {
                levelFileName += "(1)";
            }
            else
            {
                // Se o nível atual já possui um número entre parênteses no nome, incrementa o número
                int count = int.Parse(match.Groups[2].Value);
                count++;
                levelFileName = match.Groups[1].Value + "(" + count.ToString() + ")";
            }

            // Obtém o caminho completo para o novo arquivo do nível duplicado
            string newLevelFilePath = Path.Combine(levelDirectory, levelFileName + ".TAOWLE");

            // Copia o arquivo do nível atual para o novo arquivo
            File.Copy(levelFilePath, newLevelFilePath);

            Debug.Log("Nível duplicado com sucesso: " + levelFileName);
        }
        else
        {
            Debug.LogWarning("O arquivo do nível atual não existe!");
        }
    }
    else
    {
        Debug.LogWarning("A pasta do mundo não existe!");
    }
}

    #endregion

}

#region Data To save Level
[System.Serializable]
public class TilemapDataList
{
    public List<TilemapData> tilemapDataList;

    public TilemapDataList(List<TilemapData> tilemapDataList)
    {
        this.tilemapDataList = tilemapDataList;
    }
}

[System.Serializable]
public class TilemapData
{
    public string tilemapName;
    public int tilemapIndex;
    public bool isSolid;
    //public bool isOneWayPlatform;
    public bool isWallPlatform;
    public int shortLayerPos;
    public float zPos;
    public List<TileData> tiles;
}

[System.Serializable]
public class TileData
{
    public Vector3Int cellPos;
    public string tileName;
    public Vector3 localPos;
}

[System.Serializable]
public class GridSizeData
{
    public int currentGridWidth;
    public int currentGridHeight;
}

[System.Serializable]
public class LevelPreferences
{
    public int MusicID;
    public int levelTime;
    public string BackGroundName;
    public string WeatherName;
    public string TimeWeather;
}

[System.Serializable]
public class EnemySaveData
{
    public string name;
    public Vector3 position;
}

[System.Serializable]
public class GameObjectSaveData
{
    public string name;
    public Vector3 position;
}

[System.Serializable]
public class DecorSaveData
{
    public string name;
    public Vector3 position;
}

[System.Serializable]
public class Decor2SaveData
{
    public string name;
    public Vector3 position;
    public string shortLayerName;
}

[System.Serializable]
public class ObjectSaveData
{
    public string name;
    public Vector3 position;
    public ObjectType objectType; // Enum do código anterior
    public List<MovementNodeData> node; // Lista de nós de movimento
    public bool isCircular;
    public bool isPingPong;
    public string id;
}


[System.Serializable]
public class MovementNodeData
{
    public Vector3 position;
    public float nodeTime;
}

[System.Serializable]
public class LevelData
{
    public string levelName;
    public string author;
}

[System.Serializable]
public class LevelDotData
{
    public string levelPath;
    public Vector3 dotPosition;
}

[System.Serializable]
public class TilemapDataWrapper
{
    public GridSizeData gridSizeData;
    public string levelName;
    public string author;
    public LevelPreferences levelPreferences;
    public List<TilemapData> tilemapDataList;
    public List<EnemySaveData> enemySaveData;
    public List<GameObjectSaveData> gameObjectSaveData;
    public List<DecorSaveData> decorSaveData;
    public List<Decor2SaveData> decor2SaveData;
    public List<ObjectSaveData> objectSaveData;
    public List<LevelDotData> levelDotDataList;
}

#endregion


