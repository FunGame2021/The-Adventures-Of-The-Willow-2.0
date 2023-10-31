using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStates : MonoBehaviour
{
    public static GameStates instance;
    public bool isLevelStarted = false;
    public bool isNormalGame;

    void Awake()
    {
        isLevelStarted = false;
    }
    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

}
