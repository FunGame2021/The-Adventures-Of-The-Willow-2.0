using UnityEngine;
using UnityEngine.EventSystems;

public class GrabButtonUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        UserInput.instance.OnGrabButtonDown();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        UserInput.instance.OnGrabButtonUp();
    }
}
