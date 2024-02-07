using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class LevelEditorCamera : MonoBehaviour
{
    [SerializeField]
    private float moveSpeedDefault = 5f; // Velocidade de movimento da c�mera
    [SerializeField]
    private float moveSpeed = 5f; // Velocidade de movimento da c�mera
    [SerializeField]
    private float boostSpeed = 10f; // Velocidade de movimento da c�mera quando a tecla "X" � pressionada
    [SerializeField]
    private float cameraPadding = 5f; // Espa�amento adicional para os limites da c�mera

    private float minX, maxX, minY, maxY; // Limites de movimento da c�mera

    [SerializeField]
    private float zoomSpeed = 5f; // Velocidade de zoom da c�mera
    [SerializeField]
    private float minZoomSize = 1f; // Tamanho m�nimo do zoom
    [SerializeField]
    private float maxZoomSize = 10f; // Tamanho m�ximo do zoom
    [SerializeField]
    private float defaultZoomSize = 5f; // Tamanho de zoom padr�o
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
            // O rato n�o est� sobre a UI
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

        float minWorldX = 0; // Come�a em 0 na borda esquerda
        float maxWorldX = gridWidth * tileSize; // Termina no tamanho total da grade na borda direita
        float minWorldY = 0; // Come�a em 0 na borda inferior
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

        // Calcula a nova posi��o da c�mera
        Vector3 newPosition = transform.position + movement;

        // Limita a posi��o da c�mera dentro dos limites calculados
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
            moveSpeed = moveSpeedDefault; // Retorna � velocidade normal quando a tecla "X" � solta
        }
    }

    public void UpdateCameraBounds()
    {
        CalculateCameraBounds();
    }

    private void ZoomCamera()
    {
        float zoomInput = Mouse.current.scroll.y.ReadValue();

        // Verifica se o usu�rio est� usando a roda do mouse para zoom
        if (zoomInput != 0)
        {
            // Calcula o novo tamanho de zoom
            currentZoomSize -= zoomInput * zoomSpeed;

            // Limita o tamanho de zoom dentro dos valores m�nimo e m�ximo
            currentZoomSize = Mathf.Clamp(currentZoomSize, minZoomSize, maxZoomSize);

            // Atualiza a posi��o da c�mera para manter o centro de foco
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 cameraPos = transform.position;
            Vector3 newCameraPos = cameraPos + (mousePos - cameraPos) * zoomInput;

            transform.position = newCameraPos;
            Camera.main.orthographicSize = currentZoomSize;
        }
        else
        {
            // Verifica se a tecla Q est� pressionada para zoom in
            if (Keyboard.current.qKey.isPressed)
            {
                ZoomIn();
            }

            // Verifica se a tecla E est� pressionada para zoom out
            if (Keyboard.current.eKey.isPressed)
            {
                ZoomOut();
            }
        }

        // Verifica se a tecla R est� sendo pressionada para redefinir o zoom para a posi��o normal
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetZoom();
        }
    }

    private void ZoomIn()
    {
        // Calcula o novo tamanho de zoom
        currentZoomSize -= zoomSpeed * Time.deltaTime;

        // Limita o tamanho de zoom dentro dos valores m�nimo e m�ximo
        currentZoomSize = Mathf.Clamp(currentZoomSize, minZoomSize, maxZoomSize);

        Camera.main.orthographicSize = currentZoomSize;
    }

    private void ZoomOut()
    {
        // Calcula o novo tamanho de zoom
        currentZoomSize += zoomSpeed * Time.deltaTime;

        // Limita o tamanho de zoom dentro dos valores m�nimo e m�ximo
        currentZoomSize = Mathf.Clamp(currentZoomSize, minZoomSize, maxZoomSize);

        Camera.main.orthographicSize = currentZoomSize;
    }

    private void ResetZoom()
    {
        currentZoomSize = defaultZoomSize;
        Camera.main.orthographicSize = currentZoomSize;
    }

}
