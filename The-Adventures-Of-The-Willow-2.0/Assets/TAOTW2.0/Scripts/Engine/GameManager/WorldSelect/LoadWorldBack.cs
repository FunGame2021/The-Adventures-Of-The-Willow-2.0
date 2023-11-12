using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadWorldBack : MonoBehaviour
{
    public static LoadWorldBack instance;
    private void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    //Chamado em botão exit do nivel e ou final do nível para voltar a carregar o mundo atual que estava
    public void WorldLoadAfterPlay()
    {
        if(PlayWorld.instance != null)
        {
            if (PlayWorld.instance.lastisWorldmap)
            {
                PlayWorld.instance.lastisWorldmap = false;
                PlayWorld.instance.isWorldmap = true;
                SceneManager.LoadScene("PlayWorld");
            }
            else
            {
                PlayWorld.instance.isWorldmap = false;
                if (PlayWorld.instance.isExtraLevels)
                {
                    SceneManager.LoadScene("ExtraLevelsList");
                }
                else if (PlayWorld.instance.isGameLevels)
                {
                    SceneManager.LoadScene("GameLevelsList");
                }
                else
                {
                    SceneManager.LoadScene("CommunityLevelsList");
                }
            }
        }
    }
}
