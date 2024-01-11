using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;
using UnityEngine.VFX;
using static Unity.Collections.AllocatorManager;

public class FinishPole : MonoBehaviour
{
    public static FinishPole instance;
    [SerializeField] private VisualEffect visualEffect;
    [SerializeField] private Animator anim;
    public bool isFinishPoleRightEnter;

    private void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if(GameStates.instance.isLevelStarted)
            {
                if (LoadPlayLevel.instance != null)
                {
                    LoadPlayLevel.instance.StopMusic();
                }
                if (playLevel.instance != null)
                {
                    playLevel.instance.StopMusic();
                }
            }
            PlayEndMusic();
            Finish();
            //anim.SetBool("Winner", true);
            PlayerController.instance.stopPlayer = true;
            //Stop game music play winner music

        }
    }

    private void PlayEndMusic()
    {
        AudioManager.instance.PlayOneShotNo3D(FMODEvents.instance.FinishMusic);
    }
    public void Finish()
    {
        if(PlayerManager.instance != null)
        {
            PlayerManager.instance.FinishLevelSave();
            PlayerManager.instance.UpdateFinishLevelInfoTXT();
        }
        if(LevelTimeManager.instance != null)
        {
            LevelTimeManager.instance.OnLevelCompleted();
        }
        FinishPoint.instance.FinishSequence();
        PlayFireworksEffect();
    }

    public void PlayFireworksEffect()
    {
        visualEffect.Play();
    }

    public void StopFireworksEffect()
    {
        visualEffect.Stop();
    }
}
