using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class LevelEditorController : MonoBehaviour
{
    public static LevelEditorController instance;

    [SerializeField] private string gameSceneName; // Nome da cena do jogo
    [SerializeField] private string levelEditorSceneName; // Nome da cena do editor de níveis

    public bool isPlaying = false;

    public string AtualWorld;
    public string AtualLevel;

    string currentSceneName;
    // private bool canStartGame = true; // Variável para controlar o atraso após iniciar o jogo

    private void Awake()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    private void Start()
    {
        if (currentSceneName != "LevelEditor" && currentSceneName != "LevelEditorPlayTest")
        {
            DestroyThis();
        }
    }
    public void DestroyThis()
    {
        DestroyImmediate(gameObject);
    }
    public void TestGamePlay()
    {
        if (LevelEditorManager.instance != null)
        {
            if (LevelEditorManager.instance.CanPlayLevel)
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
        else
        {
            if (isPlaying)
            {
                StopGame();
            }
        }
    }
    private void Update()
    {
        // Verifica o input do jogador para alternar entre reprodução e edição
        if (UserInput.instance.playerMoveAndExtraActions.UI.LeftCTRL.IsPressed() && UserInput.instance.playerMoveAndExtraActions.UI.EnterPlayLevelEditor.WasPressedThisFrame())
        {
            if (LevelEditorManager.instance != null)
            {
                if (LevelEditorManager.instance.CanPlayLevel)
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
            else
            {
                if (isPlaying)
                {
                    StopGame();
                }
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

        // Carrega a cena do editor de níveis
        SceneManager.LoadScene(levelEditorSceneName);

    }

 

    public void ExitLevelEditor()
    {
        Destroy(gameObject);
    }


}
