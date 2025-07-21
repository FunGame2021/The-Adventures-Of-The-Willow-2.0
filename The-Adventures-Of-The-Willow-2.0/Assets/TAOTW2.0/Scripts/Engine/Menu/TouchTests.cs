using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class TouchTests : MonoBehaviour
{
    void OnEnable()
    {
        // Ativa o modo EnhancedTouch (essencial para multitouch confiável)
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
        if (!Touchscreen.current?.enabled ?? true)
        {
            Debug.LogWarning("⚠️ Touchscreen.current está null ou desativado.");
            return;
        }

        if (Touch.activeTouches.Count == 0)
        {
            Debug.Log("📲 Nenhum toque detectado.");
        }
        else
        {
            foreach (var touch in Touch.activeTouches)
            {
                Debug.Log($"👉 Dedo {touch.finger.index} | {touch.phase} | Posição: {touch.screenPosition}");
            }
        }
    }
}
