using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.IO.Compression;

public class WorldsUpdate : MonoBehaviour
{
    private string githubUrl = "https://raw.githubusercontent.com/jojocarlos/The-Adventures-Of-The-Willow-2.0/main/Assets/StreamingAssets/Worlds/ExtraWorlds/";
    private string inGameExtraWorlds = "Assets/StreamingAssets/Worlds/";

    void Start()
    {
        StartCoroutine(CheckAndDownloadNewVersion());
    }

    IEnumerator CheckAndDownloadNewVersion()
    {
        // Verificar se o arquivo JSON local existe
        string localVersionFilePath = Path.Combine(inGameExtraWorlds, "ExtraWorldsVersion");
        if (!File.Exists(localVersionFilePath))
        {
            // Se o arquivo não existir, baixe a nova versão
            yield return StartCoroutine(DownloadReplaceFolderFromGitHub("ExtraWorlds", "Worlds"));
        }
        else
        {
            // Se o arquivo existir, compare versões e tome ação, se necessário
            // Chame diretamente o método CompareVersionsAndDownload
            yield return CompareVersionsAndDownload();
        }
    }

    IEnumerator CompareVersionsAndDownload()
    {
        // Get the local version of ExtraWorlds
        ExtraWorldsVersion localExtraWorldsVersion = GetLocalExtraWorldsVersion();

        // Get the version of ExtraWorlds on GitHub
        ExtraWorldsVersion githubVersion = (ExtraWorldsVersion)GetGitHubExtraWorldsVersion();

        // Compare versions and take action if necessary
        if (IsLocalVersionLower(localExtraWorldsVersion, githubVersion))
        {
            // Local version is lower, so download the folder from GitHub and replace
            yield return StartCoroutine(DownloadReplaceFolderFromGitHub("ExtraWorlds", "Worlds"));
        }
    }
    ExtraWorldsVersion GetLocalExtraWorldsVersion()
    {
        string localVersionFilePath = Path.Combine(inGameExtraWorlds, "ExtraWorldsVersion");
        if (File.Exists(localVersionFilePath))
        {
            string jsonLocalVersion = File.ReadAllText(localVersionFilePath);
            return JsonUtility.FromJson<ExtraWorldsVersion>(jsonLocalVersion);
        }
        return new ExtraWorldsVersion("0.0.0"); // Or another default version if no file is found
    }

    IEnumerator GetGitHubExtraWorldsVersion()
    {
        string githubVersionUrl = githubUrl + "ExtraWorldsVersion";
        UnityWebRequest webRequest = UnityWebRequest.Get(githubVersionUrl);

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Request error: " + webRequest.error);
            yield break;
        }

        // Agora, retorne o resultado ou o manipule como necessário
        var githubVersion = JsonUtility.FromJson<ExtraWorldsVersion>(webRequest.downloadHandler.text);

        // Retorne a versão obtida
        yield return githubVersion;
    }

    bool IsLocalVersionLower(ExtraWorldsVersion localVersion, ExtraWorldsVersion remoteVersion)
    {
        // Compare versions here
        // If the local version is lower, return true; otherwise, return false
        return CompareVersion(localVersion, remoteVersion) < 0;
    }

    int CompareVersion(ExtraWorldsVersion version1, ExtraWorldsVersion version2)
    {
        // Implement version comparison logic here
        // For example, you can split versions into parts (major, minor, revision) and compare each part
        return string.Compare(version1.version, version2.version);
    }

    IEnumerator DownloadReplaceFolderFromGitHub(string githubFolder, string localFolder)
    {
        string githubFolderUrl = githubUrl + githubFolder;

        // Download the folder from GitHub
        using (UnityWebRequest webRequest = UnityWebRequest.Get(githubFolderUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Request error: " + webRequest.error);
                yield break;
            }

            // Replace the local folder with the downloaded data
            ReplaceLocalFolder(localFolder, webRequest.downloadHandler.data);
        }
    }

    void ReplaceLocalFolder(string localFolder, byte[] folderData)
    {
        string localFolderPath = Path.Combine(inGameExtraWorlds, localFolder);

        // Delete the local folder if it exists
        if (Directory.Exists(localFolderPath))
            Directory.Delete(localFolderPath, true);

        // Create the new local folder
        Directory.CreateDirectory(localFolderPath);

        // Extract and save the folder data
        string zipFilePath = Path.Combine(inGameExtraWorlds, localFolder + ".zip");
        File.WriteAllBytes(zipFilePath, folderData);

        // Extract the zip file (you may need a third-party library for this)
        // Example using System.IO.Compression:
        ZipFile.ExtractToDirectory(zipFilePath, localFolderPath);

        // Delete the zip file after extraction, if desired
        File.Delete(zipFilePath);
    }
}

[System.Serializable]
public class ExtraWorldsVersion
{
    public string version;

    public ExtraWorldsVersion(string version)
    {
        this.version = version;
    }
}
