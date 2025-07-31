using UnityEngine;
using UnityEngine.InputSystem;

public class ResizeObject : MonoBehaviour
{
    private Vector2 initialScale;
    private Vector2 initialPointerPosition;
    private bool isResizing = false;
    private GameObject hitObject;

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
        if (Touchscreen.current != null)
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
        if (Touchscreen.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null && IsArrow(hit.collider.gameObject))
            {
                hitObject = hit.collider.gameObject;
                initialScale = squareToResize.transform.localScale;
                initialPointerPosition = Mouse.current.position.ReadValue();
                isResizing = true;
            }
        }

        if (isResizing && Touchscreen.current != null && Mouse.current.leftButton.isPressed)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 delta = (mousePos - initialPointerPosition) * 0.01f;
            ResizeAccordingToArrow(delta);
            UpdateArrowPositions();
        }

        if (Touchscreen.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isResizing = false;
            hitObject = null;
        }
    }

    private void HandleTouchInput()
    {
        if (Touchscreen.current == null)
        {
            Debug.Log("Touchscreen não encontrado");
            return;
        }

        var touches = Touchscreen.current.touches;

        if (touches.Count == 0) return;

        var touch = touches[0];
        Debug.Log($"Touch phase: {touch.phase.ReadValue()}");

        if (touch.press.wasPressedThisFrame)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(touch.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null && IsArrow(hit.collider.gameObject))
            {
                hitObject = hit.collider.gameObject;
                initialScale = squareToResize.transform.localScale;
                initialPointerPosition = touch.position.ReadValue();
                isResizing = true;
            }
        }

        if (isResizing && touch.press.isPressed)
        {
            Vector2 touchPos = touch.position.ReadValue();
            Vector2 delta = (touchPos - initialPointerPosition) * 0.01f;
            ResizeAccordingToArrow(delta);
            UpdateArrowPositions();
        }

        if (touch.press.wasReleasedThisFrame)
        {
            isResizing = false;
            hitObject = null;
        }
    }

    private bool IsArrow(GameObject obj)
    {
        return obj == topArrow || obj == bottomArrow || obj == leftArrow || obj == rightArrow ||
               obj == topLeftArrow || obj == topRightArrow || obj == bottomLeftArrow || obj == bottomRightArrow;
    }

    private void ResizeAccordingToArrow(Vector2 delta)
    {
        if (hitObject == topArrow)
        {
            squareToResize.transform.localScale = new Vector2(initialScale.x, initialScale.y + delta.y);
        }
        else if (hitObject == bottomArrow)
        {
            squareToResize.transform.localScale = new Vector2(initialScale.x, initialScale.y - delta.y);
        }
        else if (hitObject == leftArrow)
        {
            squareToResize.transform.localScale = new Vector2(initialScale.x - delta.x, initialScale.y);
        }
        else if (hitObject == rightArrow)
        {
            squareToResize.transform.localScale = new Vector2(initialScale.x + delta.x, initialScale.y);
        }
        else if (hitObject == topLeftArrow)
        {
            squareToResize.transform.localScale = new Vector2(initialScale.x - delta.x, initialScale.y + delta.y);
        }
        else if (hitObject == topRightArrow)
        {
            squareToResize.transform.localScale = new Vector2(initialScale.x + delta.x, initialScale.y + delta.y);
        }
        else if (hitObject == bottomLeftArrow)
        {
            squareToResize.transform.localScale = new Vector2(initialScale.x - delta.x, initialScale.y - delta.y);
        }
        else if (hitObject == bottomRightArrow)
        {
            squareToResize.transform.localScale = new Vector2(initialScale.x + delta.x, initialScale.y - delta.y);
        }

        // Limita escala mínima para evitar tamanho zero ou negativo
        squareToResize.transform.localScale = new Vector2(
            Mathf.Max(squareToResize.transform.localScale.x, 0.1f),
            Mathf.Max(squareToResize.transform.localScale.y, 0.1f)
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
