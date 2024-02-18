using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    //same KeyID;
    public string keyID;

    private bool isFollowing;

    public float followSpeed;

    public Transform followTarget;
    public Animator Anim;
    public bool keyDoorOpened;

    private Vector3 targetPosition;
    private Vector3 currentVelocity;

    public float smoothTime = 0.3f;
    public float delay = 1.0f;  // Atraso em segundos


    void Start()
    {
        Anim.SetBool("PlayerFollow", false);
    }

    void Update()
    {
        if (isFollowing)
        {
            // Posi��o atrasada em rela��o � posi��o do jogador
            Vector3 delayedPosition = followTarget.position - followTarget.forward * delay;

            // Suaviza��o exponencial para a posi��o
            targetPosition = Vector3.SmoothDamp(targetPosition, delayedPosition, ref currentVelocity, smoothTime);

            // Aplica a posi��o suavizada � chave
            transform.position = targetPosition;
        }
        if (keyDoorOpened)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isFollowing)
            {
                KeyFollow thePlayer = FindObjectOfType<KeyFollow>();
                Anim.SetTrigger("PlayerFollow");
                followTarget = thePlayer.KeyFollowPoint;
                isFollowing = true;
                thePlayer.AddKey(this);
            }
        }
    }
}

