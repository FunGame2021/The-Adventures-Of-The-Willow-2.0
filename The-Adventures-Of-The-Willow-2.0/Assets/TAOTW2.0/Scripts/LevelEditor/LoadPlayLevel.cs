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
    public string IceTag;

    [SerializeField] private List<TileCategoryData> tileCategories; // Lista de categorias de telhas

    #endregion
    [SerializeField]public bool StartedLevel;
    private bool canStart;
    [SerializeField] private GameObject LevelInfoPanel;
    [SerializeField] private GameObject PressStartInfo;

    [SerializeField] private GameObject DeathZone;
    public bool isPlayingLevel;

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
        
        LoadLevel(worldNames, LevelNames, "Sector1");
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
                GameStates.instance.isLevelStarted = true;
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


    public void LoadLevel(string worldName, string level, string sectorName)
    {

        // Obt�m o caminho completo para a pasta do mundo
        string worldFolderPath = Path.Combine(Path.Combine(Application.persistentDataPath, "LevelEditor"), worldName);

        // Obt�m o caminho completo para o arquivo JSON com a extens�o ".TAOWLE" dentro da pasta do mundo
        string loadPath = Path.Combine(worldFolderPath, level + ".TAOWLE");

        Debug.Log("Tilemap data loaded from " + worldName + "/" + level + ".TAOWLE");


        // Verifica se o arquivo JSON existe
        if (File.Exists(loadPath))
        {
            // L� o conte�do do arquivo JSON
            string json = File.ReadAllText(loadPath);

            // Desserializa o JSON para um objeto LevelDataWrapper
            LevelDataWrapper levelDataWrapper = JsonUtility.FromJson<LevelDataWrapper>(json);
            if (levelDataWrapper != null)
            {
                LevelName.text = levelDataWrapper.levelName;
                AutorName.text = levelDataWrapper.author;

                gameLevelTime = levelDataWrapper.levelTime;

                // Procura o setor especificado pelo nome
                SectorData sectorData = levelDataWrapper.sectorData.Find(sector => sector.sectorName == sectorName);

                if (sectorData != null)
                {
                    // Agora voc� pode acessar os dados do setor especificado
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


                    MusicID = sectorData.levelPreferences.MusicID;


                    GridWidth = gridSizeData.currentGridWidth;
                    GridHeight = gridSizeData.currentGridHeight;
                    AdjustDeathZoneColliderSize();

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
                            else if (tilemapData.isSolid && tilemapData.isWallPlatform)
                            {
                                SetTilemapLayer(newTilemap, wallLayer);
                            }
                            else if (tilemapData.isSolid && tilemapData.isIce)
                            {
                                newTilemap.gameObject.tag = IceTag;
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
                            }
                            if (triggerData.type == "Stop Particles")
                            {
                                gameObjectObject.tag = "Particles";
                                ParticlesController playParticlesScript = gameObjectObject.AddComponent<ParticlesController>();
                                playParticlesScript.isToPlay = false;
                                playParticlesScript.particleIDName = triggerData.customScript;
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
                                if (gameObjectInfo.GameObjectName == particleData.nameID)
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

                            // Verifique se o objeto tem um componente ParticleSystem
                            ParticleSystem particleSystem = gameObjectObject.GetComponent<ParticleSystem>();

                            if (particleSystem != null)
                            {
                                if (particleData.initialStarted)
                                {
                                    // Reproduza o sistema de part�culas
                                    particleSystem.Play();
                                }
                                else
                                {
                                    // Pare o sistema de part�culas
                                    particleSystem.Stop();
                                }
                                var mainModule = particleSystem.main;
                                mainModule.loop = particleData.isLoop;
                            }
                            else
                            {
                                Debug.LogWarning("ParticleSystem component not found on GameObject: " + particleData.nameID);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Save file not found: " + loadPath);
                }
            }
        }
    }
    
    public void LoadSector(string SectorNameToLoad)
    {

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
}
