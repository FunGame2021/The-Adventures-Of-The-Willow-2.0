using UnityEngine;
using UnityEngine.VFX;

public class FinishPole : MonoBehaviour
{
    public static FinishPole instance;

    [Header("Visual Effects")]
    [SerializeField] private VisualEffect visualEffect;
    [SerializeField] private Animator anim;
    public bool isFinishPoleRightEnter;

    [Header("Timing Settings")]
    [SerializeField] private float initialDelay = 1f;
    [SerializeField] private float walkDuration = 3f;
    [SerializeField] private float fireworksDuration = 5f;

    private bool isFinishing = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && GameStates.instance.isLevelStarted && !isFinishing)
        {
            StartFinishSequence();
            Debug.Log("Touched pole");
        }
    }

    public void StartFinishSequence()
    {
        Debug.Log("Sequence");
        isFinishing = true;
        anim.SetBool("Winner", true);
        // Controle do player
        PlayerController.instance.StartFinishSequence(isFinishPoleRightEnter);

        // Efeitos sonoros
        AudioManager.instance.PlayOneShotNo3D(FMODEvents.instance.FinishMusic);
        LoadPlayLevel.instance?.StopMusic();
        playLevel.instance?.StopMusic();

        // Progressão do jogo
        PlayerManager.instance?.FinishLevelSave();
        PlayerManager.instance?.UpdateFinishLevelInfoTXT();
        LevelTimeManager.instance?.OnLevelCompleted();

        // Efeitos visuais
        visualEffect.Play();
        CameraZoom.instance.ZoomOutFinish();

        StartCoroutine(FinishRoutine());
    }

    private System.Collections.IEnumerator FinishRoutine()
    {
        yield return new WaitForSeconds(initialDelay);

        // Tempo de caminhada com zoom
        yield return new WaitForSeconds(walkDuration);

        // Finalização
        visualEffect.Stop();
        //CameraZoom.instance.ResetZoom();

        // Transição de cena
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if(LoadPlayLevel.instance != null)
        {
            LoadPlayLevel.instance.StopTestGameButton();
        }
        var sceneManager = GameObject.Find("SceneManager")?.GetComponent<LoadScenes>();
        sceneManager?.loadSceneEscapeButton();
    }
}