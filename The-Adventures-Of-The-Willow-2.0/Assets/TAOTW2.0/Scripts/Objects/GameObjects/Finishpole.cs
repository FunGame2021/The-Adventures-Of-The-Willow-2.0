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
    [HideInInspector] public bool enterRight;

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
            // Verifica a posição do jogador em relação à posição do colisor
            float playerPositionX = collision.transform.position.x;
            float finishPointPositionX = transform.position.x;

            if (playerPositionX < finishPointPositionX)
            {
                // O jogador entrou no colisor pelo lado esquerdo
                // Execute o código apropriado para essa situação
                enterRight = true;
            }
            else
            {
                // O jogador entrou no colisor pelo lado direito
                // Execute o código apropriado para essa situação
                enterRight = false;
            }
            if(GameStates.Instance.isNormalGame)
            {
                playLevel.instance.StopMusic();
            }
            else
            {
                LoadPlayLevel.instance.StopMusic();
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
