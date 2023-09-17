using UnityEngine;
using System.IO;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager instance;

    private const int MaxProfiles = 4;
    private const string ProfileFolderName = "profiles";

    public string selectedProfile;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            //LoadSelectedProfile();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CreateProfile(int profileNumber)
    {
        string profilePath = GetProfilePath(profileNumber);

        if (!Directory.Exists(profilePath))
        {
            Directory.CreateDirectory(profilePath);
            Debug.Log("Profile " + profileNumber + " created.");
        }
        else
        {
            Debug.Log("Profile " + profileNumber + " already exists.");
        }
    }

    public void SelectProfile(int profileNumber)
    {
        string profilePath = GetProfilePath(profileNumber);

        if (!Directory.Exists(profilePath))
        {
            CreateProfile(profileNumber);
        }

        selectedProfile = profilePath;
        //SaveSelectedProfile();
        Debug.Log("Profile " + profileNumber + " selected.");
    }

    public void ResetProfile(int profileNumber)
    {
        string profilePath = GetProfilePath(profileNumber);

        if (Directory.Exists(profilePath))
        {
            Directory.Delete(profilePath, true);
            Debug.Log("Profile " + profileNumber + " reset.");
        }
        else
        {
            Debug.Log("Profile " + profileNumber + " does not exist.");
        }
    }

    //private void LoadSelectedProfile()
    //{
    //    selectedProfile = PlayerPrefs.GetString("SelectedProfile", "");
    //}

    //private void SaveSelectedProfile()
    //{
    //    PlayerPrefs.SetString("SelectedProfile", selectedProfile);
    //}

    private string GetProfilePath(int profileNumber)
    {
        return Path.Combine(Application.persistentDataPath, ProfileFolderName, "profile" + profileNumber);
    }
}


