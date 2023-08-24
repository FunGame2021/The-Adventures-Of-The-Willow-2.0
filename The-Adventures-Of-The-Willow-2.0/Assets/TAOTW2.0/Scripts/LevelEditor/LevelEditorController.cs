using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class LevelEditorController : MonoBehaviour
{
    public static LevelEditorController instance;

    [SerializeField] private string gameSceneName; // Nome da cena do jogo
    [SerializeField] private string levelEditorSceneName; // Nome da cena do editor de n�veis

    public bool isPlaying = false;

    public string AtualWorld;
    public string AtualLevel;

   // private bool canStartGame = true; // Vari�vel para controlar o atraso ap�s iniciar o jogo

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Update()
    {
        // Verifica o input do jogador para alternar entre reprodu��o e edi��o
        if (UserInput.instance.playerMoveAndExtraActions.UI.EnterPlayLevelEditor.WasPressedThisFrame())
        {
            if (isPlaying)
            {
                StopGame();
            }
            else
            {
                //Warn To Save Level After Test Game
                WarnStartGame();
            }
        }

    }

    private void WarnStartGame()
    {
        if (WorldManager.instance != null)
        {
            WorldManager.instance.WarnTestLevelPanel.SetActive(true);
        }
    }

    public void StartGame()
    {
        // Inicia o jogo
        isPlaying = true;

        // Carrega a cena do jogo
        SceneManager.LoadScene(gameSceneName);

        //Save security
        LevelEditorManager.instance.SaveLevel();;

        if (WorldManager.instance != null)
        {
            WorldManager.instance.WarnTestLevelPanel.SetActive(true);
        }
        AtualWorld = WorldManager.instance.currentWorldName;
        AtualLevel = WorldManager.instance.currentLevelName;

    }

    private void StopGame()
    {
        // Para o jogo
        isPlaying = false;

        // Carrega a cena do editor de n�veis
        SceneManager.LoadScene(levelEditorSceneName);

    }

 

    public void ExitLevelEditor()
    {
        Destroy(gameObject);
    }

    //Load Level On New Scene

}
