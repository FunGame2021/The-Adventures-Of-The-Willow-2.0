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

    public int Duration;
    public int remainingDuration;

    private bool Pause;

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
        if(remainingDuration == 20)
        {
            LoadPlayLevel.instance.SpeedMusic();
        }
    }

    public void Being(int Second)
    {
        remainingDuration = Second;
        StartCoroutine(UpdateTimer());
    }

    private IEnumerator UpdateTimer()
    {
        while(remainingDuration >= 0)
        {
            if (!Pause)
            {
                uiTimerText.text = $"{remainingDuration / 60:00} : {remainingDuration % 60:00}";
                uiFill.fillAmount = Mathf.InverseLerp(0, Duration, remainingDuration);
                remainingDuration--;
                yield return new WaitForSeconds(1f);
            }
            yield return null;
        }
        OnEnd();
    }

    private void OnEnd()
    {
        //End Time, if want do something
    }

}
