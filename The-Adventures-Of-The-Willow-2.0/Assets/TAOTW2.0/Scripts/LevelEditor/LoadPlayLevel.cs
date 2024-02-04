using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using FMODUnity;
using FMOD.Studio;
using System.Collections;
using static Cinemachine.DocumentationSortingAttribute;

public class LoadPlayLevel : MonoBehaviour
{
    public static LoadPlayLevel instance;

    [SerializeField] private TMP_Text LevelName;
    [SerializeField] private TMP_Text AutorName;
    public string LevelNames;
    public string worldNames;

    public int MusicID;
    private FMOD.Studio.EventInstance musicEventInstance;
    #region Manager
    public int gameLevelTime;
    #endregion
    #region Decor
    public DecorData ScriptableDecorData;
    public Decor2Data ScriptableDecor2Data;
    #endregion

    #region Objects
    public ObjectsData ScriptableObjectData;
    #endregion

    #region Enemy
    public EnemyData ScriptableEnemyData;
    #endregion

    #region Background
    private Transform backgroundLocal;
    private GameObject currentBackgroundInstance;
    public BackgroundData backgroundData;
    private string selectedBackgroundName;
    #endregion

    #region TimeWeather
    private string volumeNameTimeWeather;
    [SerializeField] private TimeWeatherData ScriptableTimeWeatherData;
    private Transform TimeWeatherLocal;
    private GameObject currentTimeWeatherInstance;
    #endregion

    #region GameObject
    public GameObjectsData ScriptableGameObjectData;
    #endregion

    #region Player
    public GameObject PlayerPrefab;
    public Transform PlayerPos;
    #endregion

    #region Tilemap
    public int GridWidth;
    public int GridHeight;
    public int GridWidthCameraSector1;
    public int GridHeightCameraSector1;
    public int GridWidthCameraSector2;
    public int GridHeightCameraSector2;
    public int GridWidthCameraSector3;
    public int GridHeightCameraSector3;
    public int GridWidthCameraSector4;
    public int GridHeightCameraSector4;
    public int GridWidthCameraSector5;
    public int GridHeightCameraSector5;

    [HideInInspector] public List<Tilemap> tilemaps = new List<Tilemap>();


    public LayerMask wallLayer;
    public LayerMask groundLayer;
    public LayerMask defaultLayer;
    public string IceTag;
    public string StickyTag;

    [SerializeField] private List<TileCategoryData> tileCategories; // Lista de categorias de telhas

    #endregion
    [SerializeField]public bool StartedLevel;
    private bool canStart;
    [SerializeField] private GameObject LevelInfoPanel;
    [SerializeField] private GameObject PressStartInfo;

    [SerializeField] private GameObject DeathZonePrefab;
    public bool isPlayingLevel;

    #region Sectors
    [SerializeField] private Transform Sector1;
    [SerializeField] private Transform Sector2;
    [SerializeField] private Transform Sector3;
    [SerializeField] private Transform Sector4;
    [SerializeField] private Transform Sector5;
    #endregion

    #region particles

    [SerializeField] private ParticleTypes scriptableParticleTypesData;

    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        PressStartInfo.SetActive(false);
        canStart = false;
        isPlayingLevel = false;
    }
    void Start()
    {
        if(LevelEditorController.instance != null)
        {
            worldNames = LevelEditorController.instance.AtualWorld;
            LevelNames = LevelEditorController.instance.AtualLevel;
        }
        
        LoadLevel(worldNames, LevelNames);
        StartedLevel = false;
        LevelInfoPanel.SetActive(true);

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

    void Update()
    {
        if (UserInput.instance.playerMoveAndExtraActions.PlayerActions.Jump.WasPressedThisFrame())
        {
            if (!StartedLevel && canStart)
            {
                isPlayingLevel = true;
                StartedLevel = true;
                PlayMusic();
                LevelTimeManager.instance.Begin(gameLevelTime);
                GameStates.instance.isLevelStarted = true;
                ScreenAspectRatio.instance.StartTransitionNow();
                PressStartInfo.SetActive(false);
            }
        }

        if(StartedLevel)
        {
            LevelInfoPanel.SetActive(false);
        }
    }
    private void SetTilemapLayer(Tilemap tilemap, LayerMask layerMask)
    {
        tilemap.gameObject.layer = LayerMaskToLayer(layerMask);
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



    public void LoadLevel(string worldName, string level)
    {
        // Obtém o caminho completo para a pasta do mundo
        string worldFolderPath = Path.Combine(Path.Combine(Application.persistentDataPath, "LevelEditor"), worldName);

        // Obtém o caminho completo para o arquivo JSON com a extensão ".TAOWLE" dentro da pasta do mundo
        string loadPath = Path.Combine(worldFolderPath, level + ".TAOWLE");

        Debug.Log("Tilemap data loaded from " + worldName + "/" + level + ".TAOWLE");


        // Verifica se o arquivo JSON existe
        if (File.Exists(loadPath))
        {
            // Lê o conteúdo do arquivo JSON
            string json = File.ReadAllText(loadPath);

            // Desserializa o JSON para um objeto LevelDataWrapper
            LevelDataWrapper levelDataWrapper = JsonUtility.FromJson<LevelDataWrapper>(json);
            if (levelDataWrapper != null)
            {
                LevelName.text = levelDataWrapper.levelName;
                AutorName.text = levelDataWrapper.author;

                gameLevelTime = levelDataWrapper.levelTime;

                // Para cada setor no JSON
                foreach (SectorData sectorData in levelDataWrapper.sectorData)
                {
                    string sectorSceneName = sectorData.sectorName;

                    // Encontre o transformador correspondente com base no nome do setor
                    Transform sectorTransform = GetSectorTransform(sectorData.sectorName);

                    // Carregue os dados correspondentes no transformador do setor
                    LoadSectorData(sectorData, sectorTransform);

                }


                Sector1.gameObject.SetActive(true);
                Sector2.gameObject.SetActive(false);
                Sector3.gameObject.SetActive(false);
                Sector4.gameObject.SetActive(false);
                Sector5.gameObject.SetActive(false);
                GridHeight = GridHeightCameraSector1;
                GridWidth = GridWidthCameraSector1;

            }
        }
    }
    public void ActiveSector(string sectorToActivate)
    {
        // Desativa todos os transforms dos setores
        DisableAllSectorTransforms();

        // Encontre o transformador correspondente com base no nome do setor
        Transform sectorTransform = GetSectorTransform(sectorToActivate);

        if (sectorTransform != null)
        {
            // Ativa o transformador do setor especificado
            sectorTransform.gameObject.SetActive(true);
            if (sectorTransform == Sector1)
            {
                GridHeight = GridHeightCameraSector1;
                GridWidth = GridWidthCameraSector1;
            }
            else if (sectorTransform == Sector2)
            {
                GridHeight = GridHeightCameraSector2;
                GridWidth = GridWidthCameraSector2;
            }
            else if (sectorTransform == Sector3)
            {
                GridHeight = GridHeightCameraSector3;
                GridWidth = GridWidthCameraSector3;
            }
            else if (sectorTransform == Sector4)
            {
                GridHeight = GridHeightCameraSector4;
                GridWidth = GridWidthCameraSector4;
            }
            else if (sectorTransform == Sector5)
            {
                GridHeight = GridHeightCameraSector5;
                GridWidth = GridWidthCameraSector5;
            }
        }
    }
    private void DisableAllSectorTransforms()
    {
        // Desativa todos os transforms dos setores
        Sector1.gameObject.SetActive(false);
        Sector2.gameObject.SetActive(false);
        Sector3.gameObject.SetActive(false);
        Sector4.gameObject.SetActive(false);
        Sector5.gameObject.SetActive(false);
    }
    private Transform GetSectorTransform(string sectorName)
    {
        switch (sectorName)
        {
            case "Sector1":
                return Sector1;
            case "Sector2":
                return Sector2;
            case "Sector3":
                return Sector3;
            case "Sector4":
                return Sector4;
            case "Sector5":
                return Sector5;
            default:
                return null;
        }
    }

    private void LoadSectorData(SectorData sectorData, Transform sectorTransform)
    {
        GameObject tilemapGrid = new GameObject("Grid");
        tilemapGrid.transform.SetParent(sectorTransform);
        Grid grid = tilemapGrid.AddComponent<Grid>();

        GameObject GameObjectsContainer = new GameObject("GameObjectsContainer");
        Transform GOtransformComponent = GameObjectsContainer.transform;
        GOtransformComponent.SetParent(sectorTransform);

        GameObject objectsContainer = new GameObject("ObjectsContainer");
        Transform OtransformComponent = objectsContainer.transform;
        OtransformComponent.SetParent(sectorTransform);

        GameObject enemyContainer = new GameObject("enemyContainer");
        Transform EtransformComponent = enemyContainer.transform;
        EtransformComponent.SetParent(sectorTransform);

        GameObject decorContainer = new GameObject("decorContainer");
        Transform DtransformComponent = decorContainer.transform;
        DtransformComponent.SetParent(sectorTransform);

        GameObject decor2Container = new GameObject("decor2Container");
        Transform D2transformComponent = decor2Container.transform;
        D2transformComponent.SetParent(sectorTransform);


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
        List<DoorSaveData> doorList = sectorData.doorSaveData;
        List<SpawnPointsSaveData> spawnPointList = sectorData.spawnPointsSaveData;
        List<MovingObjectSaveData> movingObjectList = sectorData.movingObjectsaveData;


        MusicID = sectorData.levelPreferences.MusicID;

        // Carregar dados do background
        string backgroundName = sectorData.levelPreferences.BackgroundName; // Substitua pelo nome da variável correta
        float backgroundOffset = sectorData.levelPreferences.BackgroundOffset; // Substitua pelo nome da variável correta
        string timeWeatherName = sectorData.levelPreferences.TimeWeather;
        LoadTimeWeather(timeWeatherName, sectorTransform);
        // Chame a função de atualização do background
        LoadBackground(backgroundName, backgroundOffset, sectorTransform);

        GridWidth = gridSizeData.currentGridWidth;
        GridHeight = gridSizeData.currentGridHeight;
        AdjustDeathZoneColliderSize(sectorTransform);
        if (sectorTransform == Sector1)
        {
            GridWidthCameraSector1 = gridSizeData.currentGridWidth;
            GridHeightCameraSector1 = gridSizeData.currentGridHeight;
        }
        else if (sectorTransform == Sector2)
        {
            GridWidthCameraSector2 = gridSizeData.currentGridWidth;
            GridHeightCameraSector2 = gridSizeData.currentGridHeight;
        }
        else if (sectorTransform == Sector3)
        {
            GridWidthCameraSector3 = gridSizeData.currentGridWidth;
            GridHeightCameraSector3 = gridSizeData.currentGridHeight;
        }
        else if (sectorTransform == Sector4)
        {
            GridWidthCameraSector4 = gridSizeData.currentGridWidth;
            GridHeightCameraSector4 = gridSizeData.currentGridHeight;
        }
        else if (sectorTransform == Sector5)
        {
            GridWidthCameraSector5 = gridSizeData.currentGridWidth;
            GridHeightCameraSector5 = gridSizeData.currentGridHeight;
        }
        LevelEditorCamera levelEditorCamera = FindObjectOfType<LevelEditorCamera>();
        if (levelEditorCamera != null)
        {
            levelEditorCamera.UpdateCameraBounds();
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
                if (decorData != null)
                {
                    decorObject.transform.localScale = decorData.scale;
                }
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
                if (decor2Data != null)
                {
                    decor2Object.transform.localScale = decor2Data.scale;
                }
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
        // Para objetos Portas
        foreach (DoorSaveData doorData in doorList)
        {
            // Encontrar o prefab do objeto Door
            GameObject doorObjectPrefab = null;

            foreach (ObjectsData.ObjectCategory category in ScriptableObjectData.categories)
            {
                foreach (ObjectsData.ObjectsInfo objectInfo in category.Objects)
                {
                    if (objectInfo.ObjectName == doorData.name)
                    {
                        doorObjectPrefab = objectInfo.prefab;
                        break;
                    }
                }

                if (doorObjectPrefab != null)
                    break;
            }

            if (doorObjectPrefab != null)
            {
                GameObject doorObject = Instantiate(doorObjectPrefab, doorData.position, Quaternion.identity);
                doorObject.transform.SetParent(GameObjectsContainer.transform);
                doorObject.tag = "Door";

                // Configurar o componente de script do prefab com os valores de doorData
                Door doorScript = doorObject.GetComponentInChildren<Door>();

                if (doorScript != null)
                {
                    doorScript.DoorID = doorData.DoorID;
                    doorScript.SecondDoorID = doorData.SecondDoorID;
                    doorScript.PositionPoint = doorData.PositionPointName;
                    doorScript.SectorName = doorData.SectorDoorName;
                    doorScript.WithKey = doorData.WithKey;
                    doorScript.toSector = doorData.ToSector;
                }
                else
                {
                    Debug.LogWarning("Door script not found on Door object: " + doorData.name);
                }
            }
            else
            {
                Debug.LogWarning("Prefab not found for Door object: " + doorData.name);
            }
        }

        foreach (SpawnPointsSaveData spawnPointData in spawnPointList)
        {
            // Crie um objeto vazio (Empty) e atribua o nameID
            GameObject spawnPointObject = new GameObject(spawnPointData.NameID);
            spawnPointObject.transform.position = spawnPointData.position;
            spawnPointObject.transform.SetParent(GameObjectsContainer.transform);

            // Adicione a tag "SpawnPoint" ao objeto
            spawnPointObject.tag = "SpawnPoint";

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
        //Carrega os GameObjects, mas se for PlayerPos carrega só a posição salva, para depois o player a obter e começar de lá.
        //Também pode ser útil se usar particulas para facilitar no level editor os seus prefabs devem ter imagem então isto poderá carregar só
        //particlas em vez com a imagem.
        foreach (GameObjectSaveData gameObjectData in gameObjectList)
        {
            if (gameObjectData.name == "PlayerPos" && sectorData.sectorName == "Sector1")
            {
                // Inicie uma Coroutine para esperar um tempo antes de instanciar o jogador.
                StartCoroutine(ToLoadPlayer(gameObjectData.position));
                // Se o GameObject salvo for o "PlayerPos", ajuste apenas a posição do objeto PlayerPrefab.
                //// Ao invés de ajustar a posição diretamente, vamos instanciar o PlayerPrefab na posição correta.
                //PlayerPrefab = Instantiate(PlayerPrefab, gameObjectData.position, Quaternion.identity);
                //if (CameraZoom.instance != null)
                //{
                //    CameraZoom.instance.Initialize();
                //}
            }
            else
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
                    GameObject gameObjectObject = Instantiate(gameObjectPrefab, gameObjectData.position, Quaternion.identity);
                    gameObjectObject.transform.SetParent(GameObjectsContainer.transform);
                }
                else
                {
                    Debug.LogWarning("Prefab not found for object: " + gameObjectData.name);
                }
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
                gameObjectObject.transform.localScale = triggerData.scale;
                if (triggerData.type == "Ladder")
                {
                    gameObjectObject.tag = "Ladder";
                }
                if (triggerData.type == "Water")
                {
                    gameObjectObject.tag = "Water";
                }
                if (triggerData.type == "Play Particles")
                {
                    gameObjectObject.tag = "Particles";
                    ParticlesController playParticlesScript = gameObjectObject.AddComponent<ParticlesController>();
                    playParticlesScript.isToPlay = true;
                    playParticlesScript.particleIDName = triggerData.customScript;
                    playParticlesScript.wasWaitTime = triggerData.wasWaitTime;
                    playParticlesScript.timeToPlay = triggerData.timeToPlay;
                }
                if (triggerData.type == "Stop Particles")
                {
                    gameObjectObject.tag = "Particles";
                    ParticlesController playParticlesScript = gameObjectObject.AddComponent<ParticlesController>();
                    playParticlesScript.isToPlay = false;
                    playParticlesScript.particleIDName = triggerData.customScript;
                    playParticlesScript.wasWaitTime = triggerData.wasWaitTime;
                    playParticlesScript.timeToPlay = triggerData.timeToPlay;
                }
            }
            else
            {
                Debug.LogWarning("Prefab not found for Trigger object: " + triggerData.name);
            }
        }
        foreach (ParticlesSaveData particleData in particlesList)
        {
            GameObject gameObjectPrefab = null;
            foreach (ParticleTypes.ParticleTypesCategory category in scriptableParticleTypesData.categories)
            {
                foreach (ParticleTypes.ParticleTypesInfo particleTypeInfo in category.ParticleTypes)
                {
                    if (particleTypeInfo.ParticleTypesName == particleData.particleType)
                    {
                        gameObjectPrefab = particleTypeInfo.prefab;
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

                // Verifique se o objeto tem um componente ParticleSystem
                ParticleSystem particleSystem = gameObjectObject.GetComponent<ParticleSystem>();

                if (particleSystem != null)
                {
                    if (particleData.initialStarted)
                    {
                        // Reproduza o sistema de partículas
                        particleSystem.Play();
                    }
                    else
                    {
                        // Pare o sistema de partículas
                        particleSystem.Stop();
                    }
                    var mainModule = particleSystem.main;
                    mainModule.loop = particleData.isLoop;
                }
                else
                {
                    Debug.LogWarning("ParticleSystem component not found on GameObject: " + particleData.particleType);
                }
            }
        }


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

    }

    private IEnumerator ToLoadPlayer(Vector3 playerPosition)
    {
        // Espere por um determinado período de tempo antes de instanciar o jogador.
        yield return new WaitForSeconds(5f); // Exemplo: espera por 2 segundos.

        PressStartInfo.SetActive(true);
        canStart = true;
        // Após o tempo de espera, instancie o jogador na posição especificada.
        PlayerPrefab = Instantiate(PlayerPrefab, playerPosition, Quaternion.identity);

    }
    private void LoadTimeWeather(string timeWeather, Transform sectorTransform)
    {
        GameObject timeWeatherLocal = new GameObject("TimeWeatherLocal");
        TimeWeatherLocal = timeWeatherLocal.transform;
        TimeWeatherLocal.SetParent(sectorTransform);

        volumeNameTimeWeather = timeWeather;
        foreach (Transform child in TimeWeatherLocal)
        {
            Destroy(child.gameObject);
        }
        GameObject TimeWeatherPrefab = null;
        foreach (TimeWeatherData.TimeWeatherCategory timeWeatherCategory in ScriptableTimeWeatherData.timeWeatherCategories)
        {
            foreach (TimeWeatherData.TimeWeather timeWeatherInfo in timeWeatherCategory.TimeWeatherList)
            {
                if (timeWeatherInfo.TimeWeatherName == volumeNameTimeWeather)
                {
                    TimeWeatherPrefab = timeWeatherInfo.TimeWeatherPrefab;
                    break;
                }
            }
            if (TimeWeatherPrefab != null)
                break;
        }
        if (TimeWeatherPrefab != null)
        {
            // Instancie o novo prefab de fundo no local com base no offset
            Vector3 spawnPosition = new Vector3(TimeWeatherLocal.position.x, TimeWeatherLocal.position.z);
            currentTimeWeatherInstance = Instantiate(TimeWeatherPrefab, spawnPosition, Quaternion.identity, TimeWeatherLocal);
        }
        else
        {
            Debug.LogWarning("Prefab not found for TimeWeather: " + volumeNameTimeWeather);
        }
    }
    private void LoadBackground(string backgroundName, float offset, Transform sectorTransform)
    {
        GameObject backgroundLocalObject = new GameObject("backgroundLocal");
        backgroundLocal = backgroundLocalObject.transform;
        backgroundLocal.SetParent(sectorTransform);

        selectedBackgroundName = backgroundName;

        foreach (Transform child in backgroundLocal)
        {
            Destroy(child.gameObject);
        }

        // Encontre o prefab do fundo correspondente com base no nome
        GameObject backgroundPrefab = null;
        foreach (BackgroundData.BiomeCategory biomeCategory in backgroundData.biomeCategories)
        {
            foreach (BackgroundData.Background backgroundInfo in biomeCategory.BackgroundList)
            {
                if (backgroundInfo.backgroundName == selectedBackgroundName)
                {
                    backgroundPrefab = backgroundInfo.backgroundPrefab;
                    break;
                }
            }
            if (backgroundPrefab != null)
                break;
        }

        if (backgroundPrefab != null)
        {
            // Instancie o novo prefab de fundo no local com base no offset
            Vector3 spawnPosition = new Vector3(backgroundLocal.position.x, offset, backgroundLocal.position.z);
            currentBackgroundInstance = Instantiate(backgroundPrefab, spawnPosition, Quaternion.identity, backgroundLocal);
        }
        else
        {
            Debug.LogWarning("Prefab not found for background: " + selectedBackgroundName);
        }

    }


    public void AdjustDeathZoneColliderSize(Transform sectorTransform)
    {
        GameObject DeathZone = Instantiate(DeathZonePrefab, new Vector3(0, -0.07f, 0), Quaternion.identity);
        DeathZone.transform.SetParent(sectorTransform);

        // Obtém a escala atual do GameObject
        Vector3 currentScale = DeathZone.transform.localScale;

        // Define a nova escala usando a largura do grid
        float newScaleX = GridWidth;

        // Mantém a escala no eixo Y e Z inalterada
        float newScaleY = currentScale.y;
        float newScaleZ = currentScale.z;

        // Cria um novo vetor de escala
        Vector3 newScale = new Vector3(newScaleX + 30f, newScaleY, newScaleZ);

        // Define a escala do GameObject
        DeathZone.transform.localScale = newScale;

        // Calcula a nova posição para centrar no eixo X
        float centerX = newScaleX / 2;

        // Obtém a posição atual do GameObject
        Vector3 currentPosition = DeathZone.transform.position;

        // Define a nova posição centrada no eixo X
        Vector3 newPosition = new Vector3(centerX, currentPosition.y, currentPosition.z);

        // Atribui a nova posição ao GameObject
        DeathZone.transform.position = newPosition;
    }

    // Função para reproduzir a música selecionada
    private void PlayMusic()
    {
        // Carregar o evento FMOD associado à música selecionada
        if (FMODEvents.instance != null && FMODEvents.instance.musicList.ContainsKey((FMODEvents.MusicID)MusicID))
        {
            EventReference musicEvent = FMODEvents.instance.musicList[(FMODEvents.MusicID)MusicID];


            // Criar uma nova instância do evento FMOD para a nova música
            musicEventInstance = RuntimeManager.CreateInstance(musicEvent);

            // Tocar a música a partir do início
            if (musicEventInstance.isValid())
            {
                musicEventInstance.start();
            }
        }
    }
    public void SpeedMusic()
    {
        musicEventInstance.setParameterByName("MusicVelocity", 1);
    }
    public void SpeedMusicNormal()
    {
        musicEventInstance.setParameterByName("MusicVelocity", 0);
    }
    public void StopMusic()
    {
        // Verificar se a instância do evento FMOD é válida e está tocando
        if (musicEventInstance.isValid())
        {
            PLAYBACK_STATE playbackState;
            musicEventInstance.getPlaybackState(out playbackState);

            // Verificar se a música está tocando antes de pará-la
            if (playbackState == PLAYBACK_STATE.PLAYING)
            {
                // Parar a instância do evento FMOD
                musicEventInstance.setParameterByName("MusicVelocity", 0);
                musicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }
    private void OnDestroy()
    {
        // Verificar se a instância do evento FMOD é válida e está tocando
        if (musicEventInstance.isValid())
        {
            PLAYBACK_STATE playbackState;
            musicEventInstance.getPlaybackState(out playbackState);

            // Verificar se a música está tocando antes de pará-la
            if (playbackState == PLAYBACK_STATE.PLAYING)
            {
                // Parar a instância do evento FMOD
                musicEventInstance.setParameterByName("MusicVelocity", 0);
                musicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }
}
