using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnyKeyPressed : MonoBehaviour
{
    public static AnyKeyPressed instance;
    [SerializeField] private GameObject textPressed;
    [SerializeField] private GameObject Buttons;
    [SerializeField] private Animator Anim;

    private bool gameStarted = false;
    private const string GAME_STARTED_KEY = "gameStarted";
	
    void Awake()
    {
        if(instance == null)
        { 
            instance = this; 
            DontDestroyOnLoad(gameObject); // Impede que o objeto seja destruído ao carregar uma nova cena
        }
        else
        {
            Destroy(gameObject); // Destroi objetos adicionais que não são a instância principal
        }

        Time.timeScale = 1;
        if (PlayerPrefs.GetInt(GAME_STARTED_KEY, 0) == 1)
        {
            gameStarted = true;
            textPressed.SetActive(false);
            Buttons.SetActive(true);
            Anim.SetBool("entered", true);
        }
        else
        {
            PlayerPrefs.SetInt(GAME_STARTED_KEY, 0);
            gameStarted = false;
            textPressed.SetActive(true);
            Buttons.SetActive(false);
            Anim.SetBool("entered", false);
        }
    }

    void Update()
    {
        if ((Keyboard.current.anyKey.isPressed || Input.anyKey || Input.touchCount > 0) && !gameStarted)
        {
            textPressed.SetActive(false);
            Buttons.SetActive(true);
            Anim.SetBool("entered", false);

            PlayerPrefs.SetInt(GAME_STARTED_KEY, 1);
            gameStarted = true;
        }
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey(GAME_STARTED_KEY);
    }

}
