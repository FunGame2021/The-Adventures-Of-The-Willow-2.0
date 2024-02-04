using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class OptionsMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject escapeMenu;
    [SerializeField] private GameObject volumeObj;


    void Start()
    {
        escapeMenu.SetActive(false);
        volumeObj.SetActive(false);
    }
    void Update()
    {
        if (UserInput.instance.playerMoveAndExtraActions.UI.EscapeMenu.WasPerformedThisFrame())
        {
            if (escapeMenu.activeSelf)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        volumeObj.SetActive(false);
        Time.timeScale = 1;
        escapeMenu.SetActive(false);
        CursorManager.instance.cursorDisappear();
    }

    private void Pause()
    {
        Time.timeScale = 0;
        escapeMenu.SetActive(true);
        volumeObj.SetActive(true);
        CursorManager.instance.cursorAppear();
    }

}
