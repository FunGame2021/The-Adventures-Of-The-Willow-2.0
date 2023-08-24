using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaitBeforePlay : MonoBehaviour
{
    [SerializeField] private string UnloadLoadScene;
    void Start()
    {
        StartCoroutine(BeforePlay());
    }

    IEnumerator BeforePlay()
    {
        //Time.timeScale = 0f;
    Scene unloadScene = SceneManager.GetSceneByName(UnloadLoadScene);
    if (unloadScene.isLoaded)
    {
        //SceneManager.UnloadSceneAsync(UnloadLoadScene);
    }
	float startTime = Time.realtimeSinceStartup;
    while (Time.realtimeSinceStartup - startTime < 7f)
    {
        yield return null;
    }
        //Time.timeScale = 1f;
		Debug.Log("Time1");
    }
}
