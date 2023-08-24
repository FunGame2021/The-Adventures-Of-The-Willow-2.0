using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class OnScreenButtonsSettings : MonoBehaviour
{
    public Toggle enableTouchButtonsToggle;

    private const string jsonFileName = "OnScreenButtonsSettings.json";

    [System.Serializable]
    public class OnScreenButtonsSettingsData
    {
        public bool enableTouchButtonsToggle;
    }

    public OnScreenButtonsSettingsData onScreenButtonsSettingsData = new OnScreenButtonsSettingsData();

    void Start()
    {
        // Carrega as configurações salvas
        LoadOnScreenButtonsSettings();

        // Adiciona um listener para detectar quando o toggle é alterado
        enableTouchButtonsToggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    public void OnToggleValueChanged(bool newValue)
    {
        // Atualiza as configurações e salva
        onScreenButtonsSettingsData.enableTouchButtonsToggle = newValue;
        SaveOnScreenButtonsSettings();
    }

    private void SaveOnScreenButtonsSettings()
    {
        string path = Application.persistentDataPath + "/Configurations";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        string json = JsonUtility.ToJson(onScreenButtonsSettingsData);
        File.WriteAllText(Application.persistentDataPath + "/Configurations/" + jsonFileName, json);
    }

    private void LoadOnScreenButtonsSettings()
    {
        string filePath = Application.persistentDataPath + "/Configurations/" + jsonFileName;
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            onScreenButtonsSettingsData = JsonUtility.FromJson<OnScreenButtonsSettingsData>(json);
        }
        else
        {
            onScreenButtonsSettingsData.enableTouchButtonsToggle = true;
            SaveOnScreenButtonsSettings();
        }

        // Atualiza a interface do usuário com as configurações carregadas
        enableTouchButtonsToggle.isOn = onScreenButtonsSettingsData.enableTouchButtonsToggle;
    }
}
