using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class GridCursor : MonoBehaviour
{
    Vector2 inputPos;
    public GameObject cursor;
    Vector3Int cellPos;

    private void Update()
    {
        CalculatePositions();
        MoveCursorToCell(GetCurrentCell());
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
        Vector3 cellCenterPos = LevelEditorManager.instance.selectedTilemap.GetCellCenterWorld(cell);
        cursor.transform.position = cellCenterPos;
    }
}
