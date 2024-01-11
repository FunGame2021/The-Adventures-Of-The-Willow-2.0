using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;


[System.Serializable]
public class SettingsData
{
    public bool autoSave;
    public int saveIntervalIndex; // Index representing the selected interval in the dropdown
}

public class AutoSaveManager : MonoBehaviour
{
    public static AutoSaveManager instance;

    public bool autoSave;
    private int saveIntervalIndex;
    [SerializeField] private TMP_Dropdown saveIntervalDropdown;
    [SerializeField] private Toggle autoSaveToggle;

    private string configFilePath;

    private void Awake()
    {
        if(instance == null)
        { 
            instance = this;
        }
    }
    private void Start()
    {
        // Set the path for the configurations folder within persistentDataPath
        configFilePath = Path.Combine(Application.persistentDataPath, "Configurations");


        // Example: Attach the method to the Toggle's onValueChanged event
        autoSaveToggle.onValueChanged.AddListener(OnAutoSaveToggleValueChanged); 
        saveIntervalDropdown.onValueChanged.AddListener(OnSaveIntervalDropdownValueChanged);

        // Load settings when the game starts
        LoadSettings();

        // Set up the dropdown with interval values
        SetupDropdown();
    }
    public void UpdateLEUISettings()
    {
        // Load settings when the game starts
        LoadSettings();

        // Set up the dropdown with interval values
        SetupDropdown();
    }
    private void SetupDropdown()
    {
        // Clear existing options
        saveIntervalDropdown.ClearOptions();

        // Array of interval values in minutes
        float[] intervalValues = { 1f, 3f, 5f, 10f, 15f, 20f, 25f, 30f };

        // Convert values to strings and add them to the dropdown options
        List<string> options = new List<string>();
        foreach (float interval in intervalValues)
        {
            options.Add($"{interval} minutes");
        }

        saveIntervalDropdown.AddOptions(options);
    }

    void OnAutoSaveToggleValueChanged(bool value)
    {
        // Update the autoSave variable based on the toggle value
        autoSave = value;

        // Save the settings whenever the toggle value changes
        SaveSettings();
    }
    void OnSaveIntervalDropdownValueChanged(int value)
    {
        // Update the saveIntervalIndex based on the dropdown selection
        saveIntervalIndex = value;

        // Save the settings whenever the dropdown value changes
        SaveSettings();
    }
    void SaveSettings()
    {
        // Create the Configurations folder if it doesn't exist
        if (!Directory.Exists(configFilePath))
        {
            Directory.CreateDirectory(configFilePath);
        }

        // Create a SettingsData instance and populate it with the current settings
        SettingsData settingsData = new SettingsData
        {
            autoSave = autoSave,
            saveIntervalIndex = saveIntervalIndex
        };
        if(LevelEditorManager.instance != null)
        {
            LevelEditorManager.instance.autoSaveTime = GetAutoSaveInterval();
        }
        // Convert the SettingsData to a JSON string
        string json = JsonUtility.ToJson(settingsData);

        // Define the file path for the settings file
        string filePath = Path.Combine(configFilePath, "LevelEditorSettings.json");

        // Write the JSON string to the file
        File.WriteAllText(filePath, json);
    }

    void LoadSettings()
    {
        // Define the file path for the settings file
        string filePath = Path.Combine(configFilePath, "LevelEditorSettings.json");

        // Check if the settings file exists
        if (File.Exists(filePath))
        {
            // Read the JSON string from the file
            string json = File.ReadAllText(filePath);

            // Deserialize the JSON string into a SettingsData instance
            SettingsData settingsData = JsonUtility.FromJson<SettingsData>(json);

            // Set the autoSave variable and dropdown value based on the loaded settings
            autoSave = settingsData.autoSave;
            saveIntervalIndex = settingsData.saveIntervalIndex;

            // Update the toggle value
            autoSaveToggle.isOn = autoSave;

            // Find the index that corresponds to the loaded time
            saveIntervalIndex = FindIndexForTime(GetAutoSaveInterval());

            // Update the dropdown value
            saveIntervalDropdown.value = saveIntervalIndex;

            // Apply the selected time interval to your game logic
            ApplySaveInterval();
        }
    }
    int FindIndexForTime(float timeInSeconds)
    {
        float[] intervalValues = { 1f, 3f, 5f, 10f, 15f, 20f, 25f, 30f }; // Valores correspondentes aos intervalos em minutos

        for (int i = 0; i < intervalValues.Length; i++)
        {
            if (Mathf.Approximately(intervalValues[i] * 60f, timeInSeconds))
            {
                return i;
            }
        }

        // Valor padrão, caso não encontre uma correspondência exata
        return 0;
    }
    public float GetAutoSaveInterval()
    {
        float[] intervalValues = { 1f, 3f, 5f, 10f, 15f, 20f, 25f, 30f }; // Valores correspondentes aos intervalos em minutos
        return intervalValues[saveIntervalIndex] * 60f; // Retorna o intervalo em segundos
    }
    void ApplySaveInterval()
    {
        // Example: Implement your logic to apply the selected time interval
        switch (saveIntervalIndex)
        {
            case 0: // 1 minute
                Debug.Log("Auto-save interval set to 1 minute");
                break;
            case 1: // 3 minutes
                Debug.Log("Auto-save interval set to 3 minutes");
                break;
            case 2: // 5 minutes
                Debug.Log("Auto-save interval set to 5 minutes");
                break;
            case 3: // 10 minutes
                Debug.Log("Auto-save interval set to 10 minutes");
                break;
            case 4: // 15 minutes
                Debug.Log("Auto-save interval set to 15 minutes");
                break;
            case 5: // 20 minutes
                Debug.Log("Auto-save interval set to 20 minutes");
                break;
            case 6: // 25 minutes
                Debug.Log("Auto-save interval set to 25 minutes");
                break;
            case 7: // 30 minutes
                Debug.Log("Auto-save interval set to 30 minutes");
                break;
            default:
                break;
        }
    }
}
