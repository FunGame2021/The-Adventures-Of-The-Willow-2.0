using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.IO;

public class GameGraphicsSettings : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Dropdown qualityDropdown;
    [SerializeField] private TMPro.TMP_Dropdown vSyncDropdown;

    private const string jsonFileName = "graphicsSettings.json";

    [System.Serializable]
    private class GraphicsSettingsData
    {
        public int qualityLevel;
        public int vSyncMode; // 0: Automatic, 1: On, 2: Off
    }

    private GraphicsSettingsData graphicsSettingsData = new GraphicsSettingsData();

    private void Awake()
    {
        qualityDropdown.onValueChanged.AddListener(new UnityAction<int>(index =>
        {
            graphicsSettingsData.qualityLevel = qualityDropdown.value;
            SaveGraphicsSettings();
        }));

        vSyncDropdown.onValueChanged.AddListener(new UnityAction<int>(index =>
        {
            graphicsSettingsData.vSyncMode = index;
            SaveGraphicsSettings();
            SetVSync(index);
        }));
    }

    private void Start()
    {
        LoadGraphicsSettings();
        qualityDropdown.value = graphicsSettingsData.qualityLevel;
        QualitySettings.SetQualityLevel(graphicsSettingsData.qualityLevel);

        vSyncDropdown.value = graphicsSettingsData.vSyncMode;
        SetVSync(graphicsSettingsData.vSyncMode);
    }
    private void SetVSync(int index)
    {
        QualitySettings.vSyncCount = index == 1 ? 1 : 0; // Ligado: 1, Desligado/Automático: 0
    }

    private void SaveGraphicsSettings()
    {
        string path = Application.persistentDataPath + "/Configurations";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        string json = JsonUtility.ToJson(graphicsSettingsData);
        File.WriteAllText(Application.persistentDataPath + "/Configurations/" + jsonFileName, json);
    }

    private void LoadGraphicsSettings()
    {
        string filePath = Application.persistentDataPath + "/Configurations/" + jsonFileName;
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            graphicsSettingsData = JsonUtility.FromJson<GraphicsSettingsData>(json);
        }
        else
        {
            graphicsSettingsData.qualityLevel = 3;
            graphicsSettingsData.vSyncMode = 0; // Defina um valor padrão para V-Sync (Automático)
            SaveGraphicsSettings();
        }
    }
}
