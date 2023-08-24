using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.IO;

public class ResolutionSettings : MonoBehaviour
{
    public TMPro.TMP_Dropdown ResolutionDropdown;
    public Toggle fullScreenToggle;

    private Resolution[] resolutions;
    private int currentResolutionIndex;
    private bool isFullScreen;

    private const string jsonFileName = "resolutionSettings.json";
    private ResolutionSettingsData resolutionSettingsData = new ResolutionSettingsData();

    [System.Serializable]
    private class ResolutionSettingsData
    {
        public int width;
        public int height;
        public bool isFullScreen;
    }

    private void Awake()
    {
        LoadResolutionSettings();

        ResolutionDropdown.onValueChanged.AddListener(new UnityAction<int>(index =>
        {
            SetResolution(index);
            SaveResolutionSettings();
        }));

        fullScreenToggle.onValueChanged.AddListener(new UnityAction<bool>(value =>
        {
            resolutionSettingsData.isFullScreen = value;
            SaveResolutionSettings();
            SetFullScreen(value);
        }));
    }

    private void Start()
    {
        resolutions = Screen.resolutions;

        ResolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == resolutionSettingsData.width &&
                resolutions[i].height == resolutionSettingsData.height)
            {
                currentResolutionIndex = i;
            }
        }

        ResolutionDropdown.AddOptions(options);
        ResolutionDropdown.value = currentResolutionIndex;
        ResolutionDropdown.RefreshShownValue();


        SetFullScreen(resolutionSettingsData.isFullScreen);
    }


    private void SetFullScreen(bool value)
    {
        isFullScreen = value;
        Screen.fullScreen = isFullScreen;
    }

    //Obsoleto
    //private void SetResolution(int index)
    //{
    //    Resolution resolution = resolutions[index];
    //    resolutionSettingsData.width = resolution.width;
    //    resolutionSettingsData.height = resolution.height;
    //    Screen.SetResolution(resolution.width, resolution.height, isFullScreen, resolution.refreshRate);
    //}
    private void SetResolution(int index)
    {
        Resolution resolution = resolutions[index];
        resolutionSettingsData.width = resolution.width;
        resolutionSettingsData.height = resolution.height;
        Screen.SetResolution(resolution.width, resolution.height, isFullScreen);
    }


    private void SaveResolutionSettings()
    {
        string path = Application.persistentDataPath + "/Configurations";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string json = JsonUtility.ToJson(resolutionSettingsData);
        File.WriteAllText(path + "/" + jsonFileName, json);
    }

    private void LoadResolutionSettings()
    {
        string filePath = Application.persistentDataPath + "/Configurations/" + jsonFileName;
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            resolutionSettingsData = JsonUtility.FromJson<ResolutionSettingsData>(json);
        }
        else
        {
            resolutionSettingsData.width = Screen.currentResolution.width;
            resolutionSettingsData.height = Screen.currentResolution.height;
            resolutionSettingsData.isFullScreen = Screen.fullScreen;

            SaveResolutionSettings();
        }
    }
}
