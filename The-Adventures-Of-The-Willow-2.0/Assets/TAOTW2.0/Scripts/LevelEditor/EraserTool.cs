using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class EraserTool : MonoBehaviour
{
    public Button eraserTileButton; // Botão de ativação/desativação da ferramenta
    public bool isActiveEraserTile = false; // Indica se a ferramenta está ativa

    public Button eraserEnemyButton; // Botão de ativação/desativação da ferramenta
    public bool isActiveEraserEnemy = false; // Indica se a ferramenta está ativa

    public GameObject selectedEnemyObject; // Objeto inimigo selecionado

    private Transform selectedEnemySprite;
    private Transform enemyParent;

    private void Start()
    {
        eraserTileButton.onClick.AddListener(ToggleTileEraser); // Adiciona um listener ao botão para alternar a ferramenta
        eraserEnemyButton.onClick.AddListener(ToggleEnemyEraser); // Adiciona um listener ao botão para alternar a ferramenta
    }

    private void Update()
    {
        // Verifica se o mouse está sobre um elemento de UI
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (isActiveEraserTile && Mouse.current.leftButton.isPressed && !PlatformNodeEditor.instance.isNodeEditor)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector3Int cellPos = LevelEditorManager.instance.selectedTilemap.WorldToCell(mouseWorldPos);

            // Apaga a telha na posição atual
            LevelEditorManager.instance.selectedTilemap.SetTile(cellPos, null);
        }

        if (isActiveEraserEnemy && Mouse.current.leftButton.isPressed && !PlatformNodeEditor.instance.isNodeEditor)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

            if (hit.collider != null)
            {
                GameObject hitObject = hit.collider.gameObject;

                // Verifique se o objeto atingido é um sprite
                if (hitObject.CompareTag("Enemy") || hitObject.CompareTag("LevelDot") || hitObject.CompareTag("GameObject")
                    || hitObject.CompareTag("ObjectObject") || hitObject.CompareTag("MovingPlatform"))
                {
                    selectedEnemySprite = hitObject.transform;
                    if(hitObject.CompareTag("MovingPlatform"))
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
                    if(enemyParent !=null)
                    {
                        // Remove o objeto e seu conteúdo
                        Destroy(enemyParent.gameObject);
                    }
                }
            }
        }
        if(isActiveEraserEnemy || isActiveEraserTile)
        {
            if (DecorButton.instance != null)
            {
                DecorButton.instance.Deselect();
            }
            if (ObjectsButton.instance != null)
            {
                ObjectsButton.instance.Deselect();
            }
            if (GameObjectButton.instance != null)
            {
                GameObjectButton.instance.Deselect();
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
    }

    private void ToggleTileEraser()
    {
        isActiveEraserTile = !isActiveEraserTile; // Inverte o estado da ferramenta

        // Ativa ou desativa visualmente o botão (opcional, depende do seu design de UI)
        ColorBlock colors = eraserTileButton.colors;
        colors.normalColor = isActiveEraserTile ? Color.red : Color.white;
        eraserTileButton.colors = colors;
    }

    private void ToggleEnemyEraser()
    {
        isActiveEraserEnemy = !isActiveEraserEnemy; // Inverte o estado da ferramenta

        // Ativa ou desativa visualmente o botão (opcional, depende do seu design de UI)
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
            {
                return parent;
            }

            parent = parent.parent;
        }

        return null;
    }
}
