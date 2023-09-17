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

    // Chame este m�todo quando o jogador completar o n�vel
    public void FinishLevelSave()
    {
        // Aqui voc� pode adicionar a l�gica para salvar os dados do jogador
        // Voc� pode acessar enemiesKilled, deaths, completionTime e coinsCollected aqui
        // e passar esses valores para o SaveGameManager.SaveGame() para salvar no arquivo.
        SaveGameManager.instance.SaveGame();
    }

    // Chame este m�todo quando o jogador matar um inimigo
    public void IncrementEnemiesKilled()
    {
        enemiesKilled++;
    }

    // Chame este m�todo quando o jogador morrer
    public void IncrementDeaths()
    {
        deaths++;
    }

    // Chame este m�todo quando o jogador coletar uma moeda
    public void IncrementCoinsCollected()
    {
        coinsCollected++;
    }


}
