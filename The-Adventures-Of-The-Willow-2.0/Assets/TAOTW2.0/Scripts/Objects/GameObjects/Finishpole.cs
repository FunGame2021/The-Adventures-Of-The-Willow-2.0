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
            Finish();
            //anim.SetBool("Winner", true);
            PlayerController.instance.stopPlayer = true;
            //Stop game music play winner music

        }
    }

    public void Finish()
    {
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
