using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartObjects : MonoBehaviour
{
    public Vector3 initialPosition;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem[] puffParticles;
    private bool animStarted;
    private bool objectInRespawn = false;
    private float fallingTimer = 0f;
    private float fallingThreshold = -1f; // Ajuste este valor de acordo com a velocidade de queda que deseja considerar
    private Rigidbody2D rb;
    private float actualGravity;
    [SerializeField] private LayerMask targetLayers; // Defina a camada alvo no Inspector

    public bool isBeingGrabbed { get; set; } // Propriedade para rastrear o estado de agarrar
    bool isplayedSFX;
    bool isGrounded;
    bool isSimulated;

    void Start()
    {
        isSimulated = false;
        rb = GetComponent<Rigidbody2D>();
        actualGravity = rb.gravityScale;
        initialPosition = this.transform.position;
        animStarted = false;
    }
    public void restartObject()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // Define a velocidade vertical para zero
            rb.gravityScale = 0f;
        }
        objectInRespawn = true;
        // Define the color SpriteRenderer to alpha 0.
        Color newColor = spriteRenderer.color;
        newColor.a = 0f;
        spriteRenderer.color = newColor;
    }

    private void Update()
    {
        if(GameStates.instance.isLevelStarted)
        {
            if (playLevel.instance != null && !isSimulated)
            {
                if (playLevel.instance.isPlayingLevel)
                {
                    isSimulated = true;
                    rb.simulated = true;
                }
            }
            if (LoadPlayLevel.instance != null && !isSimulated)
            {
                if (LoadPlayLevel.instance.isPlayingLevel)
                {
                    isSimulated = true;
                    rb.simulated = true;
                }
            }
        }

        if (PlayerHealth.instance != null)
        {
            if (PlayerHealth.instance.isDead && !IsOverlapping(initialPosition))
            {
                restartObject();
            }
        }

        if (objectInRespawn && IsOverlapping(initialPosition))
        {
            restartObject();
            objectInRespawn = true;
        }

        if (!IsOverlapping(initialPosition) && objectInRespawn)
        {
            // Ajuste a posi��o para a posi��o inicial
            transform.position = initialPosition;
            rb.gravityScale = actualGravity;

            animStarted = true;
            animator.SetTrigger("Puff");

            objectInRespawn = false;
        }

        // Verifique se o objeto est� caindo
        if (IsFalling())
        {
            fallingTimer += Time.deltaTime;

            // Se o objeto estiver caindo por mais de 10 segundos, fa�a respawn
            if (fallingTimer >= 10f)
            {
                restartObject();
            }
        }
        else
        {
            // Reinicie o temporizador se o objeto n�o estiver mais caindo
            fallingTimer = 0f;
        }

        if(rb.linearVelocity.y < -3 && !isBeingGrabbed && rb.linearVelocity.y != 0 && !isplayedSFX && !isGrounded)
        {
            isplayedSFX = true;
            AudioManager.instance.PlayOneShot(FMODEvents.instance.FallingObject, this.transform.position);
        }
        if(rb.linearVelocity.y == 0)
        {
            isplayedSFX = false;
        }
        if(isBeingGrabbed)
        {
            isGrounded = false;
        }
    }

    //Call on animation 
    public void startPuffParticles()
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.PoofObject, this.transform.position);
        // Iterate through the puffParticles array and play each ParticleSystem
        foreach (ParticleSystem particles in puffParticles)
        {
            particles.Play();
        };
    }
    public void toOriginalColor()
    {
        Color newBColor = spriteRenderer.color;
        newBColor.a = 1f;
        spriteRenderer.color = newBColor;
        animStarted = false;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verifica se a camada do objeto colidido est� na LayerMask targetLayers
        if (collision.gameObject.CompareTag("ground") || collision.gameObject.CompareTag("ObjectObject") && !isBeingGrabbed)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.PoofObject, this.transform.position);
            
            foreach (ParticleSystem particles in puffParticles)
            {
                particles.Play();
            };
        }

        int collisionLayer = collision.gameObject.layer; // Obt�m a camada do objeto colidido

        // Verifique se a camada do objeto colidido est� na LayerMask targetLayers
        if (targetLayers == (targetLayers | (1 << collisionLayer)) && !isBeingGrabbed)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.PoofObject, this.transform.position);

            foreach (ParticleSystem particles in puffParticles)
            {
                particles.Play();
            };
        }
        if (collision.gameObject.CompareTag("immediate_death"))
        {
            restartObject();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("ground") || collision.gameObject.CompareTag("ObjectObject"))
        {
            isGrounded = true;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("ground") || collision.gameObject.CompareTag("ObjectObject"))
        {
            isGrounded = false;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("immediate_death"))
        {
            restartObject();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("immediate_death"))
        {
            restartObject();
        }
    }

    private bool IsOverlapping(Vector3 positionToCheck)
    {
        // Obt�m o colisor da caixa atual
        BoxCollider2D myCollider = GetComponent<BoxCollider2D>();

        if (myCollider == null)
        {
            // Se o objeto n�o tiver um colisor de caixa, n�o h� sobreposi��o
            return false;
        }

        // Calcula o tamanho da caixa com base no tamanho do colisor
        Vector2 boxSize = new Vector2(myCollider.size.x, myCollider.size.y);

        // Verifica se h� algum colisor na �rea da caixa de sobreposi��o
        Collider2D[] colliders = Physics2D.OverlapBoxAll(positionToCheck, boxSize, 0f);

        // Remove o pr�prio colisor (caso exista)
        List<Collider2D> filteredColliders = new List<Collider2D>(colliders);
        filteredColliders.RemoveAll(c => c.gameObject == gameObject);
        filteredColliders.RemoveAll(c => c.gameObject.name == "CameraConfiner");

        // Retorna verdadeiro se ainda houver colisores na lista
        return filteredColliders.Count > 0;
    }
    private bool IsFalling()
    {
        // Verifica a velocidade vertical do objeto
        if (rb != null && rb.linearVelocity.y < fallingThreshold)
        {
            return true;
        }

        return false;
    }

}
