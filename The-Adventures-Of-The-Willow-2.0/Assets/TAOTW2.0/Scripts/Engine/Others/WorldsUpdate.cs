using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.IO.Compression;

public class WorldsUpdate : MonoBehaviour
{
    private string githubUrl = "https://raw.githubusercontent.com/jojocarlos/The-Adventures-Of-The-Willow-2.0/main/GameWorlds/ExtraWorldsVersionInfo.json";
    private string githubUrlWorlds = "https://raw.githubusercontent.com/jojocarlos/The-Adventures-Of-The-Willow-2.0/main/GameWorlds/Worlds.zip";

    private string inGameExtraWorlds = "Assets/StreamingAssets/";

    void Start()
    {
        StartCoroutine(CheckAndDownloadNewVersion());
    }

    IEnumerator CheckAndDownloadNewVersion()
    {
        Debug.Log("URL usada: " + githubUrl);

        // Configuração da solicitação UnityWebRequest com um tempo limite de 10 segundos
        UnityWebRequest webRequest = UnityWebRequest.Get(githubUrl);
        webRequest.timeout = 10;

        // Iniciar a solicitação
        yield return webRequest.SendWebRequest();

        // Verificar erros
        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Erro na solicitação: " + webRequest.error);
        }
        else
        {
            // Se a solicitação for bem-sucedida, imprimir o conteúdo da resposta
            Debug.Log("Conteúdo da resposta: " + webRequest.downloadHandler.text);
        }

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
        string githubFolderUrl = githubUrlWorlds;

        // Verificar se a pasta ZIP já existe localmente antes de baixar novamente
        string zipFilePath = Path.Combine(inGameExtraWorlds, localFolder + ".zip");
        if (!File.Exists(zipFilePath))
        {
            // Download the ZIP file from GitHub
            using (UnityWebRequest webRequest = UnityWebRequest.Get(githubFolderUrl))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Request error: " + webRequest.error);
                    yield break;
                }

                // Replace the local folder with the downloaded ZIP file
                ReplaceLocalFolder(localFolder, webRequest.downloadHandler.data, zipFilePath);
            }
        }
        else
        {
            Debug.Log("A versão mais recente já está baixada. Nenhuma ação necessária.");
        }
    }

    void ReplaceLocalFolder(string localFolder, byte[] folderData, string zipFilePath)
    {
        string localFolderPath = Path.Combine(inGameExtraWorlds, localFolder);

        // Extract and save the folder data
        File.WriteAllBytes(zipFilePath, folderData);

        // Extract the zip file directly into the existing local folder
        using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string entryPath = Path.Combine(localFolderPath, entry.FullName);

                // Create directories if they don't exist
                Directory.CreateDirectory(Path.GetDirectoryName(entryPath));

                // Extract the entry's contents
                entry.ExtractToFile(entryPath, true);
            }
        }

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
