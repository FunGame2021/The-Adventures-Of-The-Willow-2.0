using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelTimeManager : MonoBehaviour
{
    public static LevelTimeManager instance;

    [SerializeField] private Image uiFill;
    [SerializeField] private TextMeshProUGUI uiTimerText;

    public int InitialDuration;
    public int remainingDuration;

    private bool isPaused; // Usado para controlar se o contador está pausado.

    private Coroutine timerCoroutine; // Usado para controlar a contagem de tempo.

    [SerializeField] private bool isNormalGame;

    //Level info save
    public float elapsedTime;


    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        //Call From Load Level
        //Being(Duration);
    }

    void Update()
    {
        if(remainingDuration <= 20)
        {
            if(isNormalGame)
            {
                playLevel.instance.SpeedMusic();
            }
            else
            {
                LoadPlayLevel.instance.SpeedMusic();
            }
        }
        else
        {
            if (isNormalGame)
            {

                playLevel.instance.SpeedMusicNormal();
            }
            else
            {

                LoadPlayLevel.instance.SpeedMusicNormal();
            }
        }
    }

    public void Begin(int seconds)
    {
        InitialDuration = seconds; // Atualiza a duração inicial
        remainingDuration = InitialDuration;
        elapsedTime = 0f; // Inicializa o tempo decorrido
        UpdateUITimer();
        StartTimer();
    }

    public void RestartTimer()
    {
        // Pare o contador de tempo atual antes de iniciar um novo.
        StopTimer();
        // Reinicia o contador de tempo para a duração inicial.
        Begin(InitialDuration);
    }

    private void StartTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        timerCoroutine = StartCoroutine(UpdateTimer());
    }

    private void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    public void PauseTimer()
    {
        isPaused = true;
    }

    public void ResumeTimer()
    {
        isPaused = false;
    }

    private IEnumerator UpdateTimer()
    {
        while (remainingDuration >= 0)
        {
            if (!isPaused)
            {
                elapsedTime += 1f; // Incrementa o tempo decorrido
                UpdateUITimer();
                remainingDuration--;
            }
            yield return new WaitForSeconds(1f);
        }
        OnEnd();
    }

    private void UpdateUITimer()
    {
        uiTimerText.text = $"{remainingDuration / 60:00} : {remainingDuration % 60:00}";
        uiFill.fillAmount = Mathf.InverseLerp(0, InitialDuration, remainingDuration);
    }
    public void OnLevelCompleted()
    {
        // Pare a contagem de tempo
        StopTimer();

    }


    private void OnEnd()
    {
        //End Time, if want do something
    }

}
