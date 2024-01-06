using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public int enemiesKilled;
    public int deaths;
    public float completionTime;
    public int coinsCollected;

    //Info Texts
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TextMeshProUGUI enemiesKilledTXT;
    [SerializeField] private TextMeshProUGUI deathsTXT;
    [SerializeField] private TextMeshProUGUI coinsCollectedTXT;
    [SerializeField] private TextMeshProUGUI completionTimeTXT;

    void Awake()
    {
        if (instance == null)
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

    public void UpdateFinishLevelInfoTXT()
    {
        infoPanel.SetActive(true);
        enemiesKilledTXT.text = enemiesKilled.ToString();
        coinsCollectedTXT.text = coinsCollected.ToString();
        deathsTXT.text = deaths.ToString();
        completionTimeTXT.text = completionTime.ToString();
    }

    // Chame este método quando o jogador matar um inimigo
    public void IncrementEnemiesKilled()
    {
        enemiesKilled +=1;
    }

    // Chame este método quando o jogador morrer
    public void IncrementDeaths()
    {
        deaths++;
    }

    // Chame este método quando o jogador coletar uma moeda
    public void IncrementCoinsCollected(int coinValue)
    {
        coinsCollected += coinValue;
    }


}
