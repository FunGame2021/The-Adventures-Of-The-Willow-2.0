using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class TileSelectionTool : MonoBehaviour
{
    public static TileSelectionTool instance;

    public Button toggleButton; // Refer�ncia ao bot�o de ativa��o/desativa��o da ferramenta
    public GameObject selectionIndicator; // Refer�ncia ao sprite do indicador de sele��o

    public bool isActive = false; // Estado da ferramenta (ativa ou n�o)
    public Color selectedColor = Color.red; // Cor das telhas selecionadas
    [HideInInspector] public List<Vector3Int> selectedTiles = new List<Vector3Int>(); // Lista das telhas selecionadas

    private Dictionary<Vector3Int, Color> originalColors = new Dictionary<Vector3Int, Color>(); // Cores originais das telhas selecionadas

    private Vector3 startMousePosition; // Posi��o inicial do mouse ao come�ar a sele��o
    private SpriteRenderer selectionRenderer; // Refer�ncia ao componente SpriteRenderer do indicador de sele��o


    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        selectionRenderer = selectionIndicator.GetComponent<SpriteRenderer>();
        selectionRenderer.enabled = false;
    }

    // Evento de ativa��o/desativa��o da ferramenta chamado pelo bot�o de UI
    public void ToggleTool()
    {
        isActive = !isActive;

        if (isActive)
        {
            // Ativar ferramenta
            toggleButton.GetComponent<Image>().color = Color.red; // Alterar a cor do bot�o para vermelho
            selectionIndicator.SetActive(true); // Ativar o indicador de sele��o

            if (DecorButton.instance != null)
            {
                DecorButton.instance.Deselect();
            }
            if (ObjectsButton.instance != null)
            {
                ObjectsButton.instance.Deselect();
            }
            if (Decor2Button.instance != null)
            {
                Decor2Button.instance.Deselect();
            }
            if (EnemyButton.instance != null)
            {
                EnemyButton.instance.Deselect();
            }
        }
        else
        {
            // Desativar ferramenta
            toggleButton.GetComponent<Image>().color = Color.white; // Restaurar a cor do bot�o para branca
            ClearSelection();
            selectionIndicator.SetActive(false); // Desativar o indicador de sele��o
            selectionRenderer.enabled = false;
        }
    }

    // Limpar a sele��o de telhas
    private void ClearSelection()
    {
        foreach (Vector3Int tilePos in selectedTiles)
        {
            // Restaurar a apar�ncia visual da telha deselecionada
            if (LevelEditorManager.instance.selectedTilemap.GetTile(tilePos) != null)
            {
                LevelEditorManager.instance.selectedTilemap.SetTileFlags(tilePos, TileFlags.None);
                LevelEditorManager.instance.selectedTilemap.SetColor(tilePos, originalColors[tilePos]);
            }
        }
        selectedTiles.Clear();
        originalColors.Clear();
    }

    // Verificar se uma posi��o est� na sele��o de telhas
    private bool IsTileSelected(Vector3Int position)
    {
        return selectedTiles.Contains(position);
    }

    // Selecionar uma telha na posi��o especificada
    private void SelectTile(Vector3Int position)
    {
        if (!IsTileSelected(position))
        {
            selectedTiles.Add(position);

            // Armazenar a cor original da telha
            TileBase tile = LevelEditorManager.instance.selectedTilemap.GetTile(position);
            if (tile != null)
            {
                Color originalColor = LevelEditorManager.instance.selectedTilemap.GetColor(position);
                originalColors.Add(position, originalColor);
            }

            // Alterar a apar�ncia visual da telha selecionada
            LevelEditorManager.instance.selectedTilemap.SetTileFlags(position, TileFlags.None);
            LevelEditorManager.instance.selectedTilemap.SetColor(position, selectedColor);
        }
    }

    private void UpdateSelectionIndicator(Vector3 startPosition, Vector3 endPosition)
    {
        // Ajustar a posi��o do indicador de sele��o
        selectionIndicator.transform.position = new Vector3(Mathf.Min(startPosition.x, endPosition.x), Mathf.Min(startPosition.y, endPosition.y), selectionIndicator.transform.position.z);

        // Ajustar o tamanho do indicador de sele��o
        float width = Mathf.Abs(endPosition.x - startPosition.x);
        float height = Mathf.Abs(endPosition.y - startPosition.y);


        // Atualizar o tamanho do 9-slice do SpriteRenderer
        selectionRenderer.size = new Vector2(width, height);

        // Converter as coordenadas do mundo para as coordenadas do grid do Tilemap
        Vector3Int startPos = LevelEditorManager.instance.selectedTilemap.WorldToCell(startPosition);
        Vector3Int endPos = LevelEditorManager.instance.selectedTilemap.WorldToCell(endPosition);

        // Obter os limites m�nimos e m�ximos de x e y
        int minX = Mathf.Min(startPos.x, endPos.x);
        int maxX = Mathf.Max(startPos.x, endPos.x);
        int minY = Mathf.Min(startPos.y, endPos.y);
        int maxY = Mathf.Max(startPos.y, endPos.y);

        // Limpar a sele��o anterior
        ClearSelection();

        // Selecionar os tiles dentro da �rea de sele��o
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                SelectTile(tilePosition);
            }
        }
    }
    private void Update()
    {
        if (isActive)
        {
            // Detectar clique para iniciar a sele��o
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                startMousePosition = Mouse.current.position.ReadValue();
                selectionRenderer.enabled = true; // ativar o indicador de sele��o
            }

            // Detectar arrasto do mouse para atualizar a sele��o
            if (Mouse.current.leftButton.isPressed)
            {
                Vector3 currentMousePosition = Mouse.current.position.ReadValue();

                // Converter a posi��o do mouse em coordenadas de mundo
                Vector3 startPosition = Camera.main.ScreenToWorldPoint(startMousePosition);
                Vector3 endPosition = Camera.main.ScreenToWorldPoint(currentMousePosition);

                // Atualizar o indicador de sele��o
                UpdateSelectionIndicator(startPosition, endPosition);

                selectionRenderer.enabled = true; // ativar o indicador de sele��o
            }

            // Detectar clique liberado para finalizar a sele��o
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                Vector3 currentMousePosition = Mouse.current.position.ReadValue();

                // Converter a posi��o do mouse em coordenadas de mundo
                Vector3 startPosition = Camera.main.ScreenToWorldPoint(startMousePosition);
                Vector3 endPosition = Camera.main.ScreenToWorldPoint(currentMousePosition);

                // Atualizar o indicador de sele��o
                UpdateSelectionIndicator(startPosition, endPosition);

                selectionRenderer.enabled = true; // ativar o indicador de sele��o
            }

            // Detectar clique direito do mouse
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                ClearSelection();
                selectionRenderer.enabled = false; // Desativar o indicador de sele��o
            }

            // Detectar pressionar a tecla "Delete"
            if (Keyboard.current[UnityEngine.InputSystem.Key.Delete].wasPressedThisFrame)
            {
                // Apagar os tiles selecionados
                foreach (Vector3Int tilePos in selectedTiles)
                {
                    LevelEditorManager.instance.selectedTilemap.SetTile(tilePos, null);
                }

                // Limpar a sele��o
                ClearSelection();
            }
        }
    }

}
