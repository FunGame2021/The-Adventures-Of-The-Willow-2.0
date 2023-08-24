using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class LoadPlayTestLevel : MonoBehaviour
{
    [SerializeField] private TMP_Text LevelName;
    [SerializeField] private TMP_Text AutorName;
    private string LevelNames;
    private string worldNames;

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

    #region Tilemap
    public Grid tilemapGrid; // Referência ao Grid onde os Tilemaps serão adicionados
    [HideInInspector] public List<Tilemap> tilemaps = new List<Tilemap>();
    #endregion

    [SerializeField] private List<TileCategoryData> tileCategories; // Lista de categorias de telhas

    private bool StartedLevel;
    [SerializeField] private GameObject LevelInfoPanel;
    void Start()
    {
        worldNames = LevelEditorController.instance.AtualWorld;
        LevelNames = LevelEditorController.instance.AtualLevel;
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
            }
        }

        if(StartedLevel)
        {
            LevelInfoPanel.SetActive(false);
        }
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
            List<ObjectSaveData> objectList = tilemapDataWrapper.objectSaveData;
            List<DecorSaveData> decorList = tilemapDataWrapper.decorSaveData;

            // Obtém a lista de TilemapData do TilemapDataWrapper
            List<TilemapData> tilemapDataList = tilemapDataWrapper.tilemapDataList;

            LevelName.text = tilemapDataWrapper.levelName;
            AutorName.text = tilemapDataWrapper.author;

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
                    GameObject objectObject = Instantiate(objectPrefab, objectData.position, Quaternion.identity);
                    objectObject.transform.SetParent(objectsContainer.transform);
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
}
