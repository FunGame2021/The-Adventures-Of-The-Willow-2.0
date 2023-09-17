using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayWorld : MonoBehaviour
{
    public static PlayWorld instance;
    public string selectedWorldName;
    public string selectedLevelName;
    public bool isWorldmap;

    //Game and extra levels game
    public bool isExtraLevels;
    public bool isGameLevels;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public void ExitPlayLevel()
    {
        Destroy(gameObject);
    }

}
