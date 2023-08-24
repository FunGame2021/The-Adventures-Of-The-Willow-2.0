using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScenesAdditive : MonoBehaviour
{
    public string nextSceneName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!SceneManager.GetSceneByName(nextSceneName).isLoaded)
            {
                SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
            }
        }
    }
}
