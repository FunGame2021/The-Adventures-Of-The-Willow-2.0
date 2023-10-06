using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;

    public int enemiesKilled;
    public int deaths;
    public float completionTime;
    public int coinsCollected;

    void Awake()
    {
       if(instance == null)
        {
            instance = this;
        }
    }

    // Chame este método quando o jogador completar o nível
    public void FinishLevelSave()
    {
        if (SaveGameManager.instance != null)
        {
            if (PlayWorld.instance != null)
            {
                SaveGameManager.instance.SetCurrentWorldAndLevel(PlayWorld.instance.selectedWorldName, PlayWorld.instance.selectedLevelName);
                SaveGameManager.instance.LevelCompleted();
            }
        }
    }

    // Chame este método quando o jogador matar um inimigo
    public void IncrementEnemiesKilled()
    {
        enemiesKilled++;
    }

    // Chame este método quando o jogador morrer
    public void IncrementDeaths()
    {
        deaths++;
    }

    // Chame este método quando o jogador coletar uma moeda
    public void IncrementCoinsCollected()
    {
        coinsCollected++;
    }


}
