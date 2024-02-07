using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class LevelEditorCamera : MonoBehaviour
{
    [SerializeField]
    private float moveSpeedDefault = 5f; // Velocidade de movimento da câmera
    [SerializeField]
    private float moveSpeed = 5f; // Velocidade de movimento da câmera
    [SerializeField]
    private float boostSpeed = 10f; // Velocidade de movimento da câmera quando a tecla "X" é pressionada
    [SerializeField]
    private float cameraPadding = 5f; // Espaçamento adicional para os limites da câmera

    private float minX, maxX, minY, maxY; // Limites de movimento da câmera

    [SerializeField]
    private float zoomSpeed = 5f; // Velocidade de zoom da câmera
    [SerializeField]
    private float minZoomSize = 1f; // Tamanho mínimo do zoom
    [SerializeField]
    private float maxZoomSize = 10f; // Tamanho máximo do zoom
    [SerializeField]
    private float defaultZoomSize = 5f; // Tamanho de zoom padrão
    private float currentZoomSize = 5f; // Tamanho de zoom atual

    private float horizontal;
    private float vertical;


    public void CameraMovement(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
        vertical = context.ReadValue<Vector2>().y;
    }

    private void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && !LevelEditorManager.instance.isCTRLClicked)
        {
            // O rato não está sobre a UI
            MoveCamera();
            BoostCameraSpeed();
            ZoomCamera();
        }
    }

    private void CalculateCameraBounds()
    {
        int gridWidth = LevelEditorManager.instance.currentGridWidth;
        int gridHeight = LevelEditorManager.instance.currentGridHeight;
        float tileSize = LevelEditorManager.instance.selectedTilemap.cellSize.x;

        float minWorldX = 0; // Começa em 0 na borda esquerda
        float maxWorldX = gridWidth * tileSize; // Termina no tamanho total da grade na borda direita
        float minWorldY = 0; // Começa em 0 na borda inferior
        float maxWorldY = gridHeight * tileSize; // Termina no tamanho total da grade na borda superior

        minX = minWorldX - cameraPadding;
        maxX = maxWorldX + cameraPadding;
        minY = minWorldY - cameraPadding;
        maxY = maxWorldY + cameraPadding;
    }


    private void MoveCamera()
    {
        float horizontalInput = UserInput.instance.moveInput.x;
        float verticalInput = UserInput.instance.moveInput.y;

        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0f) * moveSpeed * Time.deltaTime;

        // Calcula a nova posição da câmera
        Vector3 newPosition = transform.position + movement;

        // Limita a posição da câmera dentro dos limites calculados
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        transform.position = newPosition;
    }
    private void BoostCameraSpeed()
    {
        if (UserInput.instance.playerMoveAndExtraActions.PlayerActions.Shoot.IsPressed())
        {
            moveSpeed = boostSpeed;
        }
        else if (UserInput.instance.playerMoveAndExtraActions.PlayerActions.Shoot.WasReleasedThisFrame())
        {
            moveSpeed = moveSpeedDefault; // Retorna à velocidade normal quando a tecla "X" é solta
        }
    }

    public void UpdateCameraBounds()
    {
        CalculateCameraBounds();
    }

    private void ZoomCamera()
    {
        float zoomInput = Mouse.current.scroll.y.ReadValue();

        // Verifica se o usuário está usando a roda do mouse para zoom
        if (zoomInput != 0)
        {
            // Calcula o novo tamanho de zoom
            currentZoomSize -= zoomInput * zoomSpeed;

            // Limita o tamanho de zoom dentro dos valores mínimo e máximo
            currentZoomSize = Mathf.Clamp(currentZoomSize, minZoomSize, maxZoomSize);

            // Atualiza a posição da câmera para manter o centro de foco
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 cameraPos = transform.position;
            Vector3 newCameraPos = cameraPos + (mousePos - cameraPos) * zoomInput;

            transform.position = newCameraPos;
            Camera.main.orthographicSize = currentZoomSize;
        }
        else
        {
            // Verifica se a tecla Q está pressionada para zoom in
            if (Keyboard.current.qKey.isPressed)
            {
                ZoomIn();
            }

            // Verifica se a tecla E está pressionada para zoom out
            if (Keyboard.current.eKey.isPressed)
            {
                ZoomOut();
            }
        }

        // Verifica se a tecla R está sendo pressionada para redefinir o zoom para a posição normal
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetZoom();
        }
    }

    private void ZoomIn()
    {
        // Calcula o novo tamanho de zoom
        currentZoomSize -= zoomSpeed * Time.deltaTime;

        // Limita o tamanho de zoom dentro dos valores mínimo e máximo
        currentZoomSize = Mathf.Clamp(currentZoomSize, minZoomSize, maxZoomSize);

        Camera.main.orthographicSize = currentZoomSize;
    }

    private void ZoomOut()
    {
        // Calcula o novo tamanho de zoom
        currentZoomSize += zoomSpeed * Time.deltaTime;

        // Limita o tamanho de zoom dentro dos valores mínimo e máximo
        currentZoomSize = Mathf.Clamp(currentZoomSize, minZoomSize, maxZoomSize);

        Camera.main.orthographicSize = currentZoomSize;
    }

    private void ResetZoom()
    {
        currentZoomSize = defaultZoomSize;
        Camera.main.orthographicSize = currentZoomSize;
    }

}
