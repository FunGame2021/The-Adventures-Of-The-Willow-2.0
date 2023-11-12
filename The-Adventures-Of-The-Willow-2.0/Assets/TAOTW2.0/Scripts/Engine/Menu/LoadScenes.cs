using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class LoadScenes : MonoBehaviour
{
	public string nametoescape;
	
    public void loadScene(string name)
    {
        SceneManager.LoadScene(name);
    }
    public void loadSceneAdditive(string name)
    {
        SceneManager.LoadScene(name, LoadSceneMode.Additive);
    }

    public void QuitGame()
	{
		Application.Quit();
	}
	
	void Update()
    {
        if (UserInput.instance.playerMoveAndExtraActions.UI.EscapeMenu.WasPerformedThisFrame())
        {
            if (LoadWorldBack.instance != null)
            {
                LoadWorldBack.instance.WorldLoadAfterPlay();
            }
            else
            {
                SceneManager.LoadScene(nametoescape);
            }
		}
	}
	
	public void loadSceneEscapeButton()
    {
        if (LoadWorldBack.instance != null)
        {
            LoadWorldBack.instance.WorldLoadAfterPlay();
        }
        else
        {
            SceneManager.LoadScene(nametoescape);
        }
    }
	
}
