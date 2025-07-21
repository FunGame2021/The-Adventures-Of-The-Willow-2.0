using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch; // Para touch avançado
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class EraserTool : MonoBehaviour
{
    public Button eraserTileButton;
    public bool isActiveEraserTile = false;

    public Button eraserEnemyButton;
    public bool isActiveEraserEnemy = false;

    public GameObject selectedEnemyObject;

    private Transform selectedEnemySprite;
    private Transform enemyParent;

    private void Start()
    {
        eraserTileButton.onClick.AddListener(ToggleTileEraser);
        eraserEnemyButton.onClick.AddListener(ToggleEnemyEraser);

        EnhancedTouchSupport.Enable(); // Ativa o suporte ao toque aprimorado
    }

    private void Update()
    {
        // Ignora se estiver sobre UI
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        // --- Mouse ---
        if (isActiveEraserTile && Mouse.current.leftButton.isPressed && !PlatformNodeEditor.instance.isNodeEditor)
        {
            EraseTileAtMouse();
        }

        if (isActiveEraserEnemy && Mouse.current.leftButton.isPressed && !PlatformNodeEditor.instance.isNodeEditor)
        {
            EraseEnemyAtMouse();
        }

        // --- Touch ---
        if (Touch.activeTouches.Count > 0 && !PlatformNodeEditor.instance.isNodeEditor)
        {
            var touch = Touch.activeTouches[0]; // Pega o primeiro toque

            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began || touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                if (isActiveEraserTile)
                {
                    EraseTileAtPosition(touch.screenPosition);
                }

                if (isActiveEraserEnemy)
                {
                    EraseEnemyAtPosition(touch.screenPosition);
                }
            }
        }

        // Deselect buttons when eraser active
        if (isActiveEraserEnemy || isActiveEraserTile)
        {
            if (DecorButton.instance != null) DecorButton.instance.Deselect();
            if (ObjectsButton.instance != null) ObjectsButton.instance.Deselect();
            if (GameObjectButton.instance != null) GameObjectButton.instance.Deselect();
            if (Decor2Button.instance != null) Decor2Button.instance.Deselect();
            if (EnemyButton.instance != null) EnemyButton.instance.Deselect();
        }
    }

    private void EraseTileAtMouse()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector3Int cellPos = LevelEditorManager.instance.selectedTilemap.WorldToCell(mouseWorldPos);
        LevelEditorManager.instance.selectedTilemap.SetTile(cellPos, null);
    }

    private void EraseEnemyAtMouse()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
            TryEraseEnemy(hit.collider.gameObject);
    }

    private void EraseTileAtPosition(Vector2 screenPosition)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPosition);
        Vector3Int cellPos = LevelEditorManager.instance.selectedTilemap.WorldToCell(worldPos);
        LevelEditorManager.instance.selectedTilemap.SetTile(cellPos, null);
    }

    private void EraseEnemyAtPosition(Vector2 screenPosition)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
            TryEraseEnemy(hit.collider.gameObject);
    }

    private void TryEraseEnemy(GameObject hitObject)
    {
        if (hitObject.CompareTag("Enemy") || hitObject.CompareTag("LevelDot") || hitObject.CompareTag("GameObject")
            || hitObject.CompareTag("ObjectObject") || hitObject.CompareTag("MovingPlatform"))
        {
            selectedEnemySprite = hitObject.transform;

            if (hitObject.CompareTag("MovingPlatform"))
            {
                PlatformNodeEditor.instance.DeleteThisPlatform(hitObject);
            }
            else if (hitObject.CompareTag("LevelDot"))
            {
                enemyParent = hitObject.transform;
            }
            else if (hitObject.CompareTag("GameObject"))
            {
                enemyParent = hitObject.transform;
            }
            else if (hitObject.CompareTag("ObjectObject"))
            {
                enemyParent = hitObject.transform;
            }
            else
            {
                enemyParent = GetEnemyParent(selectedEnemySprite);
            }

            if (enemyParent != null)
            {
                Destroy(enemyParent.gameObject);
            }
        }
    }

    public void ToggleTileEraser()
    {
        isActiveEraserTile = !isActiveEraserTile;
        ColorBlock colors = eraserTileButton.colors;
        colors.normalColor = isActiveEraserTile ? Color.red : Color.white;
        eraserTileButton.colors = colors;
    }

    public void ToggleEnemyEraser()
    {
        isActiveEraserEnemy = !isActiveEraserEnemy;
        ColorBlock colors = eraserEnemyButton.colors;
        colors.normalColor = isActiveEraserEnemy ? Color.red : Color.white;
        eraserEnemyButton.colors = colors;
    }

    public void SelectEnemyObject(GameObject enemyObject)
    {
        selectedEnemyObject = enemyObject;
    }

    private Transform GetEnemyParent(Transform spriteTransform)
    {
        Transform parent = spriteTransform.parent;

        while (parent != null)
        {
            if (parent.CompareTag("EnemyObject"))
                return parent;

            parent = parent.parent;
        }

        return null;
    }
}
