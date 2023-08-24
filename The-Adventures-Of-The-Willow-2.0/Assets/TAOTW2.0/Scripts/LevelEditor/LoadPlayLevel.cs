using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using FMODUnity;
using FMOD.Studio;

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
    private int gameLevelTime;
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

    public Grid tilemapGrid; // Referência ao Grid onde os Tilemaps serão adicionados
    [HideInInspector] public List<Tilemap> tilemaps = new List<Tilemap>();


    public LayerMask wallLayer;
    public LayerMask groundLayer;
    public LayerMask defaultLayer;

    [SerializeField] private List<TileCategoryData> tileCategories; // Lista de categorias de telhas

    #endregion
    private bool StartedLevel;
    [SerializeField] private GameObject LevelInfoPanel;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    void Start()
    {
        //se for do editor faça
        if(LevelEditorController.instance != null)
        {
            worldNames = LevelEditorController.instance.AtualWorld;
            LevelNames = LevelEditorController.instance.AtualLevel;
        }
        else // então se for jogo normal faça
        {
            //worldNames = LoadLevel.instance.AtualWorld;
            //LevelNames = LevelEditorController.instance.AtualLevel;
        }
        
        LoadLevel(worldNames, LevelNames);
        StartedLevel = false;
        LevelInfoPanel.SetActive(true);

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

    void Update()
    {
        if (UserInput.instance.playerMoveAndExtraActions.PlayerActions.Jump.WasPressedThisFrame())
        {
            if (!StartedLevel)
            {
                StartedLevel = true;
                CameraZoom.instance.Initialize();
                PlayMusic();
                LevelTimeManager.instance.Being(gameLevelTime);
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

            // Obtém a lista de TilemapData do TilemapDataWrapper
            List<TilemapData> tilemapDataList = tilemapDataWrapper.tilemapDataList;

            LevelName.text = tilemapDataWrapper.levelName;
            AutorName.text = tilemapDataWrapper.author;

            GridWidth = gridSizeData.currentGridWidth;
            GridHeight = gridSizeData.currentGridHeight;


            MusicID = tilemapDataWrapper.levelPreferences.MusicID;
            gameLevelTime = tilemapDataWrapper.levelPreferences.levelTime;

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
            //Carrega os GameObjects, mas se for PlayerPos carrega só a posição salva, para depois o player a obter e começar de lá.
            //Também pode ser útil se usar particulas para facilitar no level editor os seus prefabs devem ter imagem então isto poderá carregar só
            //particlas em vez com a imagem.
            foreach (GameObjectSaveData gameObjectData in gameObjectList)
            {
                if (gameObjectData.name == "PlayerPos")
                {
                    // Se o GameObject salvo for o "PlayerPos", ajuste apenas a posição do objeto PlayerPrefab.
                    // Ao invés de ajustar a posição diretamente, vamos instanciar o PlayerPrefab na posição correta.
                    PlayerPrefab = Instantiate(PlayerPrefab, gameObjectData.position, Quaternion.identity);
                    if (CameraZoom.instance != null)
                    {
                        CameraZoom.instance.Initialize();
                    }
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
            Debug.LogWarning("Save file not found: " + loadPath);
        }
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
