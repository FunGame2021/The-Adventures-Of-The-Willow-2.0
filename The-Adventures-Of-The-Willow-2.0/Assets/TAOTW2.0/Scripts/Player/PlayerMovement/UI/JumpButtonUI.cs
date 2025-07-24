using UnityEngine;
using UnityEngine.EventSystems;

public class JumpButtonUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        UserInput.instance.OnJumpButtonDown();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        UserInput.instance.OnJumpButtonUp();
    }
}
