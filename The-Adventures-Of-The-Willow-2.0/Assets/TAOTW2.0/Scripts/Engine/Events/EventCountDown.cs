using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EventCountDown : MonoBehaviour
{
    public TextMeshProUGUI EventName;
    public TextMeshProUGUI Time;
    public DateTime eventDate;
    public int Year;
    public int Month;
    public int Day;
    public int Hour;
    public int Minutes;
    public int Seconds;

    private void Start()
    {
        eventDate = new DateTime(Year, Month, Day, Hour, Minutes, Seconds, 0);
    }

    private void Update()
    {
        EventName.text = RemoteConfig.Instance.EventName;
        TimeSpan diff = eventDate - DateTime.Now;

        if (diff.Ticks < 0)
        {
            diff = TimeSpan.Zero;
        }

        string r = "";

        if (diff.TotalDays >= 30) // Mais de um mês
        {
            r += ((int)(diff.TotalDays / 30)).ToString() + "M";
        }
        else if (diff.TotalDays >= 1) // Mais de um dia
        {
            r += ((int)diff.TotalDays).ToString("00") + "D";
        }
        else // Menos de um dia
        {
            r += " ";
        }
        /*/ Show year reference
    if (diff.TotalDays >= 365) // Mais de um ano
    {
        r += ((int)(diff.TotalDays / 365)).ToString() + "Y";
    }
    else if (diff.TotalDays >= 30) // Mais de um mês
    {
        r += ((int)(diff.TotalDays / 30)).ToString() + "M";
    }
    else if (diff.TotalDays >= 1) // Mais de um dia
    {
        r += ((int)diff.TotalDays).ToString("00") + "D";
    }*/

        r += " ";

        r += diff.Hours.ToString("00") + "h ";
        r += diff.Minutes.ToString("00") + "m ";
        r += diff.Seconds.ToString("00") + "s";

        Time.text = r;
    }
}

