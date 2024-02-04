using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateCursor : MonoBehaviour
{
    void Update()
    {
        CursorManager.instance.cursorAppear();
    }
}
