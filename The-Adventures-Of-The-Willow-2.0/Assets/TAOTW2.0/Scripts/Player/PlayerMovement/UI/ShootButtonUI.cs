using UnityEngine;
using UnityEngine.EventSystems;

public class ShootButtonUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        UserInput.instance.OnShootButtonDown();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        UserInput.instance.OnShootButtonUp();
    }
}
