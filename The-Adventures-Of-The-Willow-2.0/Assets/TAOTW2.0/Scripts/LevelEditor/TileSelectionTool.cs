using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class TileSelectionTool : MonoBehaviour
{
    public static TileSelectionTool instance;

    public Button toggleButton; // Referência ao botão de ativação/desativação da ferramenta
    public GameObject selectionIndicator; // Referência ao sprite do indicador de seleção

    public bool isActive = false; // Estado da ferramenta (ativa ou não)
    public Color selectedColor = Color.red; // Cor das telhas selecionadas
    [HideInInspector] public List<Vector3Int> selectedTiles = new List<Vector3Int>(); // Lista das telhas selecionadas

    private Dictionary<Vector3Int, Color> originalColors = new Dictionary<Vector3Int, Color>(); // Cores originais das telhas selecionadas

    private Vector3 startMousePosition; // Posição inicial do mouse ao começar a seleção
    private SpriteRenderer selectionRenderer; // Referência ao componente SpriteRenderer do indicador de seleção


    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        selectionRenderer = selectionIndicator.GetComponent<SpriteRenderer>();
        selectionRenderer.enabled = false;
    }

    // Evento de ativação/desativação da ferramenta chamado pelo botão de UI
    public void ToggleTool()
    {
        isActive = !isActive;

        if (isActive)
        {
            // Ativar ferramenta
            toggleButton.GetComponent<Image>().color = Color.red; // Alterar a cor do botão para vermelho
            selectionIndicator.SetActive(true); // Ativar o indicador de seleção

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
            toggleButton.GetComponent<Image>().color = Color.white; // Restaurar a cor do botão para branca
            ClearSelection();
            selectionIndicator.SetActive(false); // Desativar o indicador de seleção
            selectionRenderer.enabled = false;
        }
    }

    // Limpar a seleção de telhas
    private void ClearSelection()
    {
        foreach (Vector3Int tilePos in selectedTiles)
        {
            // Restaurar a aparência visual da telha deselecionada
            if (LevelEditorManager.instance.selectedTilemap.GetTile(tilePos) != null)
            {
                LevelEditorManager.instance.selectedTilemap.SetTileFlags(tilePos, TileFlags.None);
                LevelEditorManager.instance.selectedTilemap.SetColor(tilePos, originalColors[tilePos]);
            }
        }
        selectedTiles.Clear();
        originalColors.Clear();
    }

    // Verificar se uma posição está na seleção de telhas
    private bool IsTileSelected(Vector3Int position)
    {
        return selectedTiles.Contains(position);
    }

    // Selecionar uma telha na posição especificada
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

            // Alterar a aparência visual da telha selecionada
            LevelEditorManager.instance.selectedTilemap.SetTileFlags(position, TileFlags.None);
            LevelEditorManager.instance.selectedTilemap.SetColor(position, selectedColor);
        }
    }

    private void UpdateSelectionIndicator(Vector3 startPosition, Vector3 endPosition)
    {
        // Ajustar a posição do indicador de seleção
        selectionIndicator.transform.position = new Vector3(Mathf.Min(startPosition.x, endPosition.x), Mathf.Min(startPosition.y, endPosition.y), selectionIndicator.transform.position.z);

        // Ajustar o tamanho do indicador de seleção
        float width = Mathf.Abs(endPosition.x - startPosition.x);
        float height = Mathf.Abs(endPosition.y - startPosition.y);


        // Atualizar o tamanho do 9-slice do SpriteRenderer
        selectionRenderer.size = new Vector2(width, height);

        // Converter as coordenadas do mundo para as coordenadas do grid do Tilemap
        Vector3Int startPos = LevelEditorManager.instance.selectedTilemap.WorldToCell(startPosition);
        Vector3Int endPos = LevelEditorManager.instance.selectedTilemap.WorldToCell(endPosition);

        // Obter os limites mínimos e máximos de x e y
        int minX = Mathf.Min(startPos.x, endPos.x);
        int maxX = Mathf.Max(startPos.x, endPos.x);
        int minY = Mathf.Min(startPos.y, endPos.y);
        int maxY = Mathf.Max(startPos.y, endPos.y);

        // Limpar a seleção anterior
        ClearSelection();

        // Selecionar os tiles dentro da área de seleção
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
            // Detectar clique para iniciar a seleção
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                startMousePosition = Mouse.current.position.ReadValue();
                selectionRenderer.enabled = true; // ativar o indicador de seleção
            }

            // Detectar arrasto do mouse para atualizar a seleção
            if (Mouse.current.leftButton.isPressed)
            {
                Vector3 currentMousePosition = Mouse.current.position.ReadValue();

                // Converter a posição do mouse em coordenadas de mundo
                Vector3 startPosition = Camera.main.ScreenToWorldPoint(startMousePosition);
                Vector3 endPosition = Camera.main.ScreenToWorldPoint(currentMousePosition);

                // Atualizar o indicador de seleção
                UpdateSelectionIndicator(startPosition, endPosition);

                selectionRenderer.enabled = true; // ativar o indicador de seleção
            }

            // Detectar clique liberado para finalizar a seleção
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                Vector3 currentMousePosition = Mouse.current.position.ReadValue();

                // Converter a posição do mouse em coordenadas de mundo
                Vector3 startPosition = Camera.main.ScreenToWorldPoint(startMousePosition);
                Vector3 endPosition = Camera.main.ScreenToWorldPoint(currentMousePosition);

                // Atualizar o indicador de seleção
                UpdateSelectionIndicator(startPosition, endPosition);

                selectionRenderer.enabled = true; // ativar o indicador de seleção
            }

            // Detectar clique direito do mouse
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                ClearSelection();
                selectionRenderer.enabled = false; // Desativar o indicador de seleção
            }

            // Detectar pressionar a tecla "Delete"
            if (Keyboard.current[UnityEngine.InputSystem.Key.Delete].wasPressedThisFrame)
            {
                // Apagar os tiles selecionados
                foreach (Vector3Int tilePos in selectedTiles)
                {
                    LevelEditorManager.instance.selectedTilemap.SetTile(tilePos, null);
                }

                // Limpar a seleção
                ClearSelection();
            }
        }
    }

}
