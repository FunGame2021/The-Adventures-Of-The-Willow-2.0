using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class EraserTool : MonoBehaviour
{
    public Button eraserTileButton;
    public bool isActiveEraserTile = false;

    public Button eraserEnemyButton;
    public bool isActiveEraserEnemy = false;

    public Button eraserDecor1Button;
    public bool isActiveEraserDecor1 = false;

    public Button eraserDecor2Button;
    public bool isActiveEraserDecor2 = false;

    public GameObject selectedEnemyObject;

    private Transform selectedEnemySprite;
    private Transform enemyParent;

    private void Start()
    {
        eraserTileButton.onClick.AddListener(ToggleTileEraser);
        eraserEnemyButton.onClick.AddListener(ToggleEnemyEraser);
        eraserDecor1Button.onClick.AddListener(ToggleDecor1Eraser);
        eraserDecor2Button.onClick.AddListener(ToggleDecor2Eraser);

        if (!EnhancedTouchSupport.enabled)
        {
            try
            {
                EnhancedTouchSupport.Enable();
                Debug.Log("EnhancedTouch enabled successfully.");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to enable EnhancedTouch: " + e.Message);
            }
        }
    }

    private void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;
#if UNITY_EDITOR || UNITY_STANDALONE
        // --- Mouse ---
        if (Keyboard.current != null && isActiveEraserTile && Mouse.current.leftButton.isPressed)
            EraseTileAtMouse();

        if (Keyboard.current != null && isActiveEraserEnemy && Mouse.current.leftButton.isPressed)
            EraseEnemyAtMouse();

        if (Keyboard.current != null && isActiveEraserDecor1 && Mouse.current.leftButton.isPressed)
            EraseDecor1AtMouse();

        if (Keyboard.current != null && isActiveEraserDecor2 && Mouse.current.leftButton.isPressed)
            EraseDecor2AtMouse();
#else
        // --- Touch ---
        if (Touch.activeTouches.Count > 0)
        {
            var touch = Touch.activeTouches[0];

            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began ||
                touch.phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                touch.phase == UnityEngine.InputSystem.TouchPhase.Stationary)
            {
                if (isActiveEraserTile)
                    EraseTileAtPosition(touch.screenPosition);

                if (isActiveEraserEnemy)
                    EraseEnemyAtPosition(touch.screenPosition);

                if (isActiveEraserDecor1)
                    EraseDecorAtPosition(touch.screenPosition, decor1: true);

                if (isActiveEraserDecor2)
                    EraseDecorAtPosition(touch.screenPosition, decor1: false);
            }
        }
#endif
        // Deselect buttons
        if (isActiveEraserEnemy || isActiveEraserDecor1 || isActiveEraserDecor2 || isActiveEraserTile)
        {
            if (DecorButton.instance != null) DecorButton.instance.Deselect();
            if (ObjectsButton.instance != null) ObjectsButton.instance.Deselect();
            if (GameObjectButton.instance != null) GameObjectButton.instance.Deselect();
            if (Decor2Button.instance != null) Decor2Button.instance.Deselect();
            if (EnemyButton.instance != null) EnemyButton.instance.Deselect();
        }

        try
        {
            if (EventSystem.current == null)
                Debug.LogWarning("EventSystem.current está null!");

            if (Keyboard.current == null)
                Debug.LogWarning("Keyboard.current está null");

            if (Mouse.current == null)
                Debug.LogWarning("Mouse.current está null");

            if (Camera.main == null)
                Debug.LogWarning("Camera.main está null!");

            if (LevelEditorManager.instance == null)
                Debug.LogWarning("LevelEditorManager.instance está null!");

            // Executa seu código normalmente aqui

        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro capturado no Update: " + e.Message + "\n" + e.StackTrace);
        }
    }

    private void EraseTileAtMouse()
    {
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = ScreenToWorldWithZ(mouseScreenPos);
        Vector3Int cellPos = LevelEditorManager.instance.selectedTilemap.WorldToCell(worldPos);
        LevelEditorManager.instance.selectedTilemap.SetTile(cellPos, null);
    }

    private void EraseEnemyAtMouse()
    {
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = ScreenToWorldWithZ(mouseScreenPos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            Debug.Log("EraseEnemyAtMouse hit: " + hit.collider.gameObject.name);
            TryEraseEnemy(hit.collider.gameObject);
        }
    }

    private void EraseDecor1AtMouse()
    {
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = ScreenToWorldWithZ(mouseScreenPos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            Debug.Log("EraseDecor1AtMouse hit: " + hit.collider.gameObject.name);
            TryEraseDecor1(hit.collider.gameObject);
        }
    }

    private void EraseDecor2AtMouse()
    {
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = ScreenToWorldWithZ(mouseScreenPos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            Debug.Log("EraseDecor2AtMouse hit: " + hit.collider.gameObject.name);
            TryEraseDecor2(hit.collider.gameObject);
        }
    }

    private void EraseTileAtPosition(Vector2 screenPosition)
    {
        Vector3 worldPos = ScreenToWorldWithZ(screenPosition);
        Vector3Int cellPos = LevelEditorManager.instance.selectedTilemap.WorldToCell(worldPos);
        LevelEditorManager.instance.selectedTilemap.SetTile(cellPos, null);
    }

    private void EraseEnemyAtPosition(Vector2 screenPosition)
    {
        Vector3 worldPos = ScreenToWorldWithZ(screenPosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            Debug.Log("EraseEnemyAtPosition hit: " + hit.collider.gameObject.name);
            TryEraseEnemy(hit.collider.gameObject);
        }
        else
        {
            Debug.Log("EraseEnemyAtPosition no hit at pos: " + worldPos);
        }
    }

    private void EraseDecorAtPosition(Vector2 screenPosition, bool decor1)
    {
        Vector3 worldPos = ScreenToWorldWithZ(screenPosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            Debug.Log($"EraseDecorAtPosition hit: {hit.collider.gameObject.name} (decor1={decor1})");
            if (decor1)
                TryEraseDecor1(hit.collider.gameObject);
            else
                TryEraseDecor2(hit.collider.gameObject);
        }
        else
        {
            Debug.Log($"EraseDecorAtPosition no hit at pos: {worldPos} (decor1={decor1})");
        }
    }

    private Vector3 ScreenToWorldWithZ(Vector2 screenPosition)
    {
        Vector3 pos = new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(Camera.main.transform.position.z));
        return Camera.main.ScreenToWorldPoint(pos);
    }

    private void TryEraseEnemy(GameObject hitObject)
    {
        // Apaga diretamente se for GameObject, ObjectObject ou LevelDot
        if (hitObject.CompareTag("GameObject") ||
            hitObject.CompareTag("ObjectObject") ||
            hitObject.CompareTag("LevelDot"))
        {
            Destroy(hitObject);
            return;
        }

        // Caso seja plataforma, use o sistema especial
        if (hitObject.CompareTag("MovingPlatform"))
        {
            PlatformNodeEditor.instance.DeleteThisPlatform(hitObject);
            return;
        }

        // Se for Enemy, tenta apagar o pai com tag "EnemyObject"
        if (hitObject.CompareTag("Enemy"))
        {
            selectedEnemySprite = hitObject.transform;
            enemyParent = GetEnemyParent(selectedEnemySprite);

            if (enemyParent != null)
                Destroy(enemyParent.gameObject);
            else
                Destroy(hitObject); // segurança: apaga direto se pai não for achado
        }
    }

    private void TryEraseDecor1(GameObject hitObject)
    {
        if (hitObject.CompareTag("DecorObject"))
            Destroy(hitObject);
    }

    private void TryEraseDecor2(GameObject hitObject)
    {
        if (hitObject.CompareTag("Decor2Object"))
            Destroy(hitObject);
    }

    public void ToggleTileEraser()
    {
        bool willBeActive = !isActiveEraserTile;
        DisableAllErasers();
        isActiveEraserTile = willBeActive;
        UpdateButtonColor(eraserTileButton, isActiveEraserTile);
    }

    public void ToggleEnemyEraser()
    {
        bool willBeActive = !isActiveEraserEnemy;
        DisableAllErasers();
        isActiveEraserEnemy = willBeActive;
        UpdateButtonColor(eraserEnemyButton, isActiveEraserEnemy);
    }

    public void ToggleDecor1Eraser()
    {
        bool willBeActive = !isActiveEraserDecor1;
        DisableAllErasers();
        isActiveEraserDecor1 = willBeActive;
        UpdateButtonColor(eraserDecor1Button, isActiveEraserDecor1);
    }

    public void ToggleDecor2Eraser()
    {
        bool willBeActive = !isActiveEraserDecor2;
        DisableAllErasers();
        isActiveEraserDecor2 = willBeActive;
        UpdateButtonColor(eraserDecor2Button, isActiveEraserDecor2);
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
    public void DisableAllErasers()
    {
        isActiveEraserTile = false;
        isActiveEraserEnemy = false;
        isActiveEraserDecor1 = false;
        isActiveEraserDecor2 = false;

        UpdateButtonColor(eraserTileButton, false);
        UpdateButtonColor(eraserEnemyButton, false);
        UpdateButtonColor(eraserDecor1Button, false);
        UpdateButtonColor(eraserDecor2Button, false);
    }

    private void UpdateButtonColor(Button button, bool isActive)
    {
        if (button == null) return;

        Color color = isActive ? Color.red : Color.white;
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = color;
        colors.pressedColor = color;
        colors.selectedColor = color;
        button.colors = colors;
    }
}
