using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class GridCursor : MonoBehaviour
{
    Vector2 inputPos;
    public GameObject cursor;
    Vector3Int cellPos;
    [SerializeField] private Color NormalCursorColor;
    [SerializeField] private Color SnapCursorColor;

    private void Update()
    {
        CalculatePositions();
        UpdateCursorSize();
        if (LevelEditorManager.instance.snapGrid)
        {
            cursor.GetComponent<SpriteRenderer>().color = SnapCursorColor;
            // Converte a posição do mouse para a posição no mundo
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            // Define a posição Z do objeto temporário para 0
            mousePosition.z = 0f;

            mousePosition.x = Mathf.Floor(mousePosition.x / LevelEditorManager.instance.SnapGridSize) * LevelEditorManager.instance.SnapGridSize + LevelEditorManager.instance.SnapGridSize / 2f;
            mousePosition.y = Mathf.Floor(mousePosition.y / LevelEditorManager.instance.SnapGridSize) * LevelEditorManager.instance.SnapGridSize + LevelEditorManager.instance.SnapGridSize / 2f;

            // Aplica a posição alinhada à grelha
            cursor.transform.position = mousePosition;
        }
        else
        {
            cursor.GetComponent<SpriteRenderer>().color = NormalCursorColor;
            MoveCursorToCell(GetCurrentCell());
        }
    }

    void CalculatePositions()
    {
        if (Mouse.current != null)
        {
            inputPos = Mouse.current.position.ReadValue();
        }
        else if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            inputPos = Touchscreen.current.touches[0].position.ReadValue();
        }
        else
        {
            // Tratamento de erro quando não há entrada de mouse ou toque
            return;
        }

        Vector3 worldPos = LevelEditorManager.instance.mainCamera.ScreenToWorldPoint(inputPos);
        cellPos = LevelEditorManager.instance.selectedTilemap.WorldToCell(worldPos);
    }

    Vector3Int GetCurrentCell()
    {
        return cellPos;
    }

    void MoveCursorToCell(Vector3Int cell)
    {
        // Obtém o tamanho real do quadrado da célula
        float cellSize = LevelEditorManager.instance.selectedTilemap.cellSize.x;

        // Ajusta a posição para o centro do quadrado de snap, levando em consideração a escala
        Vector3 cellCenterPos = LevelEditorManager.instance.selectedTilemap.GetCellCenterWorld(cell);

        // Se a grelha de snap estiver desativada, ajusta a posição para o centro da célula normal
        cursor.transform.position = cellCenterPos;

    }





    void UpdateCursorSize()
    {
        float cellSize = LevelEditorManager.instance.selectedTilemap.cellSize.x;

        // Define o tamanho do cursor para o tamanho do quadrado de snap ou célula normal
        float scaleFactor = LevelEditorManager.instance.snapGrid ? LevelEditorManager.instance.SnapGridSize : 1f;
        cursor.transform.localScale = new Vector3(cellSize, cellSize, 1f) * scaleFactor;
    }


}
