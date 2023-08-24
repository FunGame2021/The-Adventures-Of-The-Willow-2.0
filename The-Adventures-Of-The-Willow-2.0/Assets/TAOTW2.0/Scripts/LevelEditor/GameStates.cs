using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStates : MonoBehaviour
{
    public static GameStates Instance;
    public bool isLevelEditor;
    void Start()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

}
