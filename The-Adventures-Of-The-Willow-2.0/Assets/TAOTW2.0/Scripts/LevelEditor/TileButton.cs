using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.InputSystem;

public class TileButton : MonoBehaviour
{
    public static TileButton instance;

    public List<TileCategoryData> tileCategories; // Lista de categorias de telhas
    public List<TileCategoryData> worldTileCategories; // Lista de categorias de telhas

    public GameObject categoryButtonPrefab; // Prefab do bot�o da categoria
    public Transform categoryContainer; // Transform do objeto que conter� os bot�es de categoria
    public Transform worldCategoryContainer; // Transform do objeto que conter� os bot�es de categoria
    public TMP_Dropdown categoryDropdown; // Dropdown para selecionar a categoria
    public TMP_Dropdown worldCategoryDropdown; // Dropdown para selecionar a categoria
    public Button fillButton; // Bot�o para ativar/desativar o preenchimento
    public Image fillButtonImage; // Imagem do bot�o de preenchimento
    public Color fillEnabledColor; // Cor quando o preenchimento est� ativado
    public Color fillDisabledColor; // Cor quando o preenchimento est� desativado

    private List<TMP_Dropdown.OptionData> categoryOptions; // Op��es do Dropdown
    private List<TMP_Dropdown.OptionData> worldCategoryOptions; // Op��es do Dropdown
    private List<Button> tileButtons; // Lista de bot�es de sele��o de telha
    [HideInInspector] public TileBase selectedTile; // Telha atualmente selecionada

    private bool isMouseHeld = false; // Indica se o bot�o do mouse est� pressionado
    public EraserTool eraserTool;

    private bool isFillEnabled = false; // Indica se o preenchimento est� ativado
    private Queue<Vector3Int> fillQueue = new Queue<Vector3Int>(); // Fila de preenchimento

    private bool isObjectSelected = false;

    [SerializeField] private GameObject TilesPanel;
    [SerializeField] private GameObject WorldTilesPanel;
    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        tileButtons = new List<Button>();
        categoryOptions = new List<TMP_Dropdown.OptionData>();
        worldCategoryOptions = new List<TMP_Dropdown.OptionData>();


        CreateWorldCategoryButtons(); // Crie bot�es para categorias do mundo
        CreateWorldTileButtons(worldTileCategories[0]); // Crie bot�es para worldTileCategories
        CreateCategoryButtons(); // Crie bot�es para categorias normais
        CreateTileButtons(tileCategories[0]); // Crie bot�es para tileCategories

        fillButton.onClick.AddListener(ToggleFill);
        UpdateFillButtonColor();
    }
    public void UpdateTileButtons()
    {

        TilesPanel.SetActive(true);
        WorldTilesPanel.SetActive(false);

        CreateCategoryButtons(); // Crie bot�es para categorias normais
        CreateTileButtons(tileCategories[0]); // Crie bot�es para tileCategories

    }

    public void UpdateWorldTileButtons()
    {
        TilesPanel.SetActive(false);
        WorldTilesPanel.SetActive(true);

        CreateWorldCategoryButtons(); // Crie bot�es para categorias do mundo
        CreateWorldTileButtons(worldTileCategories[0]); // Crie bot�es para worldTileCategories
    }
    public void ClearSelectedTile()
    {
        selectedTile = null;
    }

    private void CreateCategoryButtons()
    {
        // Cria as op��es do Dropdown
        categoryOptions.Clear();

        // Para cada categoria, cria uma op��o no Dropdown
        for (int i = 0; i < tileCategories.Count; i++)
        {
            TileCategoryData category = tileCategories[i];

            // Cria a op��o do Dropdown para a categoria atual
            TMP_Dropdown.OptionData categoryOption = new TMP_Dropdown.OptionData(category.categoryName);
            categoryOptions.Add(categoryOption);
        }

        // Configura as op��es do Dropdown
        categoryDropdown.ClearOptions();
        categoryDropdown.AddOptions(categoryOptions);
        categoryDropdown.onValueChanged.AddListener(SelectCategory);

        // Seleciona a primeira categoria por padr�o
        SelectCategory(0);
    }

    private void CreateTileButtons(TileCategoryData category)
    {
        // Remove os bot�es de sele��o de telha anteriores
        foreach (Button tileButton in tileButtons)
        {
            Destroy(tileButton.gameObject);
        }

        tileButtons.Clear();

        // Para cada tipo de telha na categoria, cria um bot�o de sele��o
        for (int i = 0; i < category.tiles.Length; i++)
        {
            TileBase tile = category.tiles[i];
            Button tileButton = Instantiate(categoryButtonPrefab, categoryContainer).GetComponent<Button>();

            // Configura o bot�o e atribui a telha correspondente
            Sprite tileSprite = GetTileSprite(tile);
            tileButton.image.sprite = tileSprite;
            tileButton.onClick.AddListener(() => SelectTile(tile));

            tileButtons.Add(tileButton);
        }
    }

    private void CreateWorldCategoryButtons()
    {
        // Cria as op��es do Dropdown
        worldCategoryOptions.Clear();

        // Para cada categoria, cria uma op��o no Dropdown
        for (int i = 0; i < worldTileCategories.Count; i++)
        {
            TileCategoryData category = worldTileCategories[i];

            // Cria a op��o do Dropdown para a categoria atual
            TMP_Dropdown.OptionData categoryOption = new TMP_Dropdown.OptionData(category.categoryName);
            worldCategoryOptions.Add(categoryOption);
        }

        // Configura as op��es do Dropdown
        worldCategoryDropdown.ClearOptions();
        worldCategoryDropdown.AddOptions(worldCategoryOptions);
        worldCategoryDropdown.onValueChanged.AddListener(SelectWorldCategory);

        // Seleciona a primeira categoria por padr�o
        SelectWorldCategory(0);
    }

    private void CreateWorldTileButtons(TileCategoryData category)
    {
        // Remove os bot�es de sele��o de telha anteriores
        foreach (Button tileButton in tileButtons)
        {
            Destroy(tileButton.gameObject);
        }

        tileButtons.Clear();

        // Para cada tipo de telha na categoria, cria um bot�o de sele��o
        for (int i = 0; i < category.tiles.Length; i++)
        {
            TileBase tile = category.tiles[i];
            Button tileButton = Instantiate(categoryButtonPrefab, worldCategoryContainer).GetComponent<Button>();

            // Configura o bot�o e atribui a telha correspondente
            Sprite tileSprite = GetTileSprite(tile);
            tileButton.image.sprite = tileSprite;
            tileButton.onClick.AddListener(() => SelectTile(tile));

            tileButtons.Add(tileButton);
        }
    }
    private Sprite GetTileSprite(TileBase tile)
    {
        if (tile is Tile tileObject)
        {
            return tileObject.sprite;
        }

        return null;
    }

    private void SelectTile(TileBase tile)
    {
        selectedTile = tile;
        // Verifica se a telha selecionada � a telha de borracha
        if (selectedTile == null)
        {
            Debug.Log("Selected Tool: Eraser");
        }
        else
        {
            Debug.Log("Selected Tile: " + selectedTile.name);
        }
    }

    private void SelectCategory(int categoryIndex)
    {
        TileCategoryData category = tileCategories[categoryIndex];

        // Atualiza os bot�es de sele��o de telha com base na categoria selecionada
        CreateTileButtons(category);
    }
    private void SelectWorldCategory(int categoryIndex)
    {
        TileCategoryData category = worldTileCategories[categoryIndex];

        // Atualiza os bot�es de sele��o de telha com base na categoria selecionada
        CreateWorldTileButtons(category);
    }
    private void Update()
    {

        if (isObjectSelected)
        {
            return; // N�o adicionar telha se um objeto estiver selecionado
        }

        // Verifica se o mouse est� sobre um elemento de UI
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (!eraserTool.isActiveEraserEnemy && !eraserTool.isActiveEraserTile && !TileSelectionTool.instance.isActive
            && !LevelEditorManager.instance.isActiveSelectPoint)
        {
            if (selectedTile != null && Mouse.current.leftButton.isPressed)
            {
                isMouseHeld = true; // Marca o bot�o do mouse como pressionado

                Vector3 mouseWorldPos = LevelEditorManager.instance.mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                Vector3Int cellPos = LevelEditorManager.instance.selectedTilemap.WorldToCell(mouseWorldPos);

                if (isFillEnabled)
                {
                    // Inicia o preenchimento
                    TileBase startTile = LevelEditorManager.instance.selectedTilemap.GetTile(cellPos);
                    FillTiles(cellPos, startTile);
                }
                else
                {
                    // Verifica se a posi��o da c�lula est� dentro dos limites da grelha
                    if (IsCellWithinGridBounds(cellPos))
                    {
                        // Define a telha selecionada na posi��o do mouse
                        LevelEditorManager.instance.selectedTilemap.SetTile(cellPos, selectedTile);
                    }
                }
            }
            else if (selectedTile != null && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                isMouseHeld = false; // Marca o bot�o do mouse como solto
            }

            // Verifica se o bot�o do mouse est� pressionado continuamente
            if (selectedTile != null && isMouseHeld && Mouse.current.leftButton.isPressed)
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                Vector3Int cellPos = LevelEditorManager.instance.selectedTilemap.WorldToCell(mouseWorldPos);

                if (isFillEnabled)
                {
                    // Inicia o preenchimento
                    TileBase startTile = LevelEditorManager.instance.selectedTilemap.GetTile(cellPos);
                    FillTiles(cellPos, startTile);
                }
                else
                {
                    // Verifica se a posi��o da c�lula est� dentro dos limites da grelha
                    if (IsCellWithinGridBounds(cellPos))
                    {
                        // Define a telha selecionada na posi��o do mouse
                        LevelEditorManager.instance.selectedTilemap.SetTile(cellPos, selectedTile);
                    }
                }
            }
        }
    }


    private void FillTiles(Vector3Int startCellPos, TileBase startTile)
    {
        // Limpa a fila de preenchimento
        fillQueue.Clear();

        // Adiciona a posi��o inicial � fila de preenchimento
        fillQueue.Enqueue(startCellPos);

        // Obt�m os limites do tilemap
        Vector3Int minPos = LevelEditorManager.instance.selectedTilemap.cellBounds.min;
        Vector3Int maxPos = LevelEditorManager.instance.selectedTilemap.cellBounds.max;

        // Cria um conjunto para armazenar as posi��es j� preenchidas
        HashSet<Vector3Int> filledPositions = new HashSet<Vector3Int>();

        // Loop enquanto a fila de preenchimento n�o estiver vazia
        while (fillQueue.Count > 0)
        {
            Vector3Int cellPos = fillQueue.Dequeue();

            // Verifica se a posi��o j� foi preenchida
            if (filledPositions.Contains(cellPos))
            {
                continue;
            }

            // Verifica se a posi��o est� dentro dos limites do tilemap
            if (cellPos.x < minPos.x || cellPos.y < minPos.y || cellPos.x >= maxPos.x || cellPos.y >= maxPos.y)
            {
                continue;
            }

            // Obt�m a telha na posi��o atual
            TileBase tile = LevelEditorManager.instance.selectedTilemap.GetTile(cellPos);

            // Verifica se a telha � igual � telha inicial
            if (tile == startTile)
            {
                // Define a telha selecionada na posi��o atual
                LevelEditorManager.instance.selectedTilemap.SetTile(cellPos, selectedTile);

                // Adiciona as posi��es vizinhas � fila de preenchimento
                fillQueue.Enqueue(cellPos + Vector3Int.up);
                fillQueue.Enqueue(cellPos + Vector3Int.down);
                fillQueue.Enqueue(cellPos + Vector3Int.left);
                fillQueue.Enqueue(cellPos + Vector3Int.right);
            }

            // Adiciona a posi��o atual ao conjunto de posi��es preenchidas
            filledPositions.Add(cellPos);
        }
    }

    private void ToggleFill()
    {
        isFillEnabled = !isFillEnabled;
        UpdateFillButtonColor();
    }

    public void DisableFill()
    {
        isFillEnabled = false;
        UpdateFillButtonColor();
    }

    private void UpdateFillButtonColor()
    {
        fillButtonImage.color = isFillEnabled ? fillEnabledColor : fillDisabledColor;
    }

    private bool IsCellWithinGridBounds(Vector3Int cellPos)
    {
        Vector3Int minCellPos = LevelEditorManager.instance.selectedTilemap.origin; // C�lula m�nima da grelha
        Vector3Int maxCellPos = minCellPos + new Vector3Int(LevelEditorManager.instance.currentGridWidth, LevelEditorManager.instance.currentGridHeight, 0); // C�lula m�xima da grelha

        return cellPos.x >= minCellPos.x && cellPos.y >= minCellPos.y && cellPos.x < maxCellPos.x && cellPos.y < maxCellPos.y;
    }
}
