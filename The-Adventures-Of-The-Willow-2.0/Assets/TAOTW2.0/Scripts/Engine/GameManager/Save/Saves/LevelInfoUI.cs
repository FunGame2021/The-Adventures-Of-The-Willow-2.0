using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class LevelInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI uiLevelNameText;
    [SerializeField] private TextMeshProUGUI uiEnemiesKilledText;
    [SerializeField] private TextMeshProUGUI uiDeathsText;
    [SerializeField] private TextMeshProUGUI uiCompletionTimeText;
    [SerializeField] private TextMeshProUGUI uiCoinsCollectedText;
    private string FormatElapsedTime(float elapsedTimeInSeconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(elapsedTimeInSeconds);

        // Formata o tempo em minutos e segundos (por exemplo, "2:05" para 2 minutos e 5 segundos).
        string formattedTime = $"{timeSpan.Minutes}:{timeSpan.Seconds:00}";

        return formattedTime;
    }
    public void LoadLevelInfo()
    {
        if (SaveGameManager.instance != null)
        {
            // Encontre o n�vel atual no currentSaveWorldData
            CurrentSaveLevelData currentLevel = SaveGameManager.instance.currentSaveWorldData.levels.Find(level => level.CurrentLevelName == SaveGameManager.instance.currentLevelName);

            if (currentLevel != null)
            {
                // Aqui voc� pode atualizar o UI com as informa��es do n�vel
                // Por exemplo, voc� pode definir o texto de um objeto TextMeshProUGUI com os detalhes do n�vel.
                uiLevelNameText.text = currentLevel.CurrentLevelName;
                uiCoinsCollectedText.text = "Coins: " + currentLevel.currentLevelCoinsCollected.ToString();
                uiDeathsText.text = "Deaths: " + currentLevel.currentLevelDeaths.ToString();
                uiEnemiesKilledText.text = "Enemies Killed: " + currentLevel.currentLevelEnemiesKilled.ToString();

                // Converte o tempo decorrido em segundos para uma string formatada de minutos e segundos.
                string formattedTime = FormatElapsedTime(currentLevel.currentLevelCompletionTime);
                uiCompletionTimeText.text = "Completion Time: " + formattedTime;
            }
            else
            {
                Debug.LogWarning("N�vel atual n�o encontrado nos dados salvos.");
            }
        }
    }
}
