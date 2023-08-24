using FMOD.Studio;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMusicManager : MonoBehaviour
{

    private EventInstance currentMenuMusicInstance;

    public List<string> menuSceneNames; // Lista de nomes das cenas de menu
    private string currentSceneName;
    private bool isPlaying;

    private static MenuMusicManager instance;

    void Awake()
    {
        // Garante que apenas uma instância do MenuMusicManager exista
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
    }
    private void Start()
    {
        currentSceneName = SceneManager.GetActiveScene().name;

        if (menuSceneNames.Contains(currentSceneName) && !isPlaying)
        {
            currentMenuMusicInstance = FMODUnity.RuntimeManager.CreateInstance(FMODEvents.instance.MenuMusicA);
            currentMenuMusicInstance.start();
            isPlaying = true;
        }
    }

    private void Update()
    {
        currentSceneName = SceneManager.GetActiveScene().name;

        if (menuSceneNames.Contains(currentSceneName) && !isPlaying)
        {
            currentMenuMusicInstance = FMODUnity.RuntimeManager.CreateInstance(FMODEvents.instance.MenuMusicA);
            currentMenuMusicInstance.start();
            isPlaying = true;
        }
        if (currentMenuMusicInstance.isValid() && isPlaying && !menuSceneNames.Contains(currentSceneName))
        {
            currentMenuMusicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            currentMenuMusicInstance.release(); // Libera a instância
            isPlaying = false;
        }

    }

}

