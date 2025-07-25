using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class LevelEditorCamera : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeedDefault = 5f;        // Velocidade padrão teclado/mouse
    [SerializeField] private float moveSpeedKeyboard = 5f;       // Velocidade teclado/mouse
    [SerializeField] private float moveSpeedTouch = 0.5f;         // Velocidade touch
    [SerializeField] private float boostSpeed = 10f;              // Boost teclado/mouse ao pressionar "X"
    [SerializeField] private float cameraPadding = 5f;            // Espaçamento adicional limites da câmera

    [Header("Zoom")]
    [SerializeField] private float zoomSpeedKeyboard = 5f;        // Velocidade zoom teclado/mouse
    [SerializeField] private float zoomSpeedTouch = 0.01f;       // Velocidade zoom touch
    [SerializeField] private float minZoomSize = 1f;
    [SerializeField] private float maxZoomSize = 10f;
    [SerializeField] private float defaultZoomSize = 5f;

    private float currentZoomSize;
    private float minX, maxX, minY, maxY;
    private float horizontal;
    private float vertical;
    private float moveSpeed;

    [SerializeField] private EasyJoystick.Joysticknew joystickUI;






    private void Awake()
    {
        currentZoomSize = defaultZoomSize;
        moveSpeed = moveSpeedDefault;
    }
    public void CameraMovement(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
        vertical = context.ReadValue<Vector2>().y;
    }

    private void Update()
    {

        if (!LevelEditorManager.instance.isCTRLClicked)
        {

            if (Touchscreen.current != null && (Touch.activeTouches.Count > 0 || (joystickUI != null && joystickUI.IsTouching)))
            {
                // Touch detected, usa movimento e zoom touch
                HandleTouchControls();
            }
            else
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    // Sem touch, usa teclado/mouse
                    MoveCameraKeyboard();
                    ZoomCameraKeyboard();
                }
            }
        }
        else
        {
            HandleBoostSpeed();
        }
    }

    private void HandleBoostSpeed()
    {
        if (Keyboard.current != null && UserInput.instance.playerMoveAndExtraActions.PlayerActions.Shoot.IsPressed())
        {
            moveSpeed = boostSpeed;
        }
        else if (Keyboard.current != null && UserInput.instance.playerMoveAndExtraActions.PlayerActions.Shoot.WasReleasedThisFrame())
        {
            moveSpeed = moveSpeedDefault;
        }
    }

    private void MoveCameraKeyboard()
    {
        Vector3 movement = new Vector3(horizontal, vertical, 0f) * moveSpeedKeyboard * Time.deltaTime;

        Vector3 newPosition = transform.position + movement;
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        transform.position = newPosition;
    }

    private void ZoomCameraKeyboard()
    {
        if (Mouse.current != null)
        {
            float zoomInput = Mouse.current.scroll.y.ReadValue();

            if (zoomInput != 0)
            {
                currentZoomSize -= zoomInput * zoomSpeedKeyboard;
                currentZoomSize = Mathf.Clamp(currentZoomSize, minZoomSize, maxZoomSize);
                Camera.main.orthographicSize = currentZoomSize;
            }
        }
        if (Keyboard.current != null && Keyboard.current.qKey.isPressed) ZoomIn();
        if (Keyboard.current != null && Keyboard.current.eKey.isPressed) ZoomOut();

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) ResetZoom();
    }


    private void HandleTouchControls()
    {
        if (Touch.activeTouches.Count < 1)
            return;

        Vector2 input = Vector2.zero;

        // Usa joystick se estiver a ser usado
        if (joystickUI != null && joystickUI.IsTouching)
        {
            input = new Vector2(joystickUI.Horizontal(), joystickUI.Vertical());
        }
        else
        {
            input = UserInput.instance.moveInput;
        }

        if (input != Vector2.zero)
        {
            Vector3 movement = new Vector3(input.x, input.y, 0f) * moveSpeedTouch * Time.deltaTime;

            Vector3 newPosition = transform.position + movement;
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

            transform.position = newPosition;
        }

        // permite zoom com dois dedos fora da UI
        if (Touch.activeTouches.Count >= 2)
        {
            var touch0 = Touch.activeTouches[0];
            var touch1 = Touch.activeTouches[1];

            bool touch0OnUI = EventSystem.current.IsPointerOverGameObject(touch0.finger.index);
            bool touch1OnUI = EventSystem.current.IsPointerOverGameObject(touch1.finger.index);

            if (!touch0OnUI && !touch1OnUI)
            {
                Vector2 currentPos0 = touch0.screenPosition;
                Vector2 currentPos1 = touch1.screenPosition;

                Vector2 delta0 = touch0.delta;
                Vector2 delta1 = touch1.delta;

                Vector2 prevPos0 = currentPos0 - delta0;
                Vector2 prevPos1 = currentPos1 - delta1;

                float currentDistance = Vector2.Distance(currentPos0, currentPos1);
                float prevDistance = Vector2.Distance(prevPos0, prevPos1);
                float distanceDelta = currentDistance - prevDistance;

                Vector2 currentMid = (currentPos0 + currentPos1) * 0.5f;

                if (Mathf.Abs(distanceDelta) > 0.01f)
                {
                    Vector3 worldBefore = Camera.main.ScreenToWorldPoint(currentMid);

                    currentZoomSize -= distanceDelta * zoomSpeedTouch;
                    currentZoomSize = Mathf.Clamp(currentZoomSize, minZoomSize, maxZoomSize);
                    Camera.main.orthographicSize = currentZoomSize;

                    Vector3 worldAfter = Camera.main.ScreenToWorldPoint(currentMid);
                    Vector3 correction = worldBefore - worldAfter;

                    Vector3 newPos = transform.position + correction;
                    newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
                    newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
                    transform.position = newPos;
                }
            }
        }
    }





    private void ZoomIn()
    {
        currentZoomSize -= zoomSpeedKeyboard * Time.deltaTime;
        currentZoomSize = Mathf.Clamp(currentZoomSize, minZoomSize, maxZoomSize);
        Camera.main.orthographicSize = currentZoomSize;
    }

    private void ZoomOut()
    {
        currentZoomSize += zoomSpeedKeyboard * Time.deltaTime;
        currentZoomSize = Mathf.Clamp(currentZoomSize, minZoomSize, maxZoomSize);
        Camera.main.orthographicSize = currentZoomSize;
    }

    private void ResetZoom()
    {
        currentZoomSize = defaultZoomSize;
        Camera.main.orthographicSize = currentZoomSize;
    }

    public void UpdateCameraBounds()
    {
        int gridWidth = LevelEditorManager.instance.currentGridWidth;
        int gridHeight = LevelEditorManager.instance.currentGridHeight;
        float tileSize = LevelEditorManager.instance.selectedTilemap.cellSize.x;

        float minWorldX = 0;
        float maxWorldX = gridWidth * tileSize;
        float minWorldY = 0;
        float maxWorldY = gridHeight * tileSize;

        minX = minWorldX - cameraPadding;
        maxX = maxWorldX + cameraPadding;
        minY = minWorldY - cameraPadding;
        maxY = maxWorldY + cameraPadding;
    }
}
