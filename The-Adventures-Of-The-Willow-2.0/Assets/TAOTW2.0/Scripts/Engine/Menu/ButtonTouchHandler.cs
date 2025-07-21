using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonTouchHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool isPressed = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        Debug.Log("Botão pressionado");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        Debug.Log("Botão solto");
    }
}
