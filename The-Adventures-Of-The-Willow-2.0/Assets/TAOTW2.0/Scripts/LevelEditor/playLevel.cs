using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using FMODUnity;
using FMOD.Studio;
using System.Collections;

public class playLevel : MonoBehaviour
{
    public static playLevel instance;


    [HideInInspector] public string levelEditorPath; // Caminho para a pasta "LevelEditor"
    [HideInInspector] public string levelEditorExtraPath; // Caminho para a pasta "LevelEditor"
    [HideInInspector] public string levelEditorGamePath; // Caminho para a pasta "LevelEditor"

    [SerializeField] private TMP_Text LevelName;
    [SerializeField] private TMP_Text AutorName;
    public string LevelNames;
    public string worldNames;
    #region Load Level
    public int MusicID;
    private FMOD.Studio.EventInstance musicEventInstance;
    #region Manager
    public int gameLevelTime;
    #endregion
    #region Decor
    public DecorData ScriptableDecorData;
    public Decor2Data ScriptableDecor2Data;
    public Transform decorContainer;
    public Transform decor2Container;
    #endregion

    #region Objects
    public ObjectsData ScriptableObjectData;
    public Transform objectsContainer;
    #endregion

    #region Enemy
    public EnemyData ScriptableEnemyData;
    public Transform enemyContainer;
    #endregion

    #region GameObject
    public GameObjectsData ScriptableGameObjectData;
    public Transform GameObjectsContainer;
    #endregion
    #region Player
    public GameObject PlayerPrefab;
    public Transform PlayerPos;
    #endregion

    #region Tilemap
    public int GridWidth;
    public int GridHeight;

    public Grid tilemapGrid; // Refer�ncia ao Grid onde os Tilemaps ser�o adicionados
    [HideInInspector] public List<Tilemap> tilemaps = new List<Tilemap>();


    public LayerMask wallLayer;
    public LayerMask groundLayer;
    public LayerMask defaultLayer;

    [SerializeField] private List<TileCategoryData> tileCategories; // Lista de categorias de telhas

    #endregion
    [SerializeField] public bool StartedLevel;
    private bool canStart;
    [SerializeField] private GameObject LevelInfoPanel;
    [SerializeField] private GameObject PressStartInfo;

    [SerializeField] private GameObject DeathZone;
    public bool isPlayingLevel;
    #endregion

    #region Load World
    [SerializeField] private GameObject WorldInfoPanel;
    private bool isPlayingWorld;
    private bool StartedWorld;

    private float currentGridWidth;
    private float currentGridHeight;
    [SerializeField] private GameObject levelDotPrefab;
    private int WorldMusicID;
    #endregion

    private bool isWorld;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        PressStartInfo.SetActive(false);
        canStart = false;
        isPlayingLevel = false;

        isWorld = PlayWorld.instance.isWorldmap;

        // Obt�m o caminho completo para a pasta "LevelEditor"
        levelEditorPath = Path.Combine(Application.persistentDataPath, "LevelEditor");
        levelEditorExtraPath = Path.Combine(Application.streamingAssetsPath, "Worlds/ExtraWorlds");
        levelEditorGamePath = Path.Combine(Application.streamingAssetsPath, "Worlds/GameWorlds");
    }
    void Start()
    {
        if (!isWorld)
        {
            //se for do Level fa�a
            if (PlayWorld.instance != null)
            {
                worldNames = PlayWorld.instance.selectedWorldName;
                LevelNames = PlayWorld.instance.selectedLevelName;
            }

            LoadLevel(worldNames, LevelNames);
            StartedLevel = false;
            LevelInfoPanel.SetActive(true);
        }
        else // ent�o carrega o mundo
        {
            //se for do Level fa�a
            if (PlayWorld.instance != null)
            {
                worldNames = PlayWorld.instance.selectedWorldName;
                LevelNames = PlayWorld.instance.selectedLevelName;
            }
            LoadWorld(worldNames);
            StartedWorld = false;
            WorldInfoPanel.SetActive(true);
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

        return null; // Retorna null se o tile n�o for encontrado
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
            }
            if (!StartedWorld && canStart)
            {
                isPlayingWorld = true;
                StartedWorld = true;
            }
        }

        if (StartedLevel)
        {
            LevelInfoPanel.SetActive(false);
        }
        if(StartedWorld)
        {
            WorldInfoPanel.SetActive(false);
        }
    }

    #region Load Level
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
        string worldFolderPath;

        if (PlayWorld.instance.isExtraLevels)
        {
            worldFolderPath = Path.Combine(levelEditorExtraPath, worldName);
        }
        else if (PlayWorld.instance.isGameLevels)
        {
            worldFolderPath = Path.Combine(levelEditorGamePath, worldName);
        }
        else
        {
            worldFolderPath = Path.Combine(levelEditorPath, worldName);
        }

        // Obt�m o caminho completo para o arquivo JSON com a extens�o ".TAOWLE" dentro da pasta do mundo
        string loadPath = Path.Combine(worldFolderPath, level + ".TAOWLE");


        // Verifica se o arquivo JSON existe
        if (File.Exists(loadPath))
        {
            // L� o conte�do do arquivo JSON
            string json = File.ReadAllText(loadPath);

            // Converte o JSON de volta para o objeto TilemapDataWrapper
            TilemapDataWrapper tilemapDataWrapper = JsonUtility.FromJson<TilemapDataWrapper>(json);

            // Obt�m o objeto GridSizeData do TilemapDataWrapper
            GridSizeData gridSizeData = tilemapDataWrapper.gridSizeData;

            // Obt�m a lista de EnemySaveData do TilemapDataWrapper
            List<EnemySaveData> enemyList = tilemapDataWrapper.enemySaveData;
            List<GameObjectSaveData> gameObjectList = tilemapDataWrapper.gameObjectSaveData;
            List<ObjectSaveData> objectList = tilemapDataWrapper.objectSaveData;
            List<DecorSaveData> decorList = tilemapDataWrapper.decorSaveData;
            List<Decor2SaveData> decor2List = tilemapDataWrapper.decor2SaveData;

            // Obt�m a lista de TilemapData do TilemapDataWrapper
            List<TilemapData> tilemapDataList = tilemapDataWrapper.tilemapDataList;

            LevelName.text = tilemapDataWrapper.levelName;
            AutorName.text = tilemapDataWrapper.author;

            GridWidth = gridSizeData.currentGridWidth;
            GridHeight = gridSizeData.currentGridHeight;
            AdjustDeathZoneColliderSize();

            MusicID = tilemapDataWrapper.levelPreferences.MusicID;
            gameLevelTime = tilemapDataWrapper.levelPreferences.levelTime;

            // Percorre os TilemapData da lista
            foreach (TilemapData tilemapData in tilemapDataList)
            {
                // Cria um novo Tilemap no Grid da cena
                GameObject newTilemapObject = new GameObject(tilemapData.tilemapName);
                newTilemapObject.transform.SetParent(tilemapGrid.transform, false);

                Tilemap newTilemap = newTilemapObject.AddComponent<Tilemap>();
                //newTilemapObject.AddComponent<TilemapRenderer>();
                TilemapRenderer newTilemapRenderer = newTilemapObject.AddComponent<TilemapRenderer>();


                // Define a posi��o do novo Tilemap
                newTilemapObject.transform.position = new Vector3Int(0, 0, Mathf.RoundToInt(tilemapData.zPos));

                newTilemapRenderer.sortingOrder = tilemapData.shortLayerPos;

                // Adiciona o Tilemap � lista
                tilemaps.Add(newTilemap);


                // Configura a propriedade "isSolid" do Tilemap
                if (tilemapData.isSolid)
                {
                    // Adiciona um Collider2D ao Tilemap se ainda n�o existir
                    if (newTilemap.GetComponent<TilemapCollider2D>() == null)
                    {
                        newTilemap.gameObject.AddComponent<TilemapCollider2D>();
                    }

                    // Obt�m o componente TilemapCollider2D do Tilemap
                    TilemapCollider2D tilemapCollider2D = newTilemap.GetComponent<TilemapCollider2D>();

                    // Verifica se o TilemapCollider2D existe
                    if (tilemapCollider2D != null)
                    {
                        // Ativa o "Used by Composite" no TilemapCollider2D
                        tilemapCollider2D.usedByComposite = true;
                    }


                    // Adiciona um CompositeCollider2D ao Tilemap se ainda n�o existir
                    if (newTilemap.GetComponent<CompositeCollider2D>() == null)
                    {
                        newTilemap.gameObject.AddComponent<CompositeCollider2D>();
                    }

                    // Obt�m o componente CompositeCollider2D do Tilemap
                    CompositeCollider2D compositeCollider2D = newTilemap.GetComponent<CompositeCollider2D>();

                    // Verifica se o CompositeCollider2D existe
                    if (compositeCollider2D != null)
                    {
                        // Obt�m o Rigidbody2D associado ao CompositeCollider2D
                        Rigidbody2D rb = compositeCollider2D.attachedRigidbody;

                        // Verifica se o Rigidbody2D existe
                        if (rb != null)
                        {
                            // Define o tipo de corpo como est�tico
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
                        // Define a telha no Tilemap na posi��o cellPos
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
                    // Crie um objeto do inimigo e defina o nome e a posi��o
                    GameObject enemyObject = Instantiate(enemyPrefab, enemyData.position, Quaternion.identity);
                    enemyObject.transform.SetParent(enemyContainer.transform);
                }
                else
                {
                    Debug.LogWarning("Prefab not found for enemy: " + enemyData.name);
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
                    // Crie um novo objeto com base no prefab e defina o nome e a posi��o
                    GameObject objectObject = Instantiate(objectPrefab, objectData.position, Quaternion.identity);
                    objectObject.transform.SetParent(objectsContainer.transform);

                    ObjectType objectType = objectData.objectType;

                    // Restaure os n�s de movimento e o tempo de transi��o para objetos com componente PlatformMovement
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
                                Transform nodeTransform = new GameObject("Node " + i).transform;
                                nodeTransform.position = objectData.node[i].position;
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

            //Carrega os GameObjects, mas se for PlayerPos carrega s� a posi��o salva, para depois o player a obter e come�ar de l�.
            //Tamb�m pode ser �til se usar particulas para facilitar no level editor os seus prefabs devem ter imagem ent�o isto poder� carregar s�
            //particlas em vez com a imagem.
            foreach (GameObjectSaveData gameObjectData in gameObjectList)
            {
                if (gameObjectData.name == "PlayerPos")
                {
                    // Inicie uma Coroutine para esperar um tempo antes de instanciar o jogador.
                    StartCoroutine(ToLoadPlayer(gameObjectData.position));
                    // Se o GameObject salvo for o "PlayerPos", ajuste apenas a posi��o do objeto PlayerPrefab.
                    //// Ao inv�s de ajustar a posi��o diretamente, vamos instanciar o PlayerPrefab na posi��o correta.
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

        }
        else
        {
            Debug.LogWarning("Save file not found: " + loadPath);
        }
    }

    private IEnumerator ToLoadPlayer(Vector3 playerPosition)
    {
        // Espere por um determinado per�odo de tempo antes de instanciar o jogador.
        yield return new WaitForSeconds(5f); // Exemplo: espera por 2 segundos.

        PressStartInfo.SetActive(true);
        canStart = true;
        // Ap�s o tempo de espera, instancie o jogador na posi��o especificada.
        PlayerPrefab = Instantiate(PlayerPrefab, playerPosition, Quaternion.identity);

    }

    public void AdjustDeathZoneColliderSize()
    {
        // Obt�m a escala atual do GameObject
        Vector3 currentScale = DeathZone.transform.localScale;

        // Define a nova escala usando a largura do grid
        float newScaleX = GridWidth;

        // Mant�m a escala no eixo Y e Z inalterada
        float newScaleY = currentScale.y;
        float newScaleZ = currentScale.z;

        // Cria um novo vetor de escala
        Vector3 newScale = new Vector3(newScaleX + 30f, newScaleY, newScaleZ);

        // Define a escala do GameObject
        DeathZone.transform.localScale = newScale;

        // Calcula a nova posi��o para centrar no eixo X
        float centerX = newScaleX / 2;

        // Obt�m a posi��o atual do GameObject
        Vector3 currentPosition = DeathZone.transform.position;

        // Define a nova posi��o centrada no eixo X
        Vector3 newPosition = new Vector3(centerX, currentPosition.y, currentPosition.z);

        // Atribui a nova posi��o ao GameObject
        DeathZone.transform.position = newPosition;
    }



    // Fun��o para reproduzir a m�sica selecionada
    private void PlayMusic()
    {
        // Carregar o evento FMOD associado � m�sica selecionada
        if (FMODEvents.instance != null && FMODEvents.instance.musicList.ContainsKey((FMODEvents.MusicID)MusicID))
        {
            EventReference musicEvent = FMODEvents.instance.musicList[(FMODEvents.MusicID)MusicID];


            // Criar uma nova inst�ncia do evento FMOD para a nova m�sica
            musicEventInstance = RuntimeManager.CreateInstance(musicEvent);

            // Tocar a m�sica a partir do in�cio
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
        // Verificar se a inst�ncia do evento FMOD � v�lida e est� tocando
        if (musicEventInstance.isValid())
        {
            PLAYBACK_STATE playbackState;
            musicEventInstance.getPlaybackState(out playbackState);

            // Verificar se a m�sica est� tocando antes de par�-la
            if (playbackState == PLAYBACK_STATE.PLAYING)
            {
                // Parar a inst�ncia do evento FMOD
                musicEventInstance.setParameterByName("MusicVelocity", 0);
                musicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }
    private void OnDestroy()
    {
        // Verificar se a inst�ncia do evento FMOD � v�lida e est� tocando
        if (musicEventInstance.isValid())
        {
            PLAYBACK_STATE playbackState;
            musicEventInstance.getPlaybackState(out playbackState);

            // Verificar se a m�sica est� tocando antes de par�-la
            if (playbackState == PLAYBACK_STATE.PLAYING)
            {
                // Parar a inst�ncia do evento FMOD
                musicEventInstance.setParameterByName("MusicVelocity", 0);
                musicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }
    #endregion

    #region Load World
    public void LoadWorld(string worldName)
    {
        string worldFolderPath;

        if (PlayWorld.instance.isExtraLevels)
        {
            worldFolderPath = Path.Combine(levelEditorExtraPath, worldName);
        }
        else if (PlayWorld.instance.isGameLevels)
        {
            worldFolderPath = Path.Combine(levelEditorGamePath, worldName);
        }
        else
        {
            worldFolderPath = Path.Combine(levelEditorPath, worldName);
        }
        // Obt�m o caminho completo para o arquivo JSON "World.TAOWWE" dentro da pasta do mundo
        string loadPath = Path.Combine(worldFolderPath, "World.TAOWWE");


        // Verifica se o arquivo JSON existe
        if (File.Exists(loadPath))
        {
            TileButton.instance.UpdateWorldTileButtons();
            // L� o conte�do do arquivo JSON
            string json = File.ReadAllText(loadPath);
            //Verifica se o arquivo n�o est� vazio
            if (!string.IsNullOrEmpty(json))
            {
                // Converte o JSON de volta para o objeto TilemapDataWrapper
                TilemapDataWrapper tilemapDataWrapper = JsonUtility.FromJson<TilemapDataWrapper>(json);

                // Obt�m o objeto GridSizeData do TilemapDataWrapper
                GridSizeData gridSizeData = tilemapDataWrapper.gridSizeData;

                List<GameObjectSaveData> gameObjectList = tilemapDataWrapper.gameObjectSaveData;
                List<LevelDotData> levelDotDataList = tilemapDataWrapper.levelDotDataList;


                // Obt�m a lista de TilemapData do TilemapDataWrappere
                List<TilemapData> tilemapDataList = tilemapDataWrapper.tilemapDataList;



                WorldMusicID = tilemapDataWrapper.levelPreferences.MusicID;
                // Restaura o tamanho do grid
                currentGridWidth = gridSizeData.currentGridWidth;
                currentGridHeight = gridSizeData.currentGridHeight;


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
                        // Crie um objeto do inimigo e defina o nome e a posi��o
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

                    // Atribua as informa��es carregadas ao novo objeto LevelDot
                    if (newLevelDot != null)
                    {
                        newLevelDot.SetLevelPath(dotData.levelPath);
                        // Voc� pode configurar outros dados do LevelDot aqui, se necess�rio
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


                    // Define a posi��o do novo Tilemap
                    newTilemapObject.transform.position = new Vector3Int(0, 0, Mathf.RoundToInt(tilemapData.zPos));

                    newTilemapRenderer.sortingOrder = tilemapData.shortLayerPos;

                    // Adiciona o Tilemap � lista
                    tilemaps.Add(newTilemap);

                    // Configura a propriedade "isSolid" do Tilemap
                    if (tilemapData.isSolid)
                    {
                        // Adiciona um Collider2D ao Tilemap se ainda n�o existir
                        if (newTilemap.GetComponent<TilemapCollider2D>() == null)
                        {
                            newTilemap.gameObject.AddComponent<TilemapCollider2D>();
                        }

                        // Obt�m o componente TilemapCollider2D do Tilemap
                        TilemapCollider2D tilemapCollider2D = newTilemap.GetComponent<TilemapCollider2D>();

                        // Verifica se o TilemapCollider2D existe
                        if (tilemapCollider2D != null)
                        {
                            // Ativa o "Used by Composite" no TilemapCollider2D
                            tilemapCollider2D.usedByComposite = true;
                        }

                        // Adiciona um CompositeCollider2D ao Tilemap se ainda n�o existir
                        if (newTilemap.GetComponent<CompositeCollider2D>() == null)
                        {
                            newTilemap.gameObject.AddComponent<CompositeCollider2D>();
                        }

                        // Obt�m o componente CompositeCollider2D do Tilemap
                        CompositeCollider2D compositeCollider2D = newTilemap.GetComponent<CompositeCollider2D>();

                        // Verifica se o CompositeCollider2D existe
                        if (compositeCollider2D != null)
                        {
                            // Obt�m o Rigidbody2D associado ao CompositeCollider2D
                            Rigidbody2D rb = compositeCollider2D.attachedRigidbody;

                            // Verifica se o Rigidbody2D existe
                            if (rb != null)
                            {
                                // Define o tipo de corpo como est�tico
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
                            // Define a telha no Tilemap na posi��o cellPos
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

                // Crie um objeto TilemapDataWrapper com as informa��es padr�o

                TilemapDataWrapper defaultWorldData = new TilemapDataWrapper();
                defaultWorldData.gridSizeData = new GridSizeData();
                defaultWorldData.gridSizeData.currentGridWidth = 20; // Tamanho de exemplo
                defaultWorldData.gridSizeData.currentGridHeight = 20; // Tamanho de exemplo
                defaultWorldData.tilemapDataList = new List<TilemapData>() { emptyTilemapData }; // Lista vazia
                defaultWorldData.gameObjectSaveData = new List<GameObjectSaveData>(); // Lista vazia

                // Salva o JSON com as informa��es padr�o no arquivo
                string defaultJson = JsonUtility.ToJson(defaultWorldData);
                File.WriteAllText(loadPath, defaultJson);

                // Carrega o mundo rec�m-criado
                //LoadSelectedWorld(defaultWorldData);
            }
        }
        else
        {
            Debug.LogWarning("Save file not found: " + loadPath);
        }
    }
    #endregion
}