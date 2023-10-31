using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class ResizeObject : MonoBehaviour
{
    private Vector2 initialScale;
    private Vector2 initialMousePosition;
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
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()), Vector2.zero);
            if (hit.collider != null)
            {
                hitObject = hit.collider.gameObject;

                if (hitObject == topArrow || hitObject == bottomArrow || hitObject == leftArrow || hitObject == rightArrow ||
                    hitObject == topLeftArrow || hitObject == topRightArrow || hitObject == bottomLeftArrow || hitObject == bottomRightArrow)
                {
                    initialScale = squareToResize.transform.localScale;
                    initialMousePosition = Mouse.current.position.ReadValue();
                    isResizing = true;
                }
            }
        }

        if (isResizing && Mouse.current.leftButton.isPressed)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector2 mouseDelta = (mousePosition - initialMousePosition) * 0.01f;

            if (hitObject == topArrow)
            {
                squareToResize.transform.localScale = new Vector2(initialScale.x, initialScale.y + mouseDelta.y);
            }
            else if (hitObject == bottomArrow)
            {
                squareToResize.transform.localScale = new Vector2(initialScale.x, initialScale.y - mouseDelta.y);
            }
            else if (hitObject == leftArrow)
            {
                squareToResize.transform.localScale = new Vector2(initialScale.x - mouseDelta.x, initialScale.y);
            }
            else if (hitObject == rightArrow)
            {
                squareToResize.transform.localScale = new Vector2(initialScale.x + mouseDelta.x, initialScale.y);
            }
            else if (hitObject == topLeftArrow)
            {
                squareToResize.transform.localScale = new Vector2(initialScale.x - mouseDelta.x, initialScale.y + mouseDelta.y);
            }
            else if (hitObject == topRightArrow)
            {
                squareToResize.transform.localScale = new Vector2(initialScale.x + mouseDelta.x, initialScale.y + mouseDelta.y);
            }
            else if (hitObject == bottomLeftArrow)
            {
                squareToResize.transform.localScale = new Vector2(initialScale.x - mouseDelta.x, initialScale.y - mouseDelta.y);
            }
            else if (hitObject == bottomRightArrow)
            {
                squareToResize.transform.localScale = new Vector2(initialScale.x + mouseDelta.x, initialScale.y - mouseDelta.y);
            }

            squareToResize.transform.localScale = new Vector2(Mathf.Max(squareToResize.transform.localScale.x, 0.1f), Mathf.Max(squareToResize.transform.localScale.y, 0.1f));

            UpdateArrowPositions();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isResizing = false;
            hitObject = null;
        }
    }

    private void UpdateArrowPositions()
    {
        topArrow.transform.position = squareToResize.transform.position + new Vector3(0, squareToResize.transform.localScale.y / 2f, 0);
        bottomArrow.transform.position = squareToResize.transform.position - new Vector3(0, squareToResize.transform.localScale.y / 2f, 0);
        leftArrow.transform.position = squareToResize.transform.position - new Vector3(squareToResize.transform.localScale.x / 2f, 0, 0);
        rightArrow.transform.position = squareToResize.transform.position + new Vector3(squareToResize.transform.localScale.x / 2f, 0, 0);

        topLeftArrow.transform.position = squareToResize.transform.position + new Vector3(-squareToResize.transform.localScale.x / 2f, squareToResize.transform.localScale.y / 2f, 0);
        topRightArrow.transform.position = squareToResize.transform.position + new Vector3(squareToResize.transform.localScale.x / 2f, squareToResize.transform.localScale.y / 2f, 0);
        bottomLeftArrow.transform.position = squareToResize.transform.position + new Vector3(-squareToResize.transform.localScale.x / 2f, -squareToResize.transform.localScale.y / 2f, 0);
        bottomRightArrow.transform.position = squareToResize.transform.position + new Vector3(squareToResize.transform.localScale.x / 2f, -squareToResize.transform.localScale.y / 2f, 0);
    }
}
