using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using System.IO;

public class LocalizationSelector : MonoBehaviour
{
    public int intID = 0;
    private LocalizationConfig config;

    private void Start()
    {
        config = LoadConfig();
        int ID = config.selectedLocaleID;
        ChangeLocale(ID);
    }

    private bool active = false;

    public void ChangeLocale(int localeID)
    {
        if (active)
            return;
        StartCoroutine(SetLocale(localeID));
    }

    IEnumerator SetLocale(int _localeID)
    {
        active = true;
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[_localeID];
        intID = _localeID;
        active = false;

        SaveConfig();
    }

    private void SaveConfig()
    {
        string configPath = Path.Combine(Application.persistentDataPath, "Configurations", "LocalizationSettings.json");
        string configJson = JsonUtility.ToJson(config);
        File.WriteAllText(configPath, configJson);
    }

    private LocalizationConfig LoadConfig()
    {
        string configPath = Path.Combine(Application.persistentDataPath, "Configurations", "LocalizationSettings.json");
        if (File.Exists(configPath))
        {
            string configJson = File.ReadAllText(configPath);
            return JsonUtility.FromJson<LocalizationConfig>(configJson);
        }
        else
        {
            return new LocalizationConfig();
        }
    }
}

[System.Serializable]
public class LocalizationConfig
{
    public int selectedLocaleID;
}
