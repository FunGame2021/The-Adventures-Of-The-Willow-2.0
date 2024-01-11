using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Text.RegularExpressions;
using System.Collections;


public class LevelEditorManager : MonoBehaviour
{
    public static LevelEditorManager instance;

    public Camera mainCamera;

    #region Containers Hide & Show
    public GameObject enemyContainer;
    public GameObject tilemapContainer;
    public GameObject decorContainer;
    public GameObject objectsContainer;
    public GameObject nodesLineRendererContainer;

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
    public Toggle toggleIcePlatform;
    public Toggle toggleStickyPlatform;
    public TMP_InputField zPosInput;
    public TMP_InputField ShortLayerPosInput;



    public GameObject tilemapOptionsPanel; // Referência ao painel de opções
    public GameObject deleteWarningPanel;
    public Button okButton; // Referência ao botão "OK" no painel de opções
    [HideInInspector] public Button selectedButton; // Variável para armazenar o botão selecionado


    [HideInInspector] public Tilemap selectedTilemap;

    //Armazenar Info Temporáriamente
    [HideInInspector] public string tempTilemapName;
    [HideInInspector] public bool tempIsSolid;
    [HideInInspector] public bool tempIsWallPlatform;
    [HideInInspector] public bool tempIsIcePlatform;
    [HideInInspector] public bool tempIsStickyPlatform;
    [HideInInspector] public float tempZPos;
    [HideInInspector] public int tempShortLayerPos;


    public LayerMask wallLayer;
    public LayerMask groundLayer;
    public LayerMask defaultLayer;
    public string IceTag;
    public string StickyTag;

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

    public Transform EnemyContainer; public EraserTool eraserTool;

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

    public TextMeshProUGUI infoGridX; // Referência ao InputField para X
    public TextMeshProUGUI infoGridY; // Referência ao InputField para Y

    public int gridSizeX; // Variável para armazenar o valor de X
    public int gridSizeY; // Variável para armazenar o valor de Y

    public LineRenderer gridOutline; // Referência ao LineRenderer para desenhar a linha da grelha

    public GameObject warnSizeGrid;
    public UnityEngine.UI.Button confirmButton;
    public GameObject gridPanel;
    #endregion

    public TMP_InputField levelNameInput;
    public TMP_InputField authorInput;

    [SerializeField] private List<TileCategoryData> tileCategories; // Lista de categorias de telhas

    #region WorldMap
    public bool isWorldMapEditor;
    [SerializeField] private GameObject levelDotPrefab;
    #endregion

    [SerializeField] private GridVisualizer gridVisualizer;

    [SerializeField] private GameObject[] PanelLevelEditor;
    [SerializeField] private GameObject[] PanelWorldEditor;

    //Info Level Edito Object selected to intantiate
    public string selectedStringInfo;

    [SerializeField] private GameObject SaveSuccessfullPanel;
    [SerializeField] private GameObject WarnExitSavePanel;
    public float autoSaveTime;
    private bool shouldAutoSave = true; // Adicione esta variável
    public bool CoroutineCalled;
    private Coroutine autoSaveCoroutine;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        shouldAutoSave = false;

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
        tempIsIcePlatform = toggleIcePlatform.isOn;
        tempIsStickyPlatform = toggleStickyPlatform.isOn;
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
                    // Verifica a tag do objeto e altera o nome conforme necessário
                    if (instantiatedObject.CompareTag("MovingPlatform"))
                    {
                        // Obtém uma lista de objetos com a mesma tag
                        GameObject[] movingPlatforms = GameObject.FindGameObjectsWithTag("MovingPlatform");

                        // Gere um ID exclusivo usando System.Guid
                        string uniqueID = System.Guid.NewGuid().ToString();

                        // Use o ID exclusivo para nomear o objeto instanciado
                        instantiatedObject.name = selectedObjectName + "_" + uniqueID;

                    }
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

                        if (gameObjectPrefab != null && SectorManager.instance.currentSectorName == "Sector1")
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
            foreach (GameObject panel in PanelWorldEditor)
            {
                panel.SetActive(true);
            }
            foreach (GameObject panel in PanelLevelEditor)
            {
                panel.SetActive(false);
            }
        }
        else
        {
            foreach (GameObject panel in PanelWorldEditor)
            {
                panel.SetActive(false);
            }
            foreach (GameObject panel in PanelLevelEditor)
            {
                panel.SetActive(true);
            }
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

        // Verifique se o tilemap tem um componente TilemapCollider2D
        bool hasCollider = tilemap.GetComponent<TilemapCollider2D>() != null;

        toggleSolid.isOn = hasCollider;
        toggleWallPlatform.isOn = hasCollider && tilemap.gameObject.layer == LayerMask.NameToLayer("Wall"); // Use LayerMask.NameToLayer para comparar a camada.
        toggleIcePlatform.isOn = hasCollider && tilemap.tag == "IcePlatform";
        toggleStickyPlatform.isOn = hasCollider && tilemap.tag == "StickyPlatform";
        zPosInput.text = tilemap.transform.position.z.ToString();

        // Obtenha o componente Renderer do Tilemap
        Renderer objectRenderer = tilemap.GetComponent<TilemapRenderer>();
        if (objectRenderer != null)
        {
            // Defina o valor do Sorting Layer no campo ShortLayerPosInput
            ShortLayerPosInput.text = objectRenderer.sortingOrder.ToString();
        }
        else
        {
            // Se não houver Renderer, defina o campo como vazio ou um valor padrão
            ShortLayerPosInput.text = "";
        }
        // Armazena as informações temporárias
        tempTilemapName = tilemapNameInput.text;
        tempIsSolid = toggleSolid.isOn;
        tempIsWallPlatform = toggleWallPlatform.isOn;
        tempIsIcePlatform = toggleIcePlatform.isOn;
        tempIsStickyPlatform = toggleStickyPlatform.isOn;
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
        tempIsStickyPlatform = toggleStickyPlatform.isOn;
        tempIsWallPlatform = toggleWallPlatform.isOn;
        tempIsIcePlatform = toggleIcePlatform.isOn;
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

            // Adicionar/Remover Collider2D e CompositeCollider2D
            if (tempIsSolid || tempIsWallPlatform || tempIsIcePlatform || tempIsStickyPlatform)
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

            // Define a camada e a tag do Tilemap
            if (tempIsSolid && !tempIsWallPlatform)
            {
                SetTilemapLayer(selectedTilemap, groundLayer);
            }
            else if(tempIsWallPlatform)
            {
                SetTilemapLayer(selectedTilemap, wallLayer);
            }
            else
            {
                SetTilemapLayer(selectedTilemap, defaultLayer);
            }


            if (tempIsIcePlatform)
            {
                selectedTilemap.gameObject.tag = IceTag;
            }
            else if(tempIsStickyPlatform)
            {
                selectedTilemap.gameObject.tag = StickyTag;
            }
            else
            {
                selectedTilemap.gameObject.tag = "ground";
            }

            // Ajusta o valor de Z-pos
            float zPos;
            if (float.TryParse(zPosInput.text, out zPos))
            {
                selectedTilemap.transform.position = new Vector3(selectedTilemap.transform.position.x, selectedTilemap.transform.position.y, zPos);
            }

            Renderer objectRenderer = selectedTilemap.GetComponent<TilemapRenderer>();
            // Altera a posição do sorting layer
            if (objectRenderer != null)
            {
                int shortLayerPos;
                if (int.TryParse(ShortLayerPosInput.text, out shortLayerPos))
                {
                    objectRenderer.sortingOrder = shortLayerPos;
                }
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
        LevelEditorCamera levelEditorCamera = FindObjectOfType<LevelEditorCamera>();
        if (levelEditorCamera != null)
        {
            levelEditorCamera.UpdateCameraBounds();
        }
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
        gridPanel.SetActive(false);
    }
    private void PerformGridResize(int newWidth, int newHeight)
    {
        currentGridWidth = newWidth;
        currentGridHeight = newHeight;
        GenerateGrid();
        DrawGridOutline();
        gridVisualizer.OnGridSizeUpdated();

        gridPanel.SetActive(false);
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
    public void OpenGridMenu()
    {
        infoGridX.text = currentGridWidth.ToString();
        infoGridY.text = currentGridHeight.ToString();
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
        nodesLineRendererContainer.SetActive(isOn);
    }
    #endregion



    #region Save Data
    public void EnableAutoSave()
    {
        shouldAutoSave = true;
    }
    public void DisableAutoSave()
    {
        shouldAutoSave = false;
        if (autoSaveCoroutine != null)
        {
            StopCoroutine(autoSaveCoroutine);
            autoSaveCoroutine = null;
        }
        CoroutineCalled = false;
    }

    public void StartAutoSaveCoroutine()
    {
        if (AutoSaveManager.instance != null && AutoSaveManager.instance.autoSave && !CoroutineCalled && shouldAutoSave)
        {
            CoroutineCalled = true;
            autoSaveTime = AutoSaveManager.instance.GetAutoSaveInterval();

            // Inicia a rotina de salvamento automático apenas se ainda não estiver em execução
            if (autoSaveCoroutine == null)
            {
                autoSaveCoroutine = StartCoroutine(AutoSaveRoutine());
            }
        }
    }

    private IEnumerator AutoSaveRoutine()
    {
        while (shouldAutoSave)
        {
            yield return new WaitForSeconds(autoSaveTime);

            //for (int countdown = (int)autoSaveTime; countdown > 0; countdown--)
            //{
            //    Debug.Log($"Próximo salvamento automático em {countdown} segundos");
            //    yield return new WaitForSeconds(1f);
            //}

            // Chama a função de salvamento automático
            Save();
        }
    }
    public void SaveBeforeExit()
    {
        StartCoroutine(SaveAndExit());
    }
    public void SaveBeforeTest()
    {
        StartCoroutine(SaveAndTest());
    }
    private IEnumerator SaveAndTest()
    {
        // Chama a função SaveLevel e aguarda até que ela termine
        yield return StartCoroutine(SaveLevel());

        // Após o término de SaveLevel
        WorldManager.instance.TestGame();
    }
    private IEnumerator SaveAndExit()
    {
        // Chama a função SaveLevel e aguarda até que ela termine
        yield return StartCoroutine(SaveLevel());

        // Após o término de SaveLevel, chama ToExitLevelEditor
        ToExitLevelEditor();
    }
    private void ToExitLevelEditor()
    {
        WarnExitSavePanel.SetActive(false);
        SaveSuccessfullPanel.SetActive(true);
    }
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

        // Cria um novo objeto LevelDataWrapper para conter as informações do nível
        LevelDataWrapper levelDataWrapper = new LevelDataWrapper();

        // Define o nome do nível e o autor no LevelDataWrapper
        levelDataWrapper.levelName = levelName;
        levelDataWrapper.author = author;
        levelDataWrapper.levelTime = 60;

        // Crie um novo setor chamado "Sector1" e preencha seus dados
        SectorData sectorData1 = new SectorData();
        // Define o nome do setor
        sectorData1.sectorName = "Sector1"; // Defina o nome do setor


        // Define o tamanho da grade do setor como 20x20
        sectorData1.gridSizeData = new GridSizeData();
        sectorData1.gridSizeData.currentGridWidth = 20; // Largura da grade
        sectorData1.gridSizeData.currentGridHeight = 20; // Altura da grade


        // Define as preferências do nível, como a música (MusicID)
        sectorData1.levelPreferences = new LevelPreferences();
        sectorData1.levelPreferences.MusicID = 1; // ID da música do nível
        sectorData1.levelPreferences.BackgroundName = "GreenMountains_01";
        sectorData1.levelPreferences.BackgroundOffset = 9.5f;


        // Crie um novo TilemapData vazio para o setor
        TilemapData emptyTilemapData = new TilemapData();
        emptyTilemapData.tilemapName = "EmptyTilemap"; // Nome do tilemap
        emptyTilemapData.tilemapIndex = 0; // Índice do tilemap
        emptyTilemapData.isSolid = false; // Define se é sólido
        emptyTilemapData.isWallPlatform = false; // Define se é uma plataforma de parede
        emptyTilemapData.shortLayerPos = 0; // Posição da camada curta
        emptyTilemapData.zPos = 0; // Posição Z
        emptyTilemapData.tiles = new List<TileData>(); // Lista de dados do tile


        // Adicione o TilemapData vazio ao setor
        sectorData1.tilemapDataList = new List<TilemapData>() { emptyTilemapData };


        // Inicialize as listas para dados de inimigos, decoração, objetos, etc.
        sectorData1.sectorName = "Sector1";
        sectorData1.enemySaveData = new List<EnemySaveData>();
        sectorData1.decorSaveData = new List<DecorSaveData>();
        sectorData1.decor2SaveData = new List<Decor2SaveData>();
        sectorData1.objectSaveData = new List<ObjectSaveData>();
        sectorData1.movingObjectsaveData = new List<MovingObjectSaveData>();
        sectorData1.gameObjectSaveData = new List<GameObjectSaveData>();
        sectorData1.triggerGameObjectSaveData = new List<TriggerGameObjectSaveData>();

        // Adicione sectorData1 à lista de setores no LevelDataWrapper
        levelDataWrapper.sectorData = new List<SectorData> { sectorData1 };

        // Converte o objeto LevelDataWrapper em formato JSON
        string json = JsonUtility.ToJson(levelDataWrapper);
        // Obtém o caminho completo para a pasta do mundo atual
        string worldFolderPath = Path.Combine(WorldManager.instance.levelEditorPath, WorldManager.instance.currentWorldName);


        // Verifica se a pasta do mundo existe
        if (!Directory.Exists(worldFolderPath))
        {
            // Cria a pasta do mundo se não existir
            Directory.CreateDirectory(worldFolderPath);
        }

        // Obtém o caminho completo para o arquivo JSON com a extensão ".TAOWLE" dentro da pasta do mundo
        string newLevelFilePath = Path.Combine(worldFolderPath, levelName + ".TAOWLE");


        // Salva o JSON em um arquivo
        File.WriteAllText(newLevelFilePath, json);


        // Atualiza a lista de botões de nível
        WorldManager.instance.InstantiateLevelButtons(WorldManager.instance.currentWorldName);

        // Exibe uma mensagem de log informando que o novo nível foi criado e salvo
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
        if (isWorldMapEditor)
        {
            StartCoroutine(SaveWorld());
        }
        else
        {
            StartCoroutine(SaveLevel());
        }
    }
    public IEnumerator SaveLevel()
    {
        // Obtém o nome do nível e autor dos campos de entrada
        string levelName = levelNameInput.text;
        string author = authorInput.text;

        // Cria um objeto LevelData para salvar os dados do nível
        LevelDataWrapper levelDataWrapper = new LevelDataWrapper();
        levelDataWrapper.levelName = levelName;
        levelDataWrapper.author = author;
        levelDataWrapper.levelTime = LevelSettings.instance.levelTime;

        LoadExistingSectors(levelDataWrapper);



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
            tilemapData.isIce = tilemap.gameObject.CompareTag(IceTag);
            tilemapData.isSticky = tilemap.gameObject.CompareTag(StickyTag);

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
        List<TriggerGameObjectSaveData> triggerList = new List<TriggerGameObjectSaveData>();
        List<ParticlesSaveData> particlesList = new List<ParticlesSaveData>();
        GameObject[] gameObjectObjects = GameObject.FindGameObjectsWithTag("GameObject");
        foreach (GameObject gameObjectObject in gameObjectObjects)
        {
            string gameObjectName = gameObjectObject.name.Replace("(Clone)", "");
            Vector3 gameObjectPosition = gameObjectObject.transform.position;

            // Verifique se o objeto tem o nome "Trigger"
            if (gameObjectName.Contains("Trigger"))
            {
                // Obtenha o script TriggerObject do objeto "Trigger" diretamente
                TriggerObject triggerObjectScript = gameObjectObject.GetComponentInChildren<TriggerObject>();

                if (triggerObjectScript != null)
                {
                    // Crie um objeto TriggerGameObjectSaveData
                    TriggerGameObjectSaveData triggerData = new TriggerGameObjectSaveData();

                    // Agora você pode acessar o valor triggerType
                    string triggerType = triggerObjectScript.thisTriggerType;
                    Vector2 triggerScale = triggerObjectScript.thisScale;

                    triggerData.name = gameObjectName;
                    triggerData.position = gameObjectPosition;
                    triggerData.type = triggerType;
                    triggerData.scale = triggerScale;
                    triggerData.customScript = triggerObjectScript.customScript;
                    triggerData.timeToPlay = triggerObjectScript.timeToPlayTrigger;
                    triggerData.wasWaitTime = triggerObjectScript.wasTriggerWaitTime;
                    // Adicione o objeto TriggerGameObjectSaveData à lista de objetos Trigger
                    triggerList.Add(triggerData);
                }
                else
                {
                    Debug.LogWarning("TriggerObject script not found on Trigger: " + gameObjectName);
                }
            }
            else if(gameObjectName.Contains("Particle"))
            {
                ParticlesObject particlesObjectScript = gameObjectObject.GetComponent<ParticlesObject>();
                if(particlesObjectScript != null)
                {
                    // Crie um objeto TriggerGameObjectSaveData
                    ParticlesSaveData particlesData = new ParticlesSaveData();
                    string particleType = particlesObjectScript.particleType;
                    string particleName = particlesObjectScript.particleName;
                    bool initialStarted = particlesObjectScript.initialStarted;
                    bool isLoop = particlesObjectScript.isLoop;

                    particlesData.name = gameObjectName;
                    particlesData.particleType = particleType;
                    particlesData.particleName = particleName;
                    particlesData.initialStarted = initialStarted;
                    particlesData.isLoop = isLoop;
                    particlesData.position = gameObjectPosition;

                    particlesList.Add(particlesData);
                }

            }
            else
            {
                // Crie um objeto GameObjectSaveData para outros objetos
                GameObjectSaveData gameObjectData = new GameObjectSaveData();
                gameObjectData.name = gameObjectName;
                gameObjectData.position = gameObjectPosition;

                gameObjectList.Add(gameObjectData);
            }
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

            objectList.Add(objectData);
        }


        List<MovingObjectSaveData> movingObjectList = new List<MovingObjectSaveData>();
        GameObject[] objectMovingPlatform = GameObject.FindGameObjectsWithTag("MovingPlatform");
        foreach (GameObject objectMP in objectMovingPlatform)
        {
            PlatformController movementComponent = objectMP.GetComponent<PlatformController>();

            if (movementComponent != null)
            {
                string objectName = movementComponent.thisPlatformNameSaveEditor;

                Vector3 objectPosition = objectMP.transform.position;

                // Obtém o tipo do objeto a partir do ScriptableObjectData
                ObjectType objectType = GetObjectType(objectName);

                MovingObjectSaveData movingObjectData = new MovingObjectSaveData();
                movingObjectData.name = objectName;
                movingObjectData.position = objectPosition;
                movingObjectData.objectType = objectType;
                movingObjectData.initialStart = movementComponent.initialStart;
                movingObjectData.rightStart = movementComponent.rightStart;
                movingObjectData.speed = movementComponent.moveSpeed;
                movingObjectData.stopDistance = movementComponent.stopDistance;
                movingObjectData.id = movementComponent.platformMoveid;

                // Preenche os dados relacionados ao movimento apenas para objetos do tipo "Moving"
                if (objectType == ObjectType.Moving)
                {
                    if (movementComponent.behaviorType == WaypointBehaviorType.PingPong)
                    {
                        movingObjectData.isPingPong = true;
                    }
                    else
                    {
                        movingObjectData.isPingPong = false;
                    }
                    if (movementComponent.pathType == WaypointPathType.Closed)
                    {
                        movingObjectData.isClosed = true;
                    }
                    else
                    {
                        movingObjectData.isClosed = false;
                    }

                    List<MovementNodeData> nodeData = new List<MovementNodeData>();
                    foreach (Vector3 waypointPosition in movementComponent.waypoints)
                    {
                        MovementNodeData node = new MovementNodeData();
                        node.position = waypointPosition;
                        nodeData.Add(node);
                    }

                    movingObjectData.node = nodeData;


                }

                movingObjectList.Add(movingObjectData);
            }
        }
        #endregion

        foreach (GameObjectSaveData gameObjectData in gameObjectList)
        {
            if (gameObjectData.name == "PlayerPos")
            {
                // Captura um screenshot do nível
                LevelShoot.instance.CaptureAndSaveScreenshot(levelName, currentGridWidth, currentGridHeight);
            }
        }
        // Inicialize levelDataWrapper.sectorData se estiver nulo
        if (levelDataWrapper.sectorData == null)
        {
            levelDataWrapper.sectorData = new List<SectorData>();
        }
        else
        {
            // Carregue os dados existentes do arquivo JSON, se houver
            LoadExistingSectors(levelDataWrapper);
        }

        // Obtém o nome do setor atualmente selecionado
        string currentSectorName = SectorManager.instance.currentSectorName;
        bool sectorExistsInSave = false;
        SectorData currentSectorData = null;

        // Verifica se o nome do setor atual está no JSON de save
        foreach (SectorData sectorData in levelDataWrapper.sectorData)
        {
            if (sectorData.sectorName == currentSectorName)
            {
                // Marque que o setor existe no save
                sectorExistsInSave = true;

                // Guarde uma referência para o setor encontrado
                currentSectorData = sectorData;
                // Saia do loop, pois o setor foi encontrado e atualizado
                break;
            }
        }

        if (!sectorExistsInSave)
        {
            // Se o setor não existir no save, crie um novo setor
            currentSectorData = new SectorData();
            currentSectorData.sectorName = currentSectorName;
            // Preencha com os dados apropriados
            // Define o tamanho da grade do setor como 20x20
            currentSectorData.gridSizeData = new GridSizeData();
            currentSectorData.gridSizeData.currentGridWidth = 20; // Largura da grade
            currentSectorData.gridSizeData.currentGridHeight = 20; // Altura da grade

            currentSectorData.levelPreferences = new LevelPreferences();

            currentSectorData.levelPreferences.MusicID = 1; // ID da música do nível
            currentSectorData.levelPreferences.BackgroundName = "GreenMountains_01";
            currentSectorData.levelPreferences.BackgroundOffset = 9.5f;

            currentSectorData.enemySaveData = new List<EnemySaveData>();
            currentSectorData.decorSaveData = new List<DecorSaveData>();
            currentSectorData.decor2SaveData = new List<Decor2SaveData>();
            currentSectorData.objectSaveData = new List<ObjectSaveData>();
            currentSectorData.movingObjectsaveData = new List<MovingObjectSaveData>();
            currentSectorData.triggerGameObjectSaveData = new List<TriggerGameObjectSaveData>();
            currentSectorData.particlesSaveData = new List<ParticlesSaveData>() ;
            currentSectorData.gameObjectSaveData = new List<GameObjectSaveData>();

            // Crie um novo TilemapData vazio para o setor
            TilemapData emptyTilemapData = new TilemapData();
            emptyTilemapData.tilemapName = "EmptyTilemap"; // Nome do tilemap
            emptyTilemapData.tilemapIndex = 0; // Índice do tilemap
            emptyTilemapData.isSolid = false; // Define se é sólido
            emptyTilemapData.isWallPlatform = false; // Define se é uma plataforma de parede
            emptyTilemapData.shortLayerPos = 0; // Posição da camada curta
            emptyTilemapData.zPos = 0; // Posição Z
            emptyTilemapData.tiles = new List<TileData>(); // Lista de dados do tile


            // Adicione o TilemapData vazio ao setor
            currentSectorData.tilemapDataList = new List<TilemapData>() { emptyTilemapData };


            // Adicione o novo setor à lista de setores no save
            levelDataWrapper.sectorData.Add(currentSectorData);
        }
        else
        {
            // Atualize os dados do setor existente com os novos dados
            currentSectorData.gridSizeData = gridSizeData; // Preencha com os dados apropriados
            currentSectorData.levelPreferences = new LevelPreferences(); // Preencha com os dados apropriados
            currentSectorData.levelPreferences.MusicID = LevelSettings.instance.MusicIDToSave;
            currentSectorData.levelPreferences.BackgroundName = LevelSettings.instance.BackgroundToSave;
            currentSectorData.levelPreferences.BackgroundOffset = LevelSettings.instance.BackgroundOffsetToSave;

            currentSectorData.tilemapDataList = tilemapDataList; // Preencha com os dados apropriados
            currentSectorData.enemySaveData = enemyList; // Preencha com os dados apropriados
            currentSectorData.gameObjectSaveData = gameObjectList; // Preencha com os dados apropriados
            currentSectorData.triggerGameObjectSaveData = triggerList;
            currentSectorData.particlesSaveData = particlesList;
            currentSectorData.decorSaveData = decorList; // Preencha com os dados apropriados
            currentSectorData.decor2SaveData = decor2List; // Preencha com os dados apropriados
            currentSectorData.objectSaveData = objectList; // Preencha com os dados apropriados
            currentSectorData.movingObjectsaveData = movingObjectList;
        }


        // Converte o objeto para JSON
        string json = JsonUtility.ToJson(levelDataWrapper, true);

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
        // Indica o término da operação de salvamento
        yield return null;
        Debug.Log("Tilemap data saved to " + savePath);
    }

    private void LoadExistingSectors(LevelDataWrapper levelDataWrapper)
    {
        // Obtém o caminho completo para o arquivo JSON
        string worldFolderPath = Path.Combine(WorldManager.instance.levelEditorPath, WorldManager.instance.currentWorldName);
        string filePath = Path.Combine(worldFolderPath, levelDataWrapper.levelName + ".TAOWLE");

        // Verifica se o arquivo JSON existe
        if (File.Exists(filePath))
        {
            // Lê o conteúdo do arquivo JSON
            string json = File.ReadAllText(filePath);

            // Deserializa o JSON de volta para um objeto LevelDataWrapper
            LevelDataWrapper loadedData = JsonUtility.FromJson<LevelDataWrapper>(json);

            // Verifica se os dados foram carregados com sucesso
            if (loadedData != null)
            {
                // Atribui os dados carregados à lista de setores em levelDataWrapper
                levelDataWrapper.sectorData = loadedData.sectorData;

                Debug.Log("Existing sectors loaded successfully.");
            }
            else
            {
                Debug.LogWarning("Failed to load existing sectors from JSON.");
            }
        }
        else
        {
            Debug.Log("No existing sectors found for this level.");
        }
    }


    public void LoadLevel(string worldName, string level, string sectorName)
    {
        EnableAutoSave();
        StartAutoSaveCoroutine();

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

            // Desserializa o JSON para um objeto LevelDataWrapper
            LevelDataWrapper levelDataWrapper = JsonUtility.FromJson<LevelDataWrapper>(json);
            if (levelDataWrapper != null)
            {
                // Aqui, você pode carregar as informações gerais do nível (nome e autor)
                string loadedLevelName = levelDataWrapper.levelName;
                string loadedAuthor = levelDataWrapper.author;

                // Exemplo: Defina o nome do nível e autor carregados em campos de entrada
                levelNameInput.text = loadedLevelName;
                authorInput.text = loadedAuthor;

                LevelSettings.instance.newLevelTime = levelDataWrapper.levelTime;
                if (SectorManager.instance != null)
                {
                    // Limpe os botões existentes antes de criar novos
                    SectorManager.instance.ClearSectorButtons();
                }

                // Itera sobre a lista de setores no LevelDataWrapper
                foreach (SectorData sectorDataBTN in levelDataWrapper.sectorData)
                {
                    // Crie um botão para cada setor
                    SectorManager.instance.AddSectorList(sectorDataBTN.sectorName);
                    SectorManager.instance.CreateSectorButton(sectorDataBTN.sectorName);
                }

                // Procura o setor especificado pelo nome
                SectorData sectorData = levelDataWrapper.sectorData.Find(sector => sector.sectorName == sectorName);

                if (sectorData != null)
                {
                    // Agora você pode acessar os dados do setor especificado
                    GridSizeData gridSizeData = sectorData.gridSizeData;
                    LevelPreferences levelPreferences = sectorData.levelPreferences;
                    List<TilemapData> tilemapDataList = sectorData.tilemapDataList;
                    List<EnemySaveData> enemyList = sectorData.enemySaveData;
                    List<GameObjectSaveData> gameObjectList = sectorData.gameObjectSaveData;
                    List<TriggerGameObjectSaveData> triggerList = sectorData.triggerGameObjectSaveData;
                    List<ParticlesSaveData> particlesList = sectorData.particlesSaveData;
                    List<DecorSaveData> decorList = sectorData.decorSaveData;
                    List<Decor2SaveData> decor2List = sectorData.decor2SaveData;
                    List<ObjectSaveData> objectList = sectorData.objectSaveData;
                    List<MovingObjectSaveData> movingObjectList = sectorData.movingObjectsaveData;

                    // Limpa os Tilemaps existentes
                    ClearTilemaps();

                    LevelSettings.instance.SetMusicID(sectorData.levelPreferences.MusicID);
                    
                    // Carregar dados do background
                    string backgroundName = sectorData.levelPreferences.BackgroundName; // Substitua pelo nome da variável correta
                    float backgroundOffset = sectorData.levelPreferences.BackgroundOffset; // Substitua pelo nome da variável correta

                    // Chame a função de atualização do background
                    LevelSettings.instance.UpdateBackground(backgroundName, backgroundOffset);

                    LevelSettings.instance.UpdateValues();
                    // Restaura o tamanho do grid
                    currentGridWidth = gridSizeData.currentGridWidth;
                    currentGridHeight = gridSizeData.currentGridHeight;

                    // Gera o grid com o novo tamanho
                    GenerateGrid();
                    DrawGridOutline();
                    LevelEditorCamera levelEditorCamera = FindObjectOfType<LevelEditorCamera>();
                    if (levelEditorCamera != null)
                    {
                        levelEditorCamera.UpdateCameraBounds();
                    }
                    gridVisualizer.OnGridSizeUpdated();

                    // Limpa os inimigos existentes
                    ClearEnemies();
                    // Limpa os objetos existentes
                    ClearObjects();
                    ClearGameObjects();
                    // Limpa os elementos decorativos existentes
                    ClearDecor();
                    ClearNodes();

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


                    // Para objetos Trigger
                    foreach (TriggerGameObjectSaveData triggerData in triggerList)
                    {
                        // Encontre o prefab do objeto Trigger
                        GameObject gameObjectPrefab = null;
                        foreach (GameObjectsData.GameObjectCategory category in ScriptableGameObjectData.categories)
                        {
                            foreach (GameObjectsData.GameObjectsInfo gameObjectInfo in category.GameObjects)
                            {
                                if (gameObjectInfo.GameObjectName == triggerData.name)
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
                            GameObject gameObjectObject = Instantiate(gameObjectPrefab, triggerData.position, Quaternion.identity);
                            gameObjectObject.transform.SetParent(GameObjectsContainer.transform);

                            // Procure o objeto Trigger com a tag "Trigger" aninhado dentro de gameObjectObject
                            GameObject triggerTransform = gameObjectObject.transform.Find("TriggerSquare").gameObject;

                            if (triggerTransform != null)
                            {
                                GameObject triggerObject = triggerTransform.gameObject;

                                // Defina a escala do objeto Trigger
                                triggerObject.transform.localScale = new Vector3(triggerData.scale.x, triggerData.scale.y, 1f);
                            }
                            else
                            {
                                Debug.LogWarning("Trigger object not found inside: " + gameObjectObject.name);
                            }

                            // Agora, configure o componente do script do prefab com os valores de triggerData
                            TriggerObject triggerScript = gameObjectObject.GetComponentInChildren<TriggerObject>();
                            if (triggerScript != null)
                            {
                                triggerScript.thisTriggerType = triggerData.type;
                                triggerScript.customScript = triggerData.customScript;
                                triggerScript.timeToPlayTrigger = triggerData.timeToPlay;
                                triggerScript.wasTriggerWaitTime = triggerData.wasWaitTime;
                            }
                            else
                            {
                                Debug.LogWarning("TriggerObject script not found on Trigger object: " + triggerData.name);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Prefab not found for Trigger object: " + triggerData.name);
                        }
                    }


                    foreach (ParticlesSaveData particleData in particlesList)
                    {
                        // Encontre o prefab do objeto Trigger
                        GameObject gameObjectPrefab = null;
                        foreach (GameObjectsData.GameObjectCategory category in ScriptableGameObjectData.categories)
                        {
                            foreach (GameObjectsData.GameObjectsInfo gameObjectInfo in category.GameObjects)
                            {
                                if (gameObjectInfo.GameObjectName == particleData.name)
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
                            GameObject gameObjectObject = Instantiate(gameObjectPrefab, particleData.position, Quaternion.identity);
                            gameObjectObject.transform.SetParent(GameObjectsContainer.transform);

                            ParticlesObject particlesScript = gameObjectObject.GetComponentInChildren<ParticlesObject>();
                            if (particlesScript != null)
                            {
                                particlesScript.particleType = particleData.particleType;
                                particlesScript.particleName = particleData.particleName;
                                particlesScript.initialStarted = particleData.initialStarted;
                                particlesScript.isLoop = particleData.isLoop;
                            }
                            else
                            {
                                Debug.LogWarning("ParticlesObject script not found on Particle object: " + particleData.name);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Prefab not found for Particle object: " + particleData.name);
                        }
                    }

                    // Para objetos normais
                    foreach (GameObjectSaveData normalObjectData in gameObjectList)
                    {
                        // Encontre o prefab do objeto normal
                        GameObject gameObjectPrefab = null;
                        foreach (GameObjectsData.GameObjectCategory category in ScriptableGameObjectData.categories)
                        {
                            foreach (GameObjectsData.GameObjectsInfo gameObjectInfo in category.GameObjects)
                            {
                                if (gameObjectInfo.GameObjectName == normalObjectData.name)
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
                            // Crie o objeto e defina o nome e a posição
                            GameObject gameObjectObject = Instantiate(gameObjectPrefab, normalObjectData.position, Quaternion.identity);
                            gameObjectObject.transform.SetParent(GameObjectsContainer.transform);
                        }
                        else
                        {
                            Debug.LogWarning("Prefab not found for normal object: " + normalObjectData.name);
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

                            
                        }
                        else
                        {
                            Debug.LogWarning("Prefab not found for object: " + objectData.name);
                        }
                    }

                    foreach (MovingObjectSaveData movingObjectSaveData in movingObjectList)
                    {
                        GameObject objectPrefab = null;
                        foreach (ObjectsData.ObjectCategory category in ScriptableObjectData.categories)
                        {
                            foreach (ObjectsData.ObjectsInfo objectInfo in category.Objects)
                            {
                                if (objectInfo.ObjectName == movingObjectSaveData.name)
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
                            GameObject movingObjectObject = Instantiate(objectPrefab, movingObjectSaveData.position, Quaternion.identity);
                            movingObjectObject.transform.SetParent(objectsContainer.transform);
                            // Gere um ID exclusivo usando System.Guid
                            string uniqueID = System.Guid.NewGuid().ToString();

                            // Use o ID exclusivo para nomear o objeto instanciado
                            movingObjectObject.name = movingObjectSaveData.name + "_" + uniqueID; // Defina o nome no objeto instanciado

                            ObjectType objectType = movingObjectSaveData.objectType;
                           
                            if (objectType == ObjectType.Moving)
                            {
                                PlatformController movementComponent = movingObjectObject.GetComponent<PlatformController>();

                                if (movementComponent != null)
                                {
                                    movementComponent.initialStart = movingObjectSaveData.initialStart;
                                    movementComponent.moveSpeed = movingObjectSaveData.speed;
                                    movementComponent.stopDistance = movingObjectSaveData.stopDistance;
                                    movementComponent.rightStart = movingObjectSaveData.rightStart;
                                    if (movingObjectSaveData.isPingPong)
                                    {
                                        movementComponent.behaviorType = WaypointBehaviorType.PingPong;
                                    }
                                    else
                                    {
                                        movementComponent.behaviorType = WaypointBehaviorType.Loop;
                                    }
                                    if(movingObjectSaveData.isClosed)
                                    {
                                        movementComponent.pathType = WaypointPathType.Closed;
                                    }
                                    else
                                    {
                                        movementComponent.pathType = WaypointPathType.Open;
                                    }
                                    movementComponent.platformMoveid = movingObjectSaveData.id;

                                    movementComponent.waypoints = new List<Vector3>();

                                    foreach (MovementNodeData nodeData in movingObjectSaveData.node)
                                    {
                                        movementComponent.waypoints.Add(nodeData.position);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Prefab not found for object: " + movingObjectSaveData.name);
                        }
                    }

                    PlatformNodeEditor.instance.ObtainCreateAllNodes();

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
                            else if (tilemapData.isWallPlatform)
                            {
                                SetTilemapLayer(newTilemap, wallLayer);
                            }
                            if (tilemapData.isIce)
                            {
                                newTilemap.gameObject.tag = IceTag;
                            }
                            else if(tilemapData.isSticky)
                            {
                                newTilemap.gameObject.tag = StickyTag;
                            }
                            else
                            {
                                newTilemap.gameObject.tag = "ground";
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
                    Debug.LogWarning("Sector not found: " + sectorName);

                    // Carregue o Setor1 por padrão, se o setor especificado não for encontrado
                    sectorName = "Sector1";
                    sectorData = levelDataWrapper.sectorData.Find(sector => sector.sectorName == sectorName);

                    if (sectorData != null)
                    {
                        // Agora você pode acessar os dados do setor especificado
                        GridSizeData gridSizeData = sectorData.gridSizeData;
                        LevelPreferences levelPreferences = sectorData.levelPreferences;
                        List<TilemapData> tilemapDataList = sectorData.tilemapDataList;
                        List<EnemySaveData> enemyList = sectorData.enemySaveData;
                        List<GameObjectSaveData> gameObjectList = sectorData.gameObjectSaveData;
                        List<DecorSaveData> decorList = sectorData.decorSaveData;
                        List<Decor2SaveData> decor2List = sectorData.decor2SaveData;
                        List<ObjectSaveData> objectList = sectorData.objectSaveData;
                        List<MovingObjectSaveData> movingObjectList = sectorData.movingObjectsaveData;

                        // Limpa os Tilemaps existentes
                        ClearTilemaps();

                        LevelSettings.instance.SetMusicID(sectorData.levelPreferences.MusicID);

                        LevelSettings.instance.UpdateValues();
                        // Restaura o tamanho do grid
                        currentGridWidth = gridSizeData.currentGridWidth;
                        currentGridHeight = gridSizeData.currentGridHeight;

                        // Gera o grid com o novo tamanho
                        GenerateGrid();
                        DrawGridOutline();
                        LevelEditorCamera levelEditorCamera = FindObjectOfType<LevelEditorCamera>();
                        if (levelEditorCamera != null)
                        {
                            levelEditorCamera.UpdateCameraBounds();
                        }
                        gridVisualizer.OnGridSizeUpdated();

                        // Limpa os inimigos existentes
                        ClearEnemies();
                        // Limpa os objetos existentes
                        ClearObjects();
                        ClearGameObjects();
                        // Limpa os elementos decorativos existentes
                        ClearDecor();
                        ClearNodes();

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

                            }
                            else
                            {
                                Debug.LogWarning("Prefab not found for object: " + objectData.name);
                            }
                        }

                        foreach (MovingObjectSaveData movingObjectSaveData in movingObjectList)
                        {
                            GameObject objectPrefab = null;
                            foreach (ObjectsData.ObjectCategory category in ScriptableObjectData.categories)
                            {
                                foreach (ObjectsData.ObjectsInfo objectInfo in category.Objects)
                                {
                                    if (objectInfo.ObjectName == movingObjectSaveData.name)
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
                                GameObject movingObjectObject = Instantiate(objectPrefab, movingObjectSaveData.position, Quaternion.identity);
                                movingObjectObject.transform.SetParent(objectsContainer.transform);
                                // Gere um ID exclusivo usando System.Guid
                                string uniqueID = System.Guid.NewGuid().ToString();

                                // Use o ID exclusivo para nomear o objeto instanciado
                                movingObjectObject.name = movingObjectSaveData.name + "_" + uniqueID; // Defina o nome no objeto instanciado

                                ObjectType objectType = movingObjectSaveData.objectType;

                                if (objectType == ObjectType.Moving)
                                {
                                    PlatformController movementComponent = movingObjectObject.GetComponent<PlatformController>();

                                    if (movementComponent != null)
                                    {
                                        movementComponent.initialStart = movingObjectSaveData.initialStart;
                                        movementComponent.moveSpeed = movingObjectSaveData.speed;
                                        movementComponent.stopDistance = movingObjectSaveData.stopDistance;
                                        movementComponent.rightStart = movingObjectSaveData.rightStart;
                                        if (movingObjectSaveData.isPingPong)
                                        {
                                            movementComponent.behaviorType = WaypointBehaviorType.PingPong;
                                        }
                                        else
                                        {
                                            movementComponent.behaviorType = WaypointBehaviorType.Loop;
                                        }
                                        if (movingObjectSaveData.isClosed)
                                        {
                                            movementComponent.pathType = WaypointPathType.Closed;
                                        }
                                        else
                                        {
                                            movementComponent.pathType = WaypointPathType.Open;
                                        }
                                        movementComponent.platformMoveid = movingObjectSaveData.id;

                                        movementComponent.waypoints = new List<Vector3>();

                                        foreach (MovementNodeData nodeData in movingObjectSaveData.node)
                                        {
                                            movementComponent.waypoints.Add(nodeData.position);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Debug.LogWarning("Prefab not found for object: " + movingObjectSaveData.name);
                            }
                        }

                        PlatformNodeEditor.instance.ObtainCreateAllNodes();

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
                                else if (tilemapData.isWallPlatform)
                                {
                                    SetTilemapLayer(newTilemap, wallLayer);
                                }
                                if (tilemapData.isIce)
                                {
                                    newTilemap.gameObject.tag = IceTag;
                                }
                                else if (tilemapData.isSticky)
                                {
                                    newTilemap.gameObject.tag = StickyTag;
                                }
                                else
                                {
                                    newTilemap.gameObject.tag = "ground";
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
                        Debug.LogWarning("Failed to load level data.");
                    }
                }
            }
            else
            {
                Debug.LogWarning("Save file not found: " + loadPath);
            }
        }
    }
    public void DeleteSector(string sectorName, string worldName, string level)
    {
        // Verificar se o setor a ser excluído não é o setor principal (sector1)
        if (sectorName != "Sector1")
        {
            // Obtém o caminho completo para a pasta do mundo
            string worldFolderPath = Path.Combine(WorldManager.instance.levelEditorPath, worldName);

            // Obtém o caminho completo para o arquivo JSON com a extensão ".TAOWLE" dentro da pasta do mundo
            string loadPath = Path.Combine(worldFolderPath, level + ".TAOWLE");

            // Verificar se o arquivo JSON existe
            if (File.Exists(loadPath))
            {
                // Lê o conteúdo do arquivo JSON
                string jsonContent = File.ReadAllText(loadPath);

                // Desserializa o JSON em um objeto LevelDataWrapper
                LevelDataWrapper levelDataWrapper = JsonUtility.FromJson<LevelDataWrapper>(jsonContent);

                // Encontre e remova o setor com o mesmo nome
                int indexToRemove = levelDataWrapper.sectorData.FindIndex(sectorData => sectorData.sectorName == sectorName);
                if (indexToRemove >= 0)
                {
                    levelDataWrapper.sectorData.RemoveAt(indexToRemove);

                    // Serialize o objeto LevelDataWrapper de volta para JSON
                    string updatedJson = JsonUtility.ToJson(levelDataWrapper);

                    // Escreva o JSON atualizado de volta no arquivo
                    File.WriteAllText(loadPath, updatedJson);

                    Debug.Log("Setor removido do JSON: " + sectorName);

                    // Atualize a interface do usuário após a exclusão do setor
                    SectorManager.instance.ClearSectorButtons();

                    // Itera sobre a lista de setores no LevelDataWrapper e recria os botões
                    foreach (SectorData sectorDataBTN in levelDataWrapper.sectorData)
                    {
                        // Crie um botão para cada setor
                        SectorManager.instance.CreateSectorButton(sectorDataBTN.sectorName);
                    }
                }
                else
                {
                    Debug.LogWarning("Setor não encontrado no JSON: " + sectorName);
                }
            }
            else
            {
                Debug.LogWarning("Arquivo JSON não encontrado: " + loadPath);
            }
        }
        else
        {
            Debug.LogWarning("Não é possível excluir o setor principal.");
        }
    }

    public IEnumerator SaveWorld()
    {
        // Cria um objeto LevelData para salvar os dados do nível
        LevelDataWrapper levelDataWrapper = new LevelDataWrapper();

        LoadExistingSectors(levelDataWrapper);

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
                dotData.levelName = levelDot.levelName;
                dotData.worldName = levelDot.worldName;
                dotData.isFirstLevel = levelDot.isFirstLevel;
                Vector3 LevelDotPos = levelDot.transform.position;
                dotData.dotPosition = LevelDotPos;
                levelDotDataList.Add(dotData);
            }
        }

        SectorData sectorData = new SectorData();
        sectorData.levelPreferences = new LevelPreferences();
        sectorData.levelPreferences.MusicID = LevelSettings.instance.MusicIDToSave;
        sectorData.gridSizeData = gridSizeData;
        sectorData.tilemapDataList = tilemapDataList;
        sectorData.gameObjectSaveData = gameObjectList;

        // Adicione a lista de LevelDotData ao objeto TilemapDataWrapper
        sectorData.levelDotDataList = levelDotDataList;

        // Converte o objeto para JSON
        string json = JsonUtility.ToJson(sectorData, true);
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

        // Indica o término da operação de salvamento
        yield return null;
        Debug.Log("Tilemap data saved to " + savePath);
    }

    public void LoadWorld(string worldName)
    {
        EnableAutoSave();
        StartAutoSaveCoroutine();

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
                SectorData tilemapDataWrapper = JsonUtility.FromJson<SectorData>(json);

                // Obtém o objeto GridSizeData do TilemapDataWrapper
                GridSizeData gridSizeData = tilemapDataWrapper.gridSizeData;

                List<GameObjectSaveData> gameObjectList = tilemapDataWrapper.gameObjectSaveData;
                List<LevelDotData> levelDotDataList = tilemapDataWrapper.levelDotDataList;


                // Obtém a lista de TilemapData do TilemapDataWrappere
                List<TilemapData> tilemapDataList = tilemapDataWrapper.tilemapDataList;


                // Limpa os Tilemaps existentes
                ClearTilemaps();


                LevelSettings.instance.SetMusicID(tilemapDataWrapper.levelPreferences.MusicID);
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
                    GameObject newLevelDotObject = Instantiate(levelDotPrefab, dotData.dotPosition, Quaternion.identity, GameObjectsContainer.transform);

                    // Obtenha o componente LevelDot do novo objeto
                    LevelDot newLevelDot = newLevelDotObject.GetComponent<LevelDot>();

                    // Atribua as informações carregadas ao novo objeto LevelDot
                    if (newLevelDot != null)
                    {
                        newLevelDot.SetLevelPath(dotData.worldName, dotData.levelName);
                        newLevelDot.isFirstLevel = dotData.isFirstLevel;
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

                        SetTilemapLayer(newTilemap, groundLayer);
                        
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

                SectorData defaultWorldData = new SectorData();
                defaultWorldData.gridSizeData = new GridSizeData();
                defaultWorldData.gridSizeData.currentGridWidth = 20; // Tamanho de exemplo
                defaultWorldData.gridSizeData.currentGridHeight = 20; // Tamanho de exemplo
                defaultWorldData.tilemapDataList = new List<TilemapData>() { emptyTilemapData }; // Lista vazia
                defaultWorldData.gameObjectSaveData = new List<GameObjectSaveData>(); // Lista vazia

                // Salva o JSON com as informações padrão no arquivo
                string defaultJson = JsonUtility.ToJson(defaultWorldData);
                File.WriteAllText(loadPath, defaultJson);

                // Limpa os inimigos existentes
                ClearEnemies();
                // Limpa os objetos existentes
                ClearObjects();
                ClearGameObjects();
                // Limpa os elementos decorativos existentes
                ClearDecor();
                ClearNodes();
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
        TileBase tile = null;

        // Percorre todas as categorias de telhas, da última para a primeira
        for (int i = tileCategories.Count - 1; i >= 0; i--)
        {
            // Procura o tile pelo nome na categoria atual
            foreach (var tileInCategory in tileCategories[i].tiles)
            {
                if (tileInCategory.name == tileName)
                {
                    tile = tileInCategory;
                    break; // Encontrou a telha, saia do loop interno
                }
            }

            if (tile != null)
            {
                break; // Encontrou a telha, saia do loop externo
            }
        }

        return tile; // Retorna a telha encontrada (ou null se não encontrada)
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
        foreach (Transform child in nodesLineRendererContainer.transform)
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
        GameObject[] nodes = GameObject.FindGameObjectsWithTag("WayPoint");
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

    public void ClearAllScene()
    {
        // Limpa os inimigos existentes
        ClearEnemies();
        // Limpa os objetos existentes
        ClearObjects();
        ClearGameObjects();
        // Limpa os elementos decorativos existentes
        ClearDecor();
        ClearNodes();
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
    public bool isIce;
    public bool isSticky;
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
    public string BackgroundName;
    public float BackgroundOffset;
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
public class TriggerGameObjectSaveData
{
    public string name;
    public Vector3 position;
    public string type; // Campo para o tipo do objeto
    public Vector2 scale; // Campo para a escala do objeto
    public string customScript;
    public float timeToPlay;
    public bool wasWaitTime;
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
    public string id;
}
[System.Serializable]
public class MovingObjectSaveData
{
    public string name;
    public bool initialStart;
    public bool rightStart;
    public float speed;
    public float stopDistance;
    public Vector3 position;
    public ObjectType objectType; // Enum do código anterior
    public List<MovementNodeData> node; // Lista de nós de movimento
    public bool isPingPong;
    public bool isClosed;
    public string id;
}
[System.Serializable]
public class MovementNodeData
{
    public Vector3 position;
}

[System.Serializable]
public class LevelData
{
    public string levelName;
    public string author;
}

[System.Serializable]
public class ParticlesSaveData
{
    public string name;
    public string particleType;
    public string particleName;
    public bool initialStarted;
    public bool isLoop;
    public Vector3 position;
}
   
[System.Serializable] 
public class LevelDotData
{
    public string levelName;
    public string worldName;
    public bool isFirstLevel;
    public Vector3 dotPosition;
}

[System.Serializable]
public class LevelDataWrapper
{
    public string levelName;
    public string author;
    public int levelTime;
    public List<SectorData> sectorData;
}

[System.Serializable]
public class SectorData
{
    public string sectorName;
    public GridSizeData gridSizeData;
    public LevelPreferences levelPreferences;
    public List<TilemapData> tilemapDataList;
    public List<EnemySaveData> enemySaveData;
    public List<GameObjectSaveData> gameObjectSaveData;
    public List<TriggerGameObjectSaveData> triggerGameObjectSaveData;
    public List<ParticlesSaveData> particlesSaveData;
    public List<DecorSaveData> decorSaveData;
    public List<Decor2SaveData> decor2SaveData;
    public List<ObjectSaveData> objectSaveData;
    public List<MovingObjectSaveData> movingObjectsaveData;
    public List<LevelDotData> levelDotDataList;
}

#endregion


