using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateCursor : MonoBehaviour
{
    void Update()
    {
        if (Application.platform != RuntimePlatform.Android ||
    Application.platform != RuntimePlatform.IPhonePlayer)
        {
            CursorManager.instance.cursorAppear();
        }
    }
}
