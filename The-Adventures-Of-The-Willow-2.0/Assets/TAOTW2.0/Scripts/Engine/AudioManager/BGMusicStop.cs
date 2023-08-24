using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMusicStop : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            AudioManager.instance.StopMusic(FMODEvents.instance.music);
            AudioManager.instance.SetMusicAreaParameter("GameMusic", 0);
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            AudioManager.instance.StopMusic(FMODEvents.instance.music);
            AudioManager.instance.SetMusicAreaParameter("GameMusic", 0);
        }
    }
}
