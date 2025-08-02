using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ResizeObject : MonoBehaviour
{
    private Vector2 initialScale;
    private Vector2 initialPointerPosition;
    private bool isResizing = false;
    private GameObject hitObject;

    [Header("Arrow References")]
    public GameObject topArrow;
    public GameObject bottomArrow;
    public GameObject leftArrow;
    public GameObject rightArrow;
    public GameObject topLeftArrow;
    public GameObject topRightArrow;
    public GameObject bottomLeftArrow;
    public GameObject bottomRightArrow;

    public GameObject squareToResize;

    private void Start()
    {
        UpdateArrowPositions();
    }

    private void Update()
    {
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            HandleTouchInput();
        }
        else
        {
            HandleMouseInput();
        }
    }

    private void HandleMouseInput()
    {
        // Verifica se está clicando na UI
        if (EventSystem.current.IsPointerOverGameObject())
        {
            if (isResizing)
            {
                CancelResizing();
            }
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            // Se clicou em área vazia e está redimensionando, cancela
            if (hit.collider == null && isResizing)
            {
                CancelResizing();
                return;
            }

            if (hit.collider != null && IsArrow(hit.collider.gameObject))
            {
                hitObject = hit.collider.gameObject;
                initialScale = squareToResize.transform.localScale;
                initialPointerPosition = Mouse.current.position.ReadValue();
                isResizing = true;
            }
        }

        if (isResizing && Mouse.current.leftButton.isPressed)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 delta = (mousePos - initialPointerPosition) * 0.01f;
            ResizeAccordingToArrow(delta);
            UpdateArrowPositions();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            CancelResizing();
        }
    }

    private void HandleTouchInput()
    {
        if (Touchscreen.current == null || Touchscreen.current.touches.Count == 0)
            return;

        var touch = Touchscreen.current.touches[0];
        var phase = touch.phase.ReadValue();

        // Verifica se está tocando na UI
        if (IsPointerOverUIObject(touch.position.ReadValue()))
        {
            if (isResizing)
            {
                CancelResizing();
            }
            return;
        }

        if (phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(touch.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            // Se tocou em área vazia e está redimensionando, cancela
            if (hit.collider == null && isResizing)
            {
                CancelResizing();
                return;
            }

            if (hit.collider != null && IsArrow(hit.collider.gameObject))
            {
                hitObject = hit.collider.gameObject;
                initialScale = squareToResize.transform.localScale;
                initialPointerPosition = touch.position.ReadValue();
                isResizing = true;
            }
        }

        if (isResizing && phase == UnityEngine.InputSystem.TouchPhase.Moved)
        {
            Vector2 touchPos = touch.position.ReadValue();
            Vector2 delta = (touchPos - initialPointerPosition) * 0.01f;
            ResizeAccordingToArrow(delta);
            UpdateArrowPositions();
        }

        if (phase == UnityEngine.InputSystem.TouchPhase.Ended ||
            phase == UnityEngine.InputSystem.TouchPhase.Canceled)
        {
            CancelResizing();
        }
    }

    // Método auxiliar para verificar toque na UI (funciona com o novo Input System)
    private bool IsPointerOverUIObject(Vector2 touchPosition)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = touchPosition;
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private void CancelResizing()
    {
        isResizing = false;
        hitObject = null;
    }

    private bool IsArrow(GameObject obj)
    {
        return obj == topArrow || obj == bottomArrow || obj == leftArrow || obj == rightArrow ||
               obj == topLeftArrow || obj == topRightArrow || obj == bottomLeftArrow || obj == bottomRightArrow;
    }

    private void ResizeAccordingToArrow(Vector2 delta)
    {
        if (hitObject == null) return;

        Vector2 newScale = initialScale;

        if (hitObject == topArrow)
        {
            newScale.y += delta.y;
        }
        else if (hitObject == bottomArrow)
        {
            newScale.y -= delta.y;
        }
        else if (hitObject == leftArrow)
        {
            newScale.x -= delta.x;
        }
        else if (hitObject == rightArrow)
        {
            newScale.x += delta.x;
        }
        else if (hitObject == topLeftArrow)
        {
            newScale.x -= delta.x;
            newScale.y += delta.y;
        }
        else if (hitObject == topRightArrow)
        {
            newScale.x += delta.x;
            newScale.y += delta.y;
        }
        else if (hitObject == bottomLeftArrow)
        {
            newScale.x -= delta.x;
            newScale.y -= delta.y;
        }
        else if (hitObject == bottomRightArrow)
        {
            newScale.x += delta.x;
            newScale.y -= delta.y;
        }

        // Limita escala mínima
        squareToResize.transform.localScale = new Vector2(
            Mathf.Max(newScale.x, 0.1f),
            Mathf.Max(newScale.y, 0.1f)
        );
    }

    private void UpdateArrowPositions()
    {
        Vector3 pos = squareToResize.transform.position;
        Vector3 scale = squareToResize.transform.localScale;

        topArrow.transform.position = pos + new Vector3(0, scale.y / 2f, 0);
        bottomArrow.transform.position = pos - new Vector3(0, scale.y / 2f, 0);
        leftArrow.transform.position = pos - new Vector3(scale.x / 2f, 0, 0);
        rightArrow.transform.position = pos + new Vector3(scale.x / 2f, 0, 0);

        topLeftArrow.transform.position = pos + new Vector3(-scale.x / 2f, scale.y / 2f, 0);
        topRightArrow.transform.position = pos + new Vector3(scale.x / 2f, scale.y / 2f, 0);
        bottomLeftArrow.transform.position = pos + new Vector3(-scale.x / 2f, -scale.y / 2f, 0);
        bottomRightArrow.transform.position = pos + new Vector3(scale.x / 2f, -scale.y / 2f, 0);
    }
}