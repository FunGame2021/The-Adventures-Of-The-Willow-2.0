using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GeneralSaveData : MonoBehaviour
{
    public static GeneralSaveData instance;

    private string GeneralSave = "YourSaveFileName"; // Change this to your desired save file name
    private SaveAllData saveData = new SaveAllData(); // Create an instance of SaveAllData

    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        // Load saved data when the game starts (if needed)
        GenLoadData();
    }

    private string GetSavePath()
    {
        // Get the path for saving data
        string saveFolder = Path.Combine(Application.persistentDataPath, ProfileManager.instance.selectedProfile);
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }
        return Path.Combine(saveFolder, GeneralSave + ".json"); // Use .json as the file extension
    }

    public void GenSaveData()
    {
        // Update the data you want to save
        saveData.gems = 10; // Example: Set the number of gems to 10

        // Convert the data to JSON format
        string jsonToSave = JsonUtility.ToJson(saveData, true);

        // Save the JSON data to a file
        File.WriteAllText(GetSavePath(), jsonToSave);
    }

    public void GenLoadData()
    {
        // Load the JSON data from the file (if it exists)
        string filePath = GetSavePath();
        if (File.Exists(filePath))
        {
            string jsonLoaded = File.ReadAllText(filePath);

            // Deserialize the JSON data back to your SaveAllData class
            saveData = JsonUtility.FromJson<SaveAllData>(jsonLoaded);

            // Now you can access the loaded data, e.g., saveData.gems
        }
    }
}

[System.Serializable]
public class SaveAllData
{
    public int gems;
}
