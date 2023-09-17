using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStates : MonoBehaviour
{
    public static GameStates Instance;
    public bool isLevelEditor;
    public bool isNormalGame;

    void Start()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

}
